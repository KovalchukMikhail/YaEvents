using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.BackgroundServices
{
    public class TopicBootstrapper
    {
        private readonly IConfiguration _config;
        private readonly ILogger<TopicBootstrapper> _logger;

        public TopicBootstrapper(IConfiguration config, ILogger<TopicBootstrapper> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var bootstrapServers = _config["Kafka:BootstrapServers"];
            var topicName = YaEventsConfigurations.KafkaConfigurations.CREATING_BOOKINGS_TOPIC_NAME;
            var replicationFactor = short.Parse(_config["Kafka:ReplicationFactor"] ?? "1");
            var partitions = short.Parse(_config["Kafka:Partitions"] ?? "3");

            using var adminClient = new AdminClientBuilder(
                    new AdminClientConfig { BootstrapServers = bootstrapServers })
                .Build();

            try
            {
                var metadata = adminClient.GetMetadata(topicName, TimeSpan.FromMilliseconds(500));
                if (!metadata.Topics.Any(t => t.Topic == topicName))
                {
                    var topicSpecification = new TopicSpecification
                    {
                        Name = topicName,
                        NumPartitions = partitions,
                        ReplicationFactor = replicationFactor
                    };

                    await adminClient.CreateTopicsAsync(
                        new[] { topicSpecification });

                    for(int i = 0; i < 10; i++)
                    {
                        metadata = adminClient.GetMetadata(topicName, TimeSpan.FromMilliseconds(500));
                        var topic = metadata.Topics.FirstOrDefault(t => t.Topic == topicName);
                        if (topic != null)
                            break;

                        await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
                    }
                    _logger.LogInformation("Topic={topicName} создан.", topicName);
                }
                else
                {
                    _logger.LogInformation("Topic={topicName} уже существует.", topicName);
                }
            }
            catch (CreateTopicsException ex)
            {
                _logger.LogInformation("Topic={topicName} уже существует.", topicName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось создать Topic={topicName}", topicName);
                throw;
            }
        }
    }
}
