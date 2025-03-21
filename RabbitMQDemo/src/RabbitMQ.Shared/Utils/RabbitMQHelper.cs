using RabbitMQ.Client;
using System;

namespace RabbitMQ.Shared.Utils;

/// <summary>
/// Classe auxiliar para criação de conexões e canais do RabbitMQ
/// </summary>
public static class RabbitMQHelper
{
    /// <summary>
    /// Cria uma conexão com o servidor RabbitMQ
    /// </summary>
    /// <returns>Uma conexão com o servidor RabbitMQ</returns>
    public static IConnection CreateConnection()
    {
        try
        {
            // Cria uma fábrica de conexões configurada com os valores padrão
            var factory = new ConnectionFactory
            {
                HostName = RabbitMQConstants.HostName,
                Port = RabbitMQConstants.Port,
                UserName = RabbitMQConstants.UserName,
                Password = RabbitMQConstants.Password,
                VirtualHost = RabbitMQConstants.VirtualHost,
                RequestedHeartbeat = RabbitMQConstants.RequestedHeartbeat,
                ClientProvidedName = RabbitMQConstants.ClientProvidedName
            };
            
            // Cria e retorna a conexão
            return factory.CreateConnection();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao criar conexão com RabbitMQ: {ex.Message}");
            throw;
        }
    }
    
    /// <summary>
    /// Cria um canal a partir de uma conexão existente
    /// </summary>
    /// <param name="connection">Conexão com o servidor RabbitMQ</param>
    /// <returns>Um canal para comunicação com o RabbitMQ</returns>
    public static IModel CreateChannel(IConnection connection)
    {
        try
        {
            // Cria e retorna um novo canal
            return connection.CreateModel();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao criar canal do RabbitMQ: {ex.Message}");
            throw;
        }
    }
} 