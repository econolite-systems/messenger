// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Econolite.Ode.Services.Messenger.Consumers;

/// <summary>
/// BaseConsumer
/// </summary>
/// <typeparam name="Key"></typeparam>
/// <typeparam name="Value"></typeparam>
public abstract class BaseConsumer<Key, Value> : BackgroundService
{
    protected abstract string _topicConfigKey { get; }

    protected readonly IConfiguration _configuration;
    protected readonly ILogger _logger;

    protected virtual string _brokerList { get; set; }
    protected virtual string _brokerListConfigKey => "Kafka:Servers";
    protected virtual string _groupId { get; set; }
    protected virtual string _groupIdConfigKey => "Kafka:GroupId";
    protected virtual string _topic { get; set; }

    /// <Summary>
    /// Warning, these return a new instance on every get. If you need to share 1 for multiple calls, use "property { get; } = new Deserializer<T>();"
    /// </Summary>
    protected virtual IDeserializer<Key> _GetKeyDeserializer => new Deserializer<Key>();
    protected virtual IDeserializer<Value> _GetValueDeserializer => new Deserializer<Value>();

    protected virtual AutoOffsetReset _queueStartingPoint => AutoOffsetReset.Latest;
    protected virtual ConsumerConfig _consumerConfig { get; }

    public BaseConsumer(IConfiguration configuration, ILogger logger, IOptions<ConsumerConfig> config)
    {
        _configuration = configuration;
        _logger = logger;
        _consumerConfig = new ConsumerConfig(config.Value.ToDictionary(x => x.Key, x => x.Value));
    }

    protected override Task ExecuteAsync(CancellationToken token)
    {
        return Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting consumer");
                    _brokerList = _configuration[_brokerListConfigKey];
                    _groupId = _configuration[_groupIdConfigKey];
                    _topic = _configuration[_topicConfigKey];
                    _consumerConfig.EnableAutoCommit = true;
                    _consumerConfig.EnableAutoOffsetStore = false;
                    _consumerConfig.AutoOffsetReset = _queueStartingPoint;
                    _consumerConfig.GroupId = _groupId;
                    using (var consumer = new ConsumerBuilder<Key, Value>(_consumerConfig)
                        .SetKeyDeserializer(_GetKeyDeserializer)
                        .SetValueDeserializer(_GetValueDeserializer)
                        .Build())
                    {
                        consumer.Subscribe(_topic);
                        _logger.LogInformation("Subscribed to {0} on {1}", _topic, _consumerConfig.BootstrapServers);
                        while (!token.IsCancellationRequested)
                        {
                            try
                            {
                                _logger.LogDebug("Waiting for item");
                                var consumerResult = consumer.Consume(token);
                                if (consumerResult != null)
                                {
                                    _logger.LogDebug("Pulled item: {0}, {1}, {2}", consumerResult.Message.Value, consumerResult.Message.Timestamp.UtcDateTime, consumerResult.Message.Key);

                                    await ProcessConsumerResult(consumerResult);

                                    _logger.LogDebug("Storing offset: {0}", consumerResult.Message.Value);

                                    consumer.StoreOffset(consumerResult);

                                    _logger.LogDebug("Done: {0}", consumerResult.Message.Value);
                                }
                            }
                            catch (System.Net.Sockets.SocketException ex)
                            {
                                _logger.LogWarning(ex, "Error attempting to consume, bad comm");
                            }
                            catch (ConsumeException ex)
                            {
                                _logger.LogWarning(ex, "Error attempting to consume");
                                consumer.Close();
                                throw;
                            }
                            catch (KafkaException ex)
                            {
                                _logger.LogWarning(ex, "Error attempting to consume, kafka exception");
                            }
                            catch (ArgumentException ex)
                            {
                                _logger.LogWarning(ex, "Error attempting to consume, argument exception");
                            }
                            catch (OperationCanceledException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error attempting to consume, unknown error of type: {ex.GetType()}, add protection for this exception.");
                            }
                        }
                    }

                    _logger.LogInformation("Consumer stopped");
                    break;
                }
                catch (ConsumeException ex)
                {
                    _logger.LogInformation(ex, "Consumer being rebuilt");
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Consumer canceled");
                    break;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Consumer canceled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Consumer stopped unexpectedly/prematurely");
                    throw;
                }
            }
        }, token);
    }

    /// <summary>
    /// ProcessConsumerResult
    /// </summary>
    /// <param name="consumerResult"></param>
    /// <returns></returns>
    protected abstract Task ProcessConsumerResult(ConsumeResult<Key, Value> consumerResult);

}
