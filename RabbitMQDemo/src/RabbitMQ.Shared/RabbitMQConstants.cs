namespace RabbitMQ.Shared;

/// <summary>
/// Constantes utilizadas para configuração e uso do RabbitMQ
/// </summary>
public static class RabbitMQConstants
{
    // ===== Configurações de Conexão =====
    public const string HostName = "localhost";
    public const int Port = 5672;
    public const string UserName = "guest";
    public const string Password = "guest";
    public const string VirtualHost = "/";
    public const string ClientProvidedName = "RabbitMQ-Demo-Client";
    public static readonly TimeSpan RequestedHeartbeat = TimeSpan.Zero;

    // ===== Filas Simples =====
    public const string SimpleQueueName = "simple-queue";
    public const string WorkQueueName = "work-queue";
    
    // ===== Exchange Direct =====
    public const string DirectExchangeName = "demo-direct-exchange";
    public const string DirectQueue1Name = "direct-queue-1";
    public const string DirectQueue2Name = "direct-queue-2";
    public const string DirectRoutingKey1 = "key1";
    public const string DirectRoutingKey2 = "key2";
    
    // ===== Exchange Fanout =====
    public const string FanoutExchangeName = "demo-fanout-exchange";
    public const string FanoutQueue1Name = "fanout-queue-1";
    public const string FanoutQueue2Name = "fanout-queue-2";
    
    // ===== Exchange Topic =====
    public const string TopicExchangeName = "demo-topic-exchange";
    public const string TopicQueue1Name = "topic-queue-1";
    public const string TopicQueue2Name = "topic-queue-2";
    public const string TopicPattern1 = "ordem.*.confirmada";
    public const string TopicPattern2 = "ordem.#";
    
    // ===== Dead Letter Exchange =====
    public const string DeadLetterExchangeName = "demo-dead-letter-exchange";
    public const string DeadLetterQueueName = "dead-letter-queue";
    public const string DeadLetterRoutingKey = "dead-letter";
    
    // ===== TTL (Time To Live) =====
    public const string TtlQueueName = "ttl-queue";
    public const int MessageTtlMs = 10000; // 10 segundos
    
    // ===== Delayed Messages =====
    public const string DelayedExchangeName = "demo-delayed-exchange";
    public const string DelayedQueueName = "delayed-queue";
    public const string DelayedRoutingKey = "delayed";
    
    // ===== Deduplicação =====
    public const string DeduplicationExchangeName = "demo-deduplication-exchange";
    public const string DeduplicationQueueName = "deduplication-queue";
    public const string DeduplicationRoutingKey = "deduplication";
    public const string MessageIdHeaderName = "x-message-id";
} 