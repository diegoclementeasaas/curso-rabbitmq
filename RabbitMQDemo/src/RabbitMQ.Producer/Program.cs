using RabbitMQ.Client;
using RabbitMQ.Shared;
using RabbitMQ.Shared.Models;
using RabbitMQ.Shared.Utils;
using System.Text;
using System.Text.Json;

namespace RabbitMQ.Producer;

public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Produtor RabbitMQ - Demonstração ===");
        Console.WriteLine("Iniciando conexão com RabbitMQ...");
        
        // Criar conexão com RabbitMQ
        using var connection = RabbitMQHelper.CreateConnection();
        using var channel = RabbitMQHelper.CreateChannel(connection);
        
        Console.WriteLine("Conexão estabelecida com sucesso!");
        Console.WriteLine();
        
        // Configurar a infraestrutura do RabbitMQ
        RabbitMQSetup.SetupInfrastructure(channel);
        
        bool continuar = true;
        while (continuar)
        {
            Console.WriteLine("\nSelecione o tipo de envio:");
            Console.WriteLine("1 - Enviar para fila simples");
            Console.WriteLine("2 - Enviar para worker queue (round-robin)");
            Console.WriteLine("3 - Enviar via exchange Direct");
            Console.WriteLine("4 - Enviar via exchange Fanout");
            Console.WriteLine("5 - Enviar via exchange Topic");
            Console.WriteLine("6 - Enviar para fila com TTL");
            Console.WriteLine("7 - Enviar para fila com delay");
            Console.WriteLine("8 - Enviar para fila com deduplicação");
            Console.WriteLine("9 - Sair");
            Console.Write("Opção: ");
            
            var option = Console.ReadLine();
            Console.WriteLine();
            
            switch (option)
            {
                case "1":
                    EnviarParaFilaSimples(channel);
                    break;
                case "2":
                    EnviarParaWorkerQueue(channel);
                    break;
                case "3":
                    EnviarViaExchangeDirect(channel);
                    break;
                case "4":
                    EnviarViaExchangeFanout(channel);
                    break;
                case "5":
                    EnviarViaExchangeTopic(channel);
                    break;
                case "6":
                    EnviarParaFilaComTTL(channel);
                    break;
                case "7":
                    EnviarMensagemAtrasada(channel);
                    break;
                case "8":
                    EnviarComDeduplicacao(channel);
                    break;
                case "9":
                    continuar = false;
                    break;
                default:
                    Console.WriteLine("Opção inválida!");
                    break;
            }
        }
        
        Console.WriteLine("Produtor finalizado. Pressione qualquer tecla para sair...");
        Console.ReadKey();
    }
    
    private static void EnviarParaFilaSimples(IModel channel)
    {
        Console.Write("Digite a mensagem: ");
        var conteudo = Console.ReadLine() ?? "Mensagem de teste";
        
        var message = new Message
        {
            Content = conteudo,
            Priority = MessagePriority.Normal
        };
        
        var messageJson = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(messageJson);
        
        // Propriedades da mensagem
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true; // Mensagem persistente (sobrevive a reinicializações)
        
        // Publicar mensagem diretamente na fila
        channel.BasicPublish(
            exchange: "",  // Exchange padrão (vazio) para envio direto para filas
            routingKey: RabbitMQConstants.SimpleQueueName, // Nome da fila como routing key
            basicProperties: properties,
            body: body);
            
        Console.WriteLine($"[x] Mensagem enviada para fila simples: {message.Content}");
    }
    
    private static void EnviarParaWorkerQueue(IModel channel)
    {
        Console.Write("Digite a mensagem: ");
        var conteudo = Console.ReadLine() ?? "Mensagem de teste";
        Console.Write("Quantidade de mensagens a enviar: ");
        if (!int.TryParse(Console.ReadLine(), out int quantidade))
            quantidade = 5;
            
        for (int i = 1; i <= quantidade; i++)
        {
            var message = new Message
            {
                Content = $"{conteudo} #{i}",
                Priority = MessagePriority.Normal
            };
            
            var messageJson = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(messageJson);
            
            // Propriedades da mensagem
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true; // Mensagem persistente
            
            // Publicar mensagem na fila de trabalho
            channel.BasicPublish(
                exchange: "",
                routingKey: RabbitMQConstants.WorkQueueName,
                basicProperties: properties,
                body: body);
                
            Console.WriteLine($"[x] Mensagem enviada para worker queue: {message.Content}");
            
            // Pequeno delay para simular processamento
            Thread.Sleep(100);
        }
    }
    
    private static void EnviarViaExchangeDirect(IModel channel)
    {
        Console.Write("Digite a mensagem: ");
        var conteudo = Console.ReadLine() ?? "Mensagem de teste";
        Console.WriteLine("Escolha a routing key:");
        Console.WriteLine($"1 - {RabbitMQConstants.DirectRoutingKey1}");
        Console.WriteLine($"2 - {RabbitMQConstants.DirectRoutingKey2}");
        Console.Write("Opção: ");
        
        string routingKey;
        if (Console.ReadLine() == "1")
            routingKey = RabbitMQConstants.DirectRoutingKey1;
        else
            routingKey = RabbitMQConstants.DirectRoutingKey2;
            
        var message = new Message
        {
            Content = conteudo,
            Priority = MessagePriority.High
        };
        
        var messageJson = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(messageJson);
        
        // Propriedades da mensagem
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        
        // Publicar mensagem no exchange Direct
        channel.BasicPublish(
            exchange: RabbitMQConstants.DirectExchangeName,
            routingKey: routingKey,
            basicProperties: properties,
            body: body);
            
        Console.WriteLine($"[x] Mensagem enviada para exchange Direct com routing key '{routingKey}': {message.Content}");
    }
    
    private static void EnviarViaExchangeFanout(IModel channel)
    {
        Console.Write("Digite a mensagem: ");
        var conteudo = Console.ReadLine() ?? "Mensagem de teste";
        
        var message = new Message
        {
            Content = conteudo,
            Priority = MessagePriority.Critical
        };
        
        var messageJson = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(messageJson);
        
        // Propriedades da mensagem
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        
        // Publicar mensagem no exchange Fanout
        channel.BasicPublish(
            exchange: RabbitMQConstants.FanoutExchangeName,
            routingKey: "", // Fanout ignora a routing key
            basicProperties: properties,
            body: body);
            
        Console.WriteLine($"[x] Mensagem enviada para exchange Fanout: {message.Content}");
    }
    
    private static void EnviarViaExchangeTopic(IModel channel)
    {
        Console.Write("Digite a mensagem: ");
        var conteudo = Console.ReadLine() ?? "Mensagem de teste";
        Console.Write("Digite a routing key (ex: topic.key1.value, topic.key2.value): ");
        var routingKey = Console.ReadLine() ?? "topic.key1.value";
        
        var message = new Message
        {
            Content = conteudo,
            Priority = MessagePriority.High
        };
        
        var messageJson = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(messageJson);
        
        // Propriedades da mensagem
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        
        // Publicar mensagem no exchange Topic
        channel.BasicPublish(
            exchange: RabbitMQConstants.TopicExchangeName,
            routingKey: routingKey,
            basicProperties: properties,
            body: body);
            
        Console.WriteLine($"[x] Mensagem enviada para exchange Topic com routing key '{routingKey}': {message.Content}");
    }
    
    /// <summary>
    /// Envia mensagem para fila com TTL configurado. Após o TTL, a mensagem será enviada para a Dead Letter Queue.
    /// </summary>
    private static void EnviarParaFilaComTTL(IModel channel)
    {
        Console.Write("Digite a mensagem: ");
        var conteudo = Console.ReadLine() ?? "Mensagem com TTL";
        
        var message = new Message
        {
            Content = conteudo,
            Priority = MessagePriority.Normal
        };
        
        var messageJson = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(messageJson);
        
        // Propriedades da mensagem
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        
        // Também podemos definir TTL individual para cada mensagem
        // properties.Expiration = "5000"; // 5 segundos
        
        // Publicar mensagem na fila com TTL
        channel.BasicPublish(
            exchange: "",
            routingKey: RabbitMQConstants.TtlQueueName,
            basicProperties: properties,
            body: body);
            
        Console.WriteLine($"[x] Mensagem enviada para fila com TTL ({RabbitMQConstants.MessageTtlMs}ms): {message.Content}");
        Console.WriteLine($"    Após {RabbitMQConstants.MessageTtlMs}ms, a mensagem será enviada para a Dead Letter Queue se não for consumida.");
    }
    
    /// <summary>
    /// Envia uma mensagem para uma fila com atraso na entrega usando o plugin de mensagens atrasadas
    /// </summary>
    private static void EnviarMensagemAtrasada(IModel channel)
    {
        Console.WriteLine("=== Enviar Mensagem Atrasada ===");
        Console.WriteLine("Este exemplo usa o plugin rabbitmq_delayed_message_exchange");
        Console.WriteLine();
        
        Console.Write("Digite o conteúdo da mensagem: ");
        var conteudo = Console.ReadLine();
        
        Console.Write("Digite o atraso em segundos: ");
        if (!int.TryParse(Console.ReadLine(), out int segundosAtraso))
        {
            segundosAtraso = 10; // valor padrão se não informado corretamente
        }
        
        // Criar a mensagem
        var mensagem = new Message
        {
            Id = Guid.NewGuid().ToString(),
            Content = conteudo,
            CreatedAt = DateTime.UtcNow
        };
        
        // Serializar a mensagem
        var mensagemJson = JsonSerializer.Serialize(mensagem);
        var body = Encoding.UTF8.GetBytes(mensagemJson);
        
        // Configurar as propriedades da mensagem
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true; // Tornar a mensagem persistente (durável)
        
        // Configurar o cabeçalho de atraso usando o plugin delayed_message
        var headers = new Dictionary<string, object>
        {
            // Especificar o atraso em milissegundos
            { "x-delay", segundosAtraso * 1000 }
        };
        properties.Headers = headers;
        
        // Publicar a mensagem no exchange delayed
        channel.BasicPublish(
            exchange: RabbitMQConstants.DelayedExchangeName,
            routingKey: RabbitMQConstants.DelayedRoutingKey,
            basicProperties: properties,
            body: body);
            
        Console.WriteLine($"Mensagem enviada com sucesso!");
        Console.WriteLine($"A mensagem será entregue após {segundosAtraso} segundos");
        Console.WriteLine($"ID da mensagem: {mensagem.Id}");
        Console.WriteLine($"Horário de envio: {mensagem.CreatedAt:G}");
        Console.WriteLine($"Horário previsto para entrega: {mensagem.CreatedAt.AddSeconds(segundosAtraso):G}");
        Console.WriteLine();
    }
    
    /// <summary>
    /// Envia mensagem com suporte à deduplicação, utilizando ID da mensagem nos headers
    /// </summary>
    private static void EnviarComDeduplicacao(IModel channel)
    {
        Console.Write("Digite a mensagem: ");
        var conteudo = Console.ReadLine() ?? "Mensagem com deduplicação";
        Console.Write("Número de vezes para enviar a mesma mensagem (para testar deduplicação): ");
        if (!int.TryParse(Console.ReadLine(), out int quantidade))
            quantidade = 3;
            
        // Usamos o mesmo ID para todas as mensagens para demonstrar a deduplicação
        string messageId = Guid.NewGuid().ToString();
        
        var message = new Message
        {
            Id = messageId, // Usar o mesmo ID em todas as cópias para demonstrar deduplicação
            Content = conteudo,
            Priority = MessagePriority.Normal
        };
        
        var messageJson = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(messageJson);
        
        // Propriedades da mensagem
        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.MessageId = messageId;
        
        // Adicionando o messageId nos headers para exemplificar deduplicação
        properties.Headers = new Dictionary<string, object>
        {
            { RabbitMQConstants.MessageIdHeaderName, messageId }
        };
        
        for (int i = 0; i < quantidade; i++)
        {
            // Enviar a mesma mensagem várias vezes
            channel.BasicPublish(
                exchange: "",
                routingKey: RabbitMQConstants.DeduplicationQueueName,
                basicProperties: properties,
                body: body);
            
            Console.WriteLine($"[x] Cópia {i+1}/{quantidade} da mensagem enviada com ID {messageId}: {message.Content}");
            
            // Pequeno delay entre envios
            Thread.Sleep(100);
        }
        
        Console.WriteLine($"[x] {quantidade} cópias da mesma mensagem foram enviadas com o mesmo ID.");
        Console.WriteLine("    O consumidor deve processar apenas uma vez usando deduplicação.");
    }
}
