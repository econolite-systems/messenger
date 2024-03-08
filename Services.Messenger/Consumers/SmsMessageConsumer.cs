// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Confluent.Kafka;
using Econolite.Ode.Monitoring.Events;
using Econolite.Ode.Monitoring.Events.Extensions;
using Econolite.Ode.Monitoring.Metrics;
using Econolite.Ode.Services.Messenger.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Econolite.Ode.Services.Messenger.Consumers;

/// <summary>
/// SmsMessageConsumer
/// </summary>
public class SmsMessageConsumer : BaseConsumer<Guid, SmsMessage>
{
    protected override string _topicConfigKey => "Kafka:Topics:SmsMessageTopic";

    private readonly IMessengerService _messengerService;
    private readonly IMetricsCounter _loopCounter;
    private readonly UserEventFactory _userEventFactory;

    /// <summary>
    /// SmsMessageConsumer
    /// </summary>
    /// <param name="config"></param>
    /// <param name="logger"></param>
    /// <param name="consumerConfig"></param>
    /// <param name="messengerService"></param>
    public SmsMessageConsumer(IConfiguration config, ILogger<SmsMessageConsumer> logger, IOptions<ConsumerConfig> consumerConfig, IMessengerService messengerService, UserEventFactory userEventFactory, IMetricsFactory metricsFactory) : base(config, logger, consumerConfig)
    {
        _messengerService = messengerService;
        _userEventFactory = userEventFactory;

        _loopCounter = metricsFactory.GetMetricsCounter("Consumer");
    }

    protected override async Task ProcessConsumerResult(ConsumeResult<Guid, SmsMessage> consumerResult)
    {
        if (consumerResult.Message.Value != null)
        {
            try
            {
                await _messengerService.SendSmsMessageAsync(consumerResult.Message.Value);
                _loopCounter.Increment();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to send message: {@message}", consumerResult.Message.Value);

                _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Error, string.Format("Unable to send message: {0}", consumerResult.Message.Value.RecipientPhone)));
            }
        }
        else
        {
            _logger.LogWarning("Unable to process message, object serialization change?");
        }
    }
}