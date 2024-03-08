// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Services.Messenger.Extensions;
using Econolite.Ode.Extensions.AspNet;
using Econolite.Ode.Router.ActionSet.Messaging.Extensions;
using Econolite.Ode.Authorization.Extensions;
using Econolite.Ode.Services.Messenger.Consumers;

await AppBuilder.BuildAndRunWebHostAsync(args, options => { options.Source = "Messenger"; options.IsApi = true; }, (builder, services) =>
{
    services.AddMessengerService();
    //services.AddKafkaConsumerConfig(builder.Configuration);
    //services.AddSmsMessageConsumer();
    services.AddHttpClient();
    builder.Services.AddTokenHandler(options =>
    {
        options.Authority = builder.Configuration.GetValue("Authentication:Authority",
            "https://keycloak.cosysdev.com/auth/realms/moundroad")!;
        options.ClientId = builder.Configuration.GetValue("Authentication:ClientId", "")!;
        options.ClientSecret = builder.Configuration.GetValue("Authentication:ClientSecret", "")!;
    });
    builder.Services.AddActionSetRouterSource(builder.Configuration);
    builder.Services.AddHostedService<ActionRequestHandler>();

});
