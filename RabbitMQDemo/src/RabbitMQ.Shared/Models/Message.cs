namespace RabbitMQ.Shared.Models;

/// <summary>
/// Modelo de mensagem para demonstração
/// </summary>
public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;
    
    public override string ToString()
    {
        return $"Mensagem [ID: {Id}] - {Content} - Criada em: {CreatedAt} - Prioridade: {Priority}";
    }
}

/// <summary>
/// Enumeração de prioridades de mensagem
/// </summary>
public enum MessagePriority
{
    Low,
    Normal,
    High,
    Critical
} 