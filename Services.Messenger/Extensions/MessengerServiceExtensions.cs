// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Microsoft.Extensions.DependencyInjection;
using Econolite.Ode.Services.Messenger.Consumers;

namespace Econolite.Ode.Services.Messenger.Extensions;

/// <summary>
/// MessengerServiceExtensions
/// </summary>
public static class MessengerServiceExtensions
{
    /// <summary>
    /// AddMessengerService
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddMessengerService(this IServiceCollection services)
    {
        services.AddSingleton<IMessengerService, MessengerService>();
        return services;
    }
    
    /// <summary>
    /// AddSmsMessageConsumer
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSmsMessageConsumer(this IServiceCollection services)
    {
        services.AddHostedService<SmsMessageConsumer>();
        return services;
    }
}
