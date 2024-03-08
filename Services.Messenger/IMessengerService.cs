// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Services.Messenger.Models;

namespace Econolite.Ode.Services.Messenger;

/// <summary>
/// IMessengerService
/// </summary>
public interface IMessengerService
{
    /// <summary>
    /// SendSmsMessageAsync
    /// </summary>
    /// <param name="smsMessage"></param>
    /// <returns></returns>
    Task SendSmsMessageAsync(SmsMessage smsMessage);

    /// <summary>
    /// SendSmsMessageAsync
    /// </summary>
    /// <param name="smsMessage"></param>
    /// <param name="twilioConfig"></param>
    /// <returns></returns>
    Task SendSmsMessageAsync(SmsMessage smsMessage, TwilioConfig twilioConfig);
}
