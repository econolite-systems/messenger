// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Microsoft.AspNetCore.Mvc;
using Econolite.Ode.Authorization;
using Econolite.Ode.Services.Messenger;
using Econolite.Ode.Services.Messenger.Models;

namespace Econolite.Ode.Api.Messenger.Controllers;

/// <summary>
/// MessengerController
/// </summary>
[ApiController]
[Route("messenger")]
[AuthorizeOde(MoundRoadRole.Administrator)]
public class MessengerController : ControllerBase
{
    private readonly ILogger<MessengerController> _logger;

    private readonly IMessengerService _messengerService;

    /// <summary>
    /// MessengerController
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="messengerService"></param>
    public MessengerController(ILogger<MessengerController> logger, IMessengerService messengerService)
    {
        _logger = logger;
        _messengerService = messengerService;
    }

    /// <summary>
    /// SendSmsMessage
    /// </summary>
    /// <param name="smsMessage"></param>
    /// <returns></returns>
    [HttpPut("send-sms-message")]
    [AuthorizeOde(MoundRoadRole.Contributor)]
    public async Task<IActionResult> SendSmsMessageAsync([FromBody] SmsMessage smsMessage)
    {
        try
        {
            await _messengerService.SendSmsMessageAsync(smsMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to Send an SMS Message");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to Send an SMS Message");
        }

        return Ok();
    }
}
