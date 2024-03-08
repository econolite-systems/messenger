// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Econolite.Ode.Services.Messenger.Models;

namespace Econolite.Ode.Services.Messenger;

/// <summary>
/// MessengerService
/// </summary>
public class MessengerService : IMessengerService
{
    private readonly ILogger<MessengerService> _logger;

    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _senderPhone;

    private const string PHONE_REGEX = @"^(\+\d{1,2}\s?)?1?\-?\.?\s?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$";

    /// <summary>
    /// MessengerService
    /// </summary>
    /// <param name="config"></param>
    /// <param name="logger"></param>
    public MessengerService(IConfiguration config, ILogger<MessengerService> logger)
    {
        _accountSid = config.GetSection("Twilio:AccountSid").Value ?? string.Empty;
        _authToken = config.GetSection("Twilio:AuthToken").Value ?? string.Empty;
        _senderPhone = config.GetSection("Twilio:SenderPhone").Value ?? string.Empty;

        _logger = logger;
    }

    public async Task SendSmsMessageAsync(SmsMessage smsMessage)
    {
        var twilioConfig = new TwilioConfig
        {
            AccountSid = _accountSid,
            AuthToken = _authToken,
            SenderPhone = _senderPhone
        };

        await SendSmsMessageAsync(smsMessage, twilioConfig);
    }

    /// <summary>
    /// SendSmsMessageAsync
    /// </summary>
    /// <param name="smsMessage"></param>
    /// <param name="twilioConfig"></param>
    /// <returns></returns>
    public async Task SendSmsMessageAsync(SmsMessage smsMessage, TwilioConfig twilioConfig)
    {
        if (!string.IsNullOrEmpty(smsMessage.RecipientPhone) && !string.IsNullOrEmpty(smsMessage.MessageText) &&
            !string.IsNullOrEmpty(twilioConfig.AccountSid) && !string.IsNullOrEmpty(twilioConfig.AuthToken) && !string.IsNullOrEmpty(twilioConfig.SenderPhone))
        {
            try
            {
                var match = Regex.Match(smsMessage.RecipientPhone, PHONE_REGEX);
                if (match.Success)
                {
                    TwilioClient.Init(twilioConfig.AccountSid, twilioConfig.AuthToken);

                    MessageResource.Create(
                        body: smsMessage.MessageText,
                        from: new PhoneNumber(twilioConfig.SenderPhone),
                        to: new PhoneNumber(smsMessage.RecipientPhone));
                }
                else
                {
                    _logger.LogWarning("Invalid phone number: {@}", smsMessage.RecipientPhone);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                throw;
            }
        }
    }
}
