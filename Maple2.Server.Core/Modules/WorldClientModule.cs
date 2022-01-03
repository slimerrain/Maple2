﻿using System;
using System.Net.Http;
using Grpc.Net.ClientFactory;
using Maple2.Server.Core.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Maple2.Server.Core.Modules;

public class WorldClientModule : GrpcClientModule {
    public override void Configure(IServiceCollection services) {
        services.AddGrpcClient<World.Service.World.WorldClient>(Options);
    }

    protected override void Options(GrpcClientFactoryOptions options) {
        options.Address = new Uri($"https://{Target.GRPC_WORLD_IP}:{Target.GRPC_WORLD_PORT}");
        options.ChannelOptionsActions.Add(chOptions => {
            // Return "true" to allow certificates that are untrusted/invalid
            chOptions.HttpHandler = new HttpClientHandler {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
        });
    }
}
