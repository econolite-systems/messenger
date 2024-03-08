// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Services.Messenger.Models;

/// <summary>
/// SmsMessage
/// </summary>
public class SmsMessage
{
    /// <summary>
    /// RecipientPhone
    /// </summary>
    public string RecipientPhone { get; set; } = string.Empty;

    /// <summary>
    /// MessageText
    /// </summary>
    public string MessageText { get; set; } = string.Empty;
}
