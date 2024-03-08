// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Services.Messenger.Models;

public class UserModel
{
    public Guid Id { get; set; }
    public long CreatedTimestamp { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public Dictionary<string, string[]> Attributes { get; set; } = new Dictionary<string, string[]>();
}
