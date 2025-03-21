using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Shared;
using RabbitMQ.Shared.Models;
using RabbitMQ.Shared.Utils;
using System.Text;
using System.Text.Json;

namespace RabbitMQ.Consumer;

public class Program
{
    // Cache de mensagens processadas para deduplicação
    private static readonly HashSet<string> _processedMessageIds = new();
    
    static void Main(string[] args)
    {
        Console.WriteLine("=== Consumidor RabbitMQ - Demonstração ===");
        Console.WriteLine("Iniciando conexão com RabbitMQ...");
        
        // Criar conexão com RabbitMQ
        using var connection = RabbitMQHelper.CreateConnection();
        using var channel = RabbitMQHelper.CreateChannel(connection);
        
        Console.WriteLine("Conexão estabelecida com sucesso!");
        Console.WriteLine();
        
        // Configurar a infraestrutura do RabbitMQ
        RabbitMQSetup.SetupInfrastructure(channel);
        
        // Configurar QoS (Quality of Service)
        // Isso limita o número de mensagens que o consumidor pode receber antes de enviar uma confirmação
        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        
        var continuar = true;
        while (continuar)
        {
            Console.WriteLine("\nSelecione o tipo de consumidor:");
            Console.WriteLine("1 - Consumidor de fila simples");
            Console.WriteLine("2 - Worker (round-robin) - Consumidor 1");
            Console.WriteLine("3 - Worker (round-robin) - Consumidor 2");
            Console.WriteLine("4 - Consumidor de exchange Direct (key1)");
            Console.WriteLine("5 - Consumidor de exchange Direct (key2)");
            Console.WriteLine("6 - Consumidor de exchange Fanout");
            Console.WriteLine("7 - Consumidor de exchange Topic (pattern1)");
            Console.WriteLine("8 - Consumidor de exchange Topic (pattern2)");
            Console.WriteLine("9 - Consumir da fila com TTL");
            Console.WriteLine("10 - Consumir mensagens atrasadas");
            Console.WriteLine("11 - Consumir da fila com deduplicação");
            Console.WriteLine("12 - Consumir da Dead Letter Queue");
            Console.WriteLine("13 - Sair");
            Console.Write("Opção: ");
            
            var option = Console.ReadLine();
            Console.WriteLine();
            
            switch (option)
            {
                case "1":
                    ConsumirFilaSimples(channel);
                    break;
                case "2":
                    ConsumirWorkerQueue(channel, "Consumidor 1");
                    break;
                case "3":
                    ConsumirWorkerQueue(channel, "Consumidor 2");
                    break;
                case "4":
                    ConsumirExchangeDirect(channel, RabbitMQConstants.DirectQueue1Name, "Direct Consumer 1");
                    break;
                case "5":
                    ConsumirExchangeDirect(channel, RabbitMQConstants.DirectQueue2Name, "Direct Consumer 2");
                    break;
                case "6":
                    ConsumirExchangeFanout(channel);
                    break;
                case "7":
                    ConsumirExchangeTopic(channel, RabbitMQConstants.TopicQueue1Name, "Topic Consumer 1");
                    break;
                case "8":
                    ConsumirExchangeTopic(channel, RabbitMQConstants.TopicQueue2Name, "Topic Consumer 2");
                    break;
                case "9":
                    ConsumirFilaComTTL(channel);
                    break;
                case "10":
                    ConsumirMensagensAtrasadas(channel);
                    break;
                case "11":
                    ConsumirComDeduplicacao(channel);
                    break;
                case "12":
                    ConsumirDeadLetterQueue(channel);
                    break;
                case "13":
                    continuar = false;
                    break;
                default:
                    Console.WriteLine("Opção inválida!");
                    break;
            }
        }
        
        Console.WriteLine("Consumidor finalizado. Pressione qualquer tecla para sair...");
        Console.ReadKey();
    }
    
    private static void ConsumirFilaSimples(IModel channel)
    {
        Console.WriteLine("Iniciando consumidor de fila simples...");
        Console.WriteLine($"Consumindo da fila: {RabbitMQConstants.SimpleQueueName}");
        Console.WriteLine("Pressione [Enter] para encerrar o consumo");
        
        // Criar um consumidor
        var consumer = new EventingBasicConsumer(channel);
        
        // Registrar o evento de recebimento de mensagem
        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<Message>(messageJson);
                
                Console.WriteLine($"[*] Mensagem recebida: {message}");
                
                // Simular algum processamento
                Thread.Sleep(500);
                
                // Confirmar o processamento da mensagem
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar mensagem: {ex.Message}");
                // Rejeitar a mensagem em caso de erro (não recolocar na fila)
                channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: false);
            }
        };
        
        // Iniciar o consumo
        var consumerTag = channel.BasicConsume(
            queue: RabbitMQConstants.SimpleQueueName,
            autoAck: false, // Desabilitar o auto-acknowledgment
            consumer: consumer);
            
        Console.WriteLine("Consumidor iniciado. Aguardando mensagens...");
        Console.ReadLine();
        
        // Cancelar o consumo quando o usuário pressionar Enter
        channel.BasicCancel(consumerTag);
        Console.WriteLine("Consumo de fila simples encerrado!");
    }
    
    private static void ConsumirWorkerQueue(IModel channel, string consumerName)
    {
        Console.WriteLine($"Iniciando worker {consumerName}...");
        Console.WriteLine($"Consumindo da fila: {RabbitMQConstants.WorkQueueName}");
        Console.WriteLine("Pressione [Enter] para encerrar o consumo");
        
        // Criar um consumidor
        var consumer = new EventingBasicConsumer(channel);
        
        // Registrar o evento de recebimento de mensagem
        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<Message>(messageJson);
                
                Console.WriteLine($"[{consumerName}] Recebido: {message}");
                
                // Simular processamento com tempo variável baseado no conteúdo da mensagem
                int processTime = 1000 + (message?.Content?.Length ?? 0) * 10;
                Console.WriteLine($"[{consumerName}] Processando por {processTime}ms...");
                Thread.Sleep(processTime);
                
                Console.WriteLine($"[{consumerName}] Processamento concluído!");
                
                // Confirmar o processamento da mensagem
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{consumerName}] Erro ao processar mensagem: {ex.Message}");
                // Rejeitar a mensagem em caso de erro (recolocar na fila)
                channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: true);
            }
        };
        
        // Iniciar o consumo
        var consumerTag = channel.BasicConsume(
            queue: RabbitMQConstants.WorkQueueName,
            autoAck: false,
            consumer: consumer);
            
        Console.WriteLine($"Worker {consumerName} iniciado. Aguardando mensagens...");
        Console.ReadLine();
        
        // Cancelar o consumo quando o usuário pressionar Enter
        channel.BasicCancel(consumerTag);
        Console.WriteLine($"Worker {consumerName} encerrado!");
    }
    
    private static void ConsumirExchangeDirect(IModel channel, string queueName, string consumerName)
    {
        Console.WriteLine($"Iniciando consumidor Direct {consumerName}...");
        Console.WriteLine($"Consumindo da fila: {queueName}");
        Console.WriteLine("Pressione [Enter] para encerrar o consumo");
        
        // Criar um consumidor
        var consumer = new EventingBasicConsumer(channel);
        
        // Registrar o evento de recebimento de mensagem
        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<Message>(messageJson);
                
                Console.WriteLine($"[{consumerName}] Recebido via Direct com routing key '{ea.RoutingKey}': {message}");
                
                // Simular processamento
                Thread.Sleep(500);
                
                // Confirmar o processamento da mensagem
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{consumerName}] Erro ao processar mensagem: {ex.Message}");
                channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: false);
            }
        };
        
        // Iniciar o consumo
        var consumerTag = channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer);
            
        Console.WriteLine($"Consumidor Direct {consumerName} iniciado. Aguardando mensagens...");
        Console.ReadLine();
        
        // Cancelar o consumo quando o usuário pressionar Enter
        channel.BasicCancel(consumerTag);
        Console.WriteLine($"Consumidor Direct {consumerName} encerrado!");
    }
    
    private static void ConsumirExchangeFanout(IModel channel)
    {
        Console.WriteLine("Iniciando consumidor Fanout...");
        Console.WriteLine("Escolha a fila para consumir:");
        Console.WriteLine($"1 - {RabbitMQConstants.FanoutQueue1Name}");
        Console.WriteLine($"2 - {RabbitMQConstants.FanoutQueue2Name}");
        Console.Write("Opção: ");
        
        string queueName;
        if (Console.ReadLine() == "1")
            queueName = RabbitMQConstants.FanoutQueue1Name;
        else
            queueName = RabbitMQConstants.FanoutQueue2Name;
            
        Console.WriteLine($"Consumindo da fila: {queueName}");
        Console.WriteLine("Pressione [Enter] para encerrar o consumo");
        
        // Criar um consumidor
        var consumer = new EventingBasicConsumer(channel);
        
        // Registrar o evento de recebimento de mensagem
        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<Message>(messageJson);
                
                Console.WriteLine($"[Fanout Consumer] Recebido via Fanout: {message}");
                
                // Simular processamento
                Thread.Sleep(500);
                
                // Confirmar o processamento da mensagem
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Fanout Consumer] Erro ao processar mensagem: {ex.Message}");
                channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: false);
            }
        };
        
        // Iniciar o consumo
        var consumerTag = channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer);
            
        Console.WriteLine("Consumidor Fanout iniciado. Aguardando mensagens...");
        Console.ReadLine();
        
        // Cancelar o consumo quando o usuário pressionar Enter
        channel.BasicCancel(consumerTag);
        Console.WriteLine("Consumidor Fanout encerrado!");
    }
    
    private static void ConsumirExchangeTopic(IModel channel, string queueName, string consumerName)
    {
        Console.WriteLine($"Iniciando consumidor Topic {consumerName}...");
        Console.WriteLine($"Consumindo da fila: {queueName}");
        Console.WriteLine("Pressione [Enter] para encerrar o consumo");
        
        // Criar um consumidor
        var consumer = new EventingBasicConsumer(channel);
        
        // Registrar o evento de recebimento de mensagem
        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<Message>(messageJson);
                
                Console.WriteLine($"[{consumerName}] Recebido via Topic com routing key '{ea.RoutingKey}': {message}");
                
                // Simular processamento
                Thread.Sleep(500);
                
                // Confirmar o processamento da mensagem
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{consumerName}] Erro ao processar mensagem: {ex.Message}");
                channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: false);
            }
        };
        
        // Iniciar o consumo
        var consumerTag = channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer);
            
        Console.WriteLine($"Consumidor Topic {consumerName} iniciado. Aguardando mensagens...");
        Console.ReadLine();
        
        // Cancelar o consumo quando o usuário pressionar Enter
        channel.BasicCancel(consumerTag);
        Console.WriteLine($"Consumidor Topic {consumerName} encerrado!");
    }
    
    /// <summary>
    /// Consome mensagens da fila com TTL configurado
    /// </summary>
    private static void ConsumirFilaComTTL(IModel channel)
    {
        Console.WriteLine("Iniciando consumidor da fila com TTL...");
        Console.WriteLine($"Consumindo da fila: {RabbitMQConstants.TtlQueueName}");
        Console.WriteLine($"Mensagens não consumidas serão movidas para a Dead Letter Queue após {RabbitMQConstants.MessageTtlMs}ms");
        Console.WriteLine("Pressione [Enter] para encerrar o consumo");
        
        // Criar um consumidor
        var consumer = new EventingBasicConsumer(channel);
        
        // Registrar o evento de recebimento de mensagem
        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<Message>(messageJson);
                
                Console.WriteLine($"[TTL Consumer] Mensagem recebida da fila com TTL: {message}");
                
                // Simular processamento
                Thread.Sleep(500);
                
                // Confirmar o processamento da mensagem
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TTL Consumer] Erro ao processar mensagem: {ex.Message}");
                // Rejeitar a mensagem em caso de erro
                channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: false);
            }
        };
        
        // Iniciar o consumo
        var consumerTag = channel.BasicConsume(
            queue: RabbitMQConstants.TtlQueueName,
            autoAck: false,
            consumer: consumer);
            
        Console.WriteLine("Consumidor de fila com TTL iniciado. Aguardando mensagens...");
        Console.ReadLine();
        
        // Cancelar o consumo quando o usuário pressionar Enter
        channel.BasicCancel(consumerTag);
        Console.WriteLine("Consumidor de fila com TTL encerrado!");
    }
    
    /// <summary>
    /// Consome mensagens atrasadas (delayed messages) da fila de atraso usando o plugin
    /// </summary>
    private static void ConsumirMensagensAtrasadas(IModel channel)
    {
        Console.WriteLine("Iniciando consumidor de mensagens atrasadas...");
        Console.WriteLine("Este consumidor recebe mensagens enviadas com atraso usando o plugin rabbitmq_delayed_message_exchange");
        Console.WriteLine($"Consumindo da fila: {RabbitMQConstants.DelayedQueueName}");
        Console.WriteLine("Pressione [Enter] para encerrar o consumo");
        
        // Criar um consumidor
        var consumer = new EventingBasicConsumer(channel);
        
        // Registrar o evento de recebimento de mensagem
        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<Message>(messageJson);
                
                var tempoDecorrido = DateTime.UtcNow - message.CreatedAt;
                
                Console.WriteLine($"[Delayed Consumer] Mensagem atrasada recebida:");
                Console.WriteLine($"  Conteúdo: {message.Content}");
                Console.WriteLine($"  ID: {message.Id}");
                Console.WriteLine($"  Enviada em: {message.CreatedAt:G}");
                Console.WriteLine($"  Recebida em: {DateTime.UtcNow:G}");
                Console.WriteLine($"  Tempo de atraso efetivo: {tempoDecorrido.TotalSeconds:F2} segundos");
                
                // Simular processamento
                Thread.Sleep(500);
                
                // Confirmar o processamento da mensagem
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                
                Console.WriteLine("[Delayed Consumer] Mensagem processada com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Delayed Consumer] Erro ao processar mensagem: {ex.Message}");
                channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: false);
            }
        };
        
        // Iniciar o consumo
        var consumerTag = channel.BasicConsume(
            queue: RabbitMQConstants.DelayedQueueName,
            autoAck: false,
            consumer: consumer);
            
        Console.WriteLine("Consumidor de mensagens atrasadas iniciado. Aguardando mensagens...");
        Console.ReadLine();
        
        // Cancelar o consumo quando o usuário pressionar Enter
        channel.BasicCancel(consumerTag);
        Console.WriteLine("Consumidor de mensagens atrasadas encerrado!");
    }
    
    /// <summary>
    /// Consome mensagens da fila com deduplicação implementada
    /// </summary>
    private static void ConsumirComDeduplicacao(IModel channel)
    {
        Console.WriteLine("Iniciando consumidor com deduplicação...");
        Console.WriteLine($"Consumindo da fila: {RabbitMQConstants.DeduplicationQueueName}");
        Console.WriteLine("Pressione [Enter] para encerrar o consumo");
        
        // Limpar o cache de mensagens processadas antes de iniciar
        _processedMessageIds.Clear();
        
        // Criar um consumidor
        var consumer = new EventingBasicConsumer(channel);
        
        // Registrar o evento de recebimento de mensagem
        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<Message>(messageJson);
                
                // Extrair o ID da mensagem
                string messageId = message.Id;
                
                // Tentar extrair o messageId do header (alternativa se o ID estiver no header)
                if (ea.BasicProperties.Headers != null && 
                    ea.BasicProperties.Headers.TryGetValue(RabbitMQConstants.MessageIdHeaderName, out var headerMessageId))
                {
                    var headerMessageIdBytes = headerMessageId as byte[];
                    if (headerMessageIdBytes != null)
                    {
                        messageId = Encoding.UTF8.GetString(headerMessageIdBytes);
                    }
                }
                
                // Verificar se a mensagem já foi processada
                bool isDuplicate = !_processedMessageIds.Add(messageId);
                
                if (isDuplicate)
                {
                    Console.WriteLine($"[Deduplication] MENSAGEM DUPLICADA DETECTADA! ID: {messageId}");
                    Console.WriteLine($"[Deduplication] Conteúdo da mensagem duplicada: {message.Content}");
                    Console.WriteLine($"[Deduplication] Esta mensagem já foi processada. Ignorando.");
                    
                    // Confirmar a mensagem duplicada (não processá-la novamente)
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    return;
                }
                
                Console.WriteLine($"[Deduplication] Nova mensagem recebida (ID: {messageId}): {message.Content}");
                
                // Simular processamento
                Thread.Sleep(500);
                
                Console.WriteLine($"[Deduplication] Mensagem processada com sucesso: {message.Content}");
                
                // Confirmar o processamento da mensagem
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Deduplication] Erro ao processar mensagem: {ex.Message}");
                channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: false);
            }
        };
        
        // Iniciar o consumo
        var consumerTag = channel.BasicConsume(
            queue: RabbitMQConstants.DeduplicationQueueName,
            autoAck: false,
            consumer: consumer);
            
        Console.WriteLine("Consumidor com deduplicação iniciado. Aguardando mensagens...");
        Console.ReadLine();
        
        // Cancelar o consumo quando o usuário pressionar Enter
        channel.BasicCancel(consumerTag);
        Console.WriteLine("Consumidor com deduplicação encerrado!");
    }
    
    /// <summary>
    /// Consome mensagens da Dead Letter Queue (mensagens que foram rejeitadas ou expiraram)
    /// </summary>
    private static void ConsumirDeadLetterQueue(IModel channel)
    {
        Console.WriteLine("Iniciando consumidor da Dead Letter Queue...");
        Console.WriteLine($"Consumindo da fila: {RabbitMQConstants.DeadLetterQueueName}");
        Console.WriteLine("Pressione [Enter] para encerrar o consumo");
        
        // Criar um consumidor
        var consumer = new EventingBasicConsumer(channel);
        
        // Registrar o evento de recebimento de mensagem
        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<Message>(messageJson);
                
                // Headers especiais que o RabbitMQ adiciona em mensagens enviadas para DLX
                string originalExchange = "";
                string originalRoutingKey = "";
                string reason = "unknown";
                
                if (ea.BasicProperties.Headers != null)
                {
                    if (ea.BasicProperties.Headers.TryGetValue("x-death", out var xdeath) && xdeath is List<object> xdeathList && xdeathList.Count > 0)
                    {
                        if (xdeathList[0] is Dictionary<string, object> xdeathInfo)
                        {
                            if (xdeathInfo.TryGetValue("exchange", out var exchange))
                                originalExchange = exchange.ToString();
                            
                            if (xdeathInfo.TryGetValue("routing-keys", out var routingKeys) && routingKeys is List<object> routingKeysList && routingKeysList.Count > 0)
                                originalRoutingKey = routingKeysList[0].ToString();
                            
                            if (xdeathInfo.TryGetValue("reason", out var deathReason))
                                reason = deathReason.ToString();
                        }
                    }
                }
                
                Console.WriteLine($"[DLQ Consumer] Mensagem da Dead Letter Queue: {message}");
                Console.WriteLine($"[DLQ Consumer] Razão: {reason}");
                Console.WriteLine($"[DLQ Consumer] Exchange original: {originalExchange}");
                Console.WriteLine($"[DLQ Consumer] Routing key original: {originalRoutingKey}");
                
                // Simular processamento
                Thread.Sleep(500);
                
                // Confirmar o processamento da mensagem
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DLQ Consumer] Erro ao processar mensagem: {ex.Message}");
                channel.BasicReject(deliveryTag: ea.DeliveryTag, requeue: false);
            }
        };
        
        // Iniciar o consumo
        var consumerTag = channel.BasicConsume(
            queue: RabbitMQConstants.DeadLetterQueueName,
            autoAck: false,
            consumer: consumer);
            
        Console.WriteLine("Consumidor da Dead Letter Queue iniciado. Aguardando mensagens...");
        Console.ReadLine();
        
        // Cancelar o consumo quando o usuário pressionar Enter
        channel.BasicCancel(consumerTag);
        Console.WriteLine("Consumidor da Dead Letter Queue encerrado!");
    }
}
