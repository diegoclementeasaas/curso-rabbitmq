# Demonstração de RabbitMQ com .NET

Este projeto contém exemplos de implementação de diferentes padrões de mensageria usando RabbitMQ com .NET para fins educacionais.

## Estrutura do Projeto

O projeto está organizado em três partes principais:

1. **RabbitMQ.Shared**: Biblioteca compartilhada contendo modelos, constantes e utilitários comuns.
2. **RabbitMQ.Producer**: Aplicação de console para publicação de mensagens.
3. **RabbitMQ.Consumer**: Aplicação de console para consumo de mensagens.

## Pré-requisitos

- .NET 6.0 ou superior
- RabbitMQ Server instalado e em execução (padrão: localhost:5672)
- Credenciais de acesso ao RabbitMQ (padrão: guest/guest)

## Como executar

### 1. Instalar o RabbitMQ Server

Certifique-se de ter o RabbitMQ instalado e em execução. Você pode usar Docker para facilitar:

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### 2. Iniciar o Consumidor ou o Produtor

Tanto o consumidor quanto o produtor podem ser iniciados primeiro, pois ambos configuram automaticamente a infraestrutura necessária no RabbitMQ (filas, exchanges e bindings).

Em um terminal, navegue até a pasta do projeto consumidor e execute:

```bash
cd src/RabbitMQ.Consumer
dotnet run
```

Em outro terminal, navegue até a pasta do projeto produtor e execute:

```bash
cd src/RabbitMQ.Producer
dotnet run
```

## Configuração Automática

O projeto inclui uma funcionalidade de configuração automática que:

- Declara todas as filas necessárias
- Cria todos os exchanges (Direct, Fanout, Topic, Headers)
- Estabelece os bindings entre exchanges e filas
- Garante que toda a infraestrutura está pronta antes de enviar ou receber mensagens

A configuração é feita pela classe `RabbitMQSetup` no projeto compartilhado, que é chamada automaticamente ao iniciar tanto o produtor quanto o consumidor.

## Conceitos Demonstrados

### Tipos de Exchange

1. **Direct Exchange**: Roteia mensagens para filas com base em routing keys exatas.
2. **Fanout Exchange**: Distribui mensagens para todas as filas vinculadas, ignorando routing keys.
3. **Topic Exchange**: Roteia mensagens baseadas em padrões de routing keys utilizando wildcards.
4. **Headers Exchange**: Roteia mensagens baseadas em seus headers em vez de routing keys.

### Padrões de Mensageria

1. **Filas Simples**: Envio direto para uma fila específica.
2. **Worker Queues**: Distribuição de trabalho entre múltiplos consumidores (round-robin).
3. **Pub/Sub**: Publicação para múltiplos consumidores usando Fanout Exchange.
4. **Routing**: Roteamento seletivo usando Direct Exchange.
5. **Topics**: Roteamento baseado em padrões usando Topic Exchange.

### Recursos Avançados

6. **Dead Letter Exchange (DLX)**: Tratamento de mensagens rejeitadas ou que expiraram
7. **TTL (Time-To-Live)**: Definição de tempo máximo de vida para mensagens
8. **Delayed Messages**: Envio de mensagens que serão entregues após um período de tempo
9. **Deduplicação de Mensagens**: Prevenção de processamento duplicado de mensagens

### Dead Letter Exchange (DLX)

O Dead Letter Exchange (DLX) é um recurso poderoso do RabbitMQ para tratar mensagens que não puderam ser entregues. Uma mensagem pode ir para a DLX quando:

- É rejeitada (negada) por um consumidor com `requeue=false`
- Expira o TTL (Time-To-Live) na fila
- A fila atinge seu limite máximo (queue length limit)

Nesta demonstração:
- Configuramos um DLX central chamado `demo-dead-letter-exchange`
- Uma fila específica `dead-letter-queue` recebe todas as mensagens "mortas"
- A opção `x-dead-letter-exchange` nas filas indica para onde enviar mensagens rejeitadas/expiradas

### TTL (Time-To-Live)

O TTL define quanto tempo uma mensagem pode permanecer em uma fila antes de ser descartada ou enviada para uma DLX.

Nesta demonstração:
- Configuramos uma fila `ttl-queue` onde as mensagens expiram após 10 segundos
- Após expirar, as mensagens são automaticamente enviadas para a Dead Letter Queue
- É possível definir TTL tanto para filas inteiras quanto para mensagens individuais

### Delayed Messages

Mensagens atrasadas permitem que você envie mensagens que só serão entregues após um período específico.

Nesta demonstração:
- Implementamos duas abordagens:
  1. Usando o plugin `rabbitmq_delayed_message_exchange` (se disponível)
  2. Uma alternativa usando TTL + DLX quando o plugin não está disponível
- O código verifica automaticamente a disponibilidade do plugin e usa a alternativa se necessário

### Deduplicação de Mensagens

A deduplicação evita que a mesma mensagem seja processada múltiplas vezes, o que é crucial em sistemas distribuídos.

Nesta demonstração:
- Implementamos deduplicação no lado do consumidor usando um cache em memória (em produção, considere Redis ou outro armazenamento distribuído)
- Usamos cabeçalhos de mensagem para adicionar um ID único à mensagem
- O consumidor verifica se o ID já foi processado antes de processar a mensagem

## Recursos Adicionais

- [Documentação Oficial do RabbitMQ](https://www.rabbitmq.com/documentation.html)
- [RabbitMQ .NET Client Library](https://www.rabbitmq.com/dotnet.html)
- [Tutoriais RabbitMQ](https://www.rabbitmq.com/getstarted.html)
- [Plugin de Mensagens Atrasadas](https://github.com/rabbitmq/rabbitmq-delayed-message-exchange) 