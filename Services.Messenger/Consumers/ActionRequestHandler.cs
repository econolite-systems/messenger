// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Authorization;
using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Elements;
using Econolite.Ode.Monitoring.Metrics;
using Econolite.Ode.Router.ActionSet.Messaging;
using Econolite.Ode.Services.Messenger.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;

namespace Econolite.Ode.Services.Messenger.Consumers;

public class ActionRequestHandler : BackgroundService
{
    private readonly ISource<ActionRequest> _source;
    private readonly HttpClient _httpClient;
    private readonly ITokenHandler _tokenHandler;
    private readonly IMessengerService _messengerService;
    private readonly string _configurationApi;
    private readonly string _authenticationApi;
    private readonly ILogger<ActionRequestHandler> _logger;
    private readonly IMetricsCounter _messageCounter;

    public ActionRequestHandler(IMessengerService messengerService, ISource<ActionRequest> source, HttpClient httpClient, ITokenHandler tokenHandler, IMetricsFactory metricsFactory, IConfiguration configuration, ILogger<ActionRequestHandler> logger)
    {
        _source = source;
        _httpClient = httpClient;
        _tokenHandler = tokenHandler;
        _messengerService = messengerService;
        _logger = logger;
        _messageCounter = metricsFactory.GetMetricsCounter("Action Request");

        _configurationApi = configuration.GetValue("Services:Configuration", "")!;
        _authenticationApi = configuration.GetValue("Authentication:Api", "")!;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting to consume Action Request");
        try
        {
            await _source.ConsumeOnAsync(async result =>
            {
                _messageCounter.Increment();
                if (result.Value.ActionType == "send-sms-message")
                {
                    await SendSmsMessageAsync(result, cancellationToken);
                }
            }, cancellationToken);
        }
        finally
        {
            _logger.LogInformation("Ending Action Request consumption");
        }
    }

    private async Task SendSmsMessageAsync(ConsumeResult<Guid, ActionRequest> result, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_configurationApi) && !string.IsNullOrEmpty(_authenticationApi))
        {
            var token = await _tokenHandler.GetTokenAsync(cancellationToken);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var twilioConfig = await _httpClient.GetFromJsonAsync<TwilioConfig>($"{_configurationApi}/twilio");
            if (twilioConfig != null)
            {
                var actionSet = await _httpClient.GetFromJsonAsync<ActionSet>($"{_configurationApi}/action-set/{result.Value.ActionSetId}");
                if (actionSet != null)
                {
                    var users = new List<UserModel>();
                    foreach (var username in result.Value.Parameter)
                    {
                        var args = new List<(string key, string value)>();
                        if (!string.IsNullOrWhiteSpace(username))
                            args.Add(("username", HttpUtility.UrlEncode(username)));
                        var user = await _httpClient.GetFromJsonAsync<UserModel[]>($"{_authenticationApi}/users?{string.Join("&", args.Select(a => $"{a.key}={a.value}"))}");
                        if (user != null)
                            users.AddRange(user);
                    }

                    if (users != null && users.Count != 0)
                    {
                        foreach (var user in users)
                        {
                            if (user.Attributes.ContainsKey("phoneNumber"))
                            {
                                var phoneNumber = user.Attributes["phoneNumber"].FirstOrDefault() ?? "";

                                var smsMessage = new SmsMessage
                                {
                                    MessageText = actionSet.Name,
                                    RecipientPhone = phoneNumber
                                };

                                await _messengerService.SendSmsMessageAsync(smsMessage, twilioConfig);
                            }
                        }
                    }
                }
            }
        }
    }
}