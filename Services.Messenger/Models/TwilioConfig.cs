// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Services.Messenger.Models;

public class TwilioConfig
{
    public Guid Id { get; set; }
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string SenderPhone { get; set; } = string.Empty;
}
