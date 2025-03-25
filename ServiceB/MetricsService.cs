using Prometheus;

namespace ServiceB;

public class MetricsService
{
    public static readonly Counter RabbitMessagesSent = Metrics.CreateCounter(
        "rabbitmq_messages_sent_total", "Total number of messages published to RabbitMQ");

    public static readonly Counter RabbitMessagesReceived = Metrics.CreateCounter(
        "rabbitmq_messages_received_total", "Total number of messages received from RabbitMQ");
}