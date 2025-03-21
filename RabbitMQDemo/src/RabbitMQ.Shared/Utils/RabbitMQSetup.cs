using RabbitMQ.Client;
using System.Collections.Generic;

namespace RabbitMQ.Shared.Utils;

/// <summary>
/// Classe utilitária para configurar automaticamente os recursos necessários no RabbitMQ
/// </summary>
public static class RabbitMQSetup
{
    /// <summary>
    /// Cria todas as filas, exchanges e bindings necessários para a aplicação
    /// </summary>
    /// <param name="channel">Canal de comunicação com o RabbitMQ</param>
    public static void SetupInfrastructure(IModel channel)
    {
        Console.WriteLine("Configurando infraestrutura do RabbitMQ...");
        
        // Configurar Dead Letter Exchange
        ConfigurarDeadLetterExchange(channel);
        
        // Configurar filas simples
        ConfigurarFilasSimples(channel);
        
        // Configurar Exchange Direct
        ConfigurarExchangeDirect(channel);
        
        // Configurar Exchange Fanout
        ConfigurarExchangeFanout(channel);
        
        // Configurar Exchange Topic
        ConfigurarExchangeTopic(channel);
        
        // Configurar TTL e Dead Letter
        ConfigurarTTL(channel);
        
        // Configurar Delayed Messages
        ConfigurarDelayedMessages(channel);
        
        // Configurar Deduplicação
        ConfigurarDeduplicacao(channel);
        
        Console.WriteLine("Infraestrutura do RabbitMQ configurada com sucesso!");
    }
    
    /// <summary>
    /// Configura todas as filas utilizadas na aplicação
    /// </summary>
    /// <param name="channel">Canal de comunicação com o RabbitMQ</param>
    private static void ConfigurarFilasSimples(IModel channel)
    {
        // Declarar uma fila simples
        channel.QueueDeclare(
            queue: RabbitMQConstants.SimpleQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
            
        // Declarar uma fila de trabalho (Worker Queue)
        channel.QueueDeclare(
            queue: RabbitMQConstants.WorkQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }
    
    /// <summary>
    /// Configura todos os exchanges e seus bindings com filas
    /// </summary>
    /// <param name="channel">Canal de comunicação com o RabbitMQ</param>
    private static void ConfigurarExchangeDirect(IModel channel)
    {
        // Declarar exchange Direct
        channel.ExchangeDeclare(
            exchange: RabbitMQConstants.DirectExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            arguments: null);
            
        // Declarar filas para o exchange Direct
        channel.QueueDeclare(
            queue: RabbitMQConstants.DirectQueue1Name,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
            
        channel.QueueDeclare(
            queue: RabbitMQConstants.DirectQueue2Name,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
            
        // Criar bindings para o exchange Direct
        channel.QueueBind(
            queue: RabbitMQConstants.DirectQueue1Name,
            exchange: RabbitMQConstants.DirectExchangeName,
            routingKey: RabbitMQConstants.DirectRoutingKey1);
            
        channel.QueueBind(
            queue: RabbitMQConstants.DirectQueue2Name,
            exchange: RabbitMQConstants.DirectExchangeName,
            routingKey: RabbitMQConstants.DirectRoutingKey2);
    }
    
    /// <summary>
    /// Configura todos os exchanges e seus bindings com filas
    /// </summary>
    /// <param name="channel">Canal de comunicação com o RabbitMQ</param>
    private static void ConfigurarExchangeFanout(IModel channel)
    {
        // Declarar exchange Fanout
        channel.ExchangeDeclare(
            exchange: RabbitMQConstants.FanoutExchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            arguments: null);
            
        // Declarar filas para o exchange Fanout
        channel.QueueDeclare(
            queue: RabbitMQConstants.FanoutQueue1Name,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
            
        channel.QueueDeclare(
            queue: RabbitMQConstants.FanoutQueue2Name,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
            
        // Criar bindings para o exchange Fanout
        channel.QueueBind(
            queue: RabbitMQConstants.FanoutQueue1Name,
            exchange: RabbitMQConstants.FanoutExchangeName,
            routingKey: ""); // Routing key não importa para Fanout
            
        channel.QueueBind(
            queue: RabbitMQConstants.FanoutQueue2Name,
            exchange: RabbitMQConstants.FanoutExchangeName,
            routingKey: ""); // Routing key não importa para Fanout
    }
    
    /// <summary>
    /// Configura todos os exchanges e seus bindings com filas
    /// </summary>
    /// <param name="channel">Canal de comunicação com o RabbitMQ</param>
    private static void ConfigurarExchangeTopic(IModel channel)
    {
        // Declarar exchange Topic
        channel.ExchangeDeclare(
            exchange: RabbitMQConstants.TopicExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null);
            
        // Declarar filas para o exchange Topic
        channel.QueueDeclare(
            queue: RabbitMQConstants.TopicQueue1Name,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
            
        channel.QueueDeclare(
            queue: RabbitMQConstants.TopicQueue2Name,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
            
        // Criar bindings para o exchange Topic
        channel.QueueBind(
            queue: RabbitMQConstants.TopicQueue1Name,
            exchange: RabbitMQConstants.TopicExchangeName,
            routingKey: RabbitMQConstants.TopicPattern1);
            
        channel.QueueBind(
            queue: RabbitMQConstants.TopicQueue2Name,
            exchange: RabbitMQConstants.TopicExchangeName,
            routingKey: RabbitMQConstants.TopicPattern2);
    }
    
    /// <summary>
    /// Configura Dead Letter Exchange e Dead Letter Queue
    /// </summary>
    /// <param name="channel">Canal de comunicação com o RabbitMQ</param>
    private static void ConfigurarDeadLetterExchange(IModel channel)
    {
        // Declarar o exchange de Dead Letter
        channel.ExchangeDeclare(
            exchange: RabbitMQConstants.DeadLetterExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            arguments: null);
            
        // Declarar a fila de Dead Letter
        channel.QueueDeclare(
            queue: RabbitMQConstants.DeadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
            
        // Criar binding para a fila de Dead Letter
        channel.QueueBind(
            queue: RabbitMQConstants.DeadLetterQueueName,
            exchange: RabbitMQConstants.DeadLetterExchangeName,
            routingKey: RabbitMQConstants.DeadLetterRoutingKey);
    }
    
    /// <summary>
    /// Configura fila com TTL (Time To Live)
    /// </summary>
    /// <param name="channel">Canal de comunicação com o RabbitMQ</param>
    private static void ConfigurarTTL(IModel channel)
    {
        // Criar argumentos para a fila com TTL
        var ttlArgs = new Dictionary<string, object>
        {
            // Configurar o DLX (Dead Letter Exchange) 
            {"x-dead-letter-exchange", RabbitMQConstants.DeadLetterExchangeName},
            {"x-dead-letter-routing-key", RabbitMQConstants.DeadLetterRoutingKey},
            // Configurar o TTL para mensagens (em milissegundos)
            {"x-message-ttl", RabbitMQConstants.MessageTtlMs}
        };
        
        // Declarar uma fila com TTL
        channel.QueueDeclare(
            queue: RabbitMQConstants.TtlQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: ttlArgs);
            
        // Usar um exchange padrão (amq.direct) para enviar mensagens para a fila com TTL
        channel.QueueBind(
            queue: RabbitMQConstants.TtlQueueName,
            exchange: "amq.direct",
            routingKey: RabbitMQConstants.TtlQueueName);
    }
    
    /// <summary>
    /// Configura um exchange e uma fila para mensagens atrasadas usando o plugin rabbitmq_delayed_message_exchange
    /// </summary>
    /// <param name="channel">Canal de comunicação com o RabbitMQ</param>
    private static void ConfigurarDelayedMessages(IModel channel)
    {
        Console.WriteLine("Configurando exchange e fila para mensagens atrasadas usando o plugin...");
        
        // Declarar uma fila para mensagens atrasadas
        channel.QueueDeclare(
            queue: RabbitMQConstants.DelayedQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
            
        // Declarar um exchange com o plugin de delayed messages
        var args = new Dictionary<string, object>
        {
            {"x-delayed-type", ExchangeType.Direct}
        };
        
        channel.ExchangeDeclare(
            exchange: RabbitMQConstants.DelayedExchangeName,
            type: "x-delayed-message", // Tipo especial fornecido pelo plugin
            durable: true,
            autoDelete: false,
            arguments: args);
            
        // Criar binding para a fila de mensagens atrasadas
        channel.QueueBind(
            queue: RabbitMQConstants.DelayedQueueName,
            exchange: RabbitMQConstants.DelayedExchangeName,
            routingKey: RabbitMQConstants.DelayedRoutingKey);
            
        Console.WriteLine("Exchange e fila para mensagens atrasadas configurados com sucesso!");
    }
    
    /// <summary>
    /// Configura fila com suporte à deduplicação de mensagens
    /// </summary>
    /// <param name="channel">Canal de comunicação com o RabbitMQ</param>
    private static void ConfigurarDeduplicacao(IModel channel)
    {
        // Declarar exchange para deduplicação
        channel.ExchangeDeclare(
            exchange: RabbitMQConstants.DeduplicationExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            arguments: null);
            
        // Declarar fila para deduplicação
        // OBS: O RabbitMQ não tem deduplicação nativa, então implementamos no código do consumidor
        channel.QueueDeclare(
            queue: RabbitMQConstants.DeduplicationQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
            
        // Criar binding para a fila de deduplicação
        channel.QueueBind(
            queue: RabbitMQConstants.DeduplicationQueueName,
            exchange: RabbitMQConstants.DeduplicationExchangeName,
            routingKey: RabbitMQConstants.DeduplicationRoutingKey);
    }
} 