using Grpc.Net.Client;
using MagicOnion.Server.Redis;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Server.GameSystems.Items;
using Server.GameSystems.Player.Accounts;
using System.Net;
using Azure.Identity;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using Server.GameSystems.Sheets;
using MessagePipe;
using Server.GameSystems;
using Server.K8s;
using ServerShared.Redis;
using ServerShared.Database;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(endpointOptions =>
    {
        endpointOptions.Protocols = HttpProtocols.Http2;
    });
});

#if (TEST || FINAL)
builder.Services.AddHostedService<GameserverK8sTask>();
#endif
builder.Services.AddSingleton<AccountRepository>();
builder.Services.AddSingleton<RedisSettings>();
builder.Services.AddSingleton<GameSheets>();
builder.Services.AddSingleton<RoomInformations>();
builder.Services.AddSingleton<ServerStatus>();
builder.Services.AddSingleton<DbSettings>(sp =>
{
    var env = DbSettings.Env.LOCAL;
#if (TEST || FINAL)
    env = DbSettings.Env.FINAL;
#endif
    return new DbSettings(sp.GetService<ILogger<DbSettings>>(), env);
});

builder.Services.AddMessagePipe();
builder.Services.AddControllersWithViews();
builder.Services.AddGrpc();
builder.Services.AddMagicOnion();

builder.Logging.AddConsole();

var app = builder.Build();

app.UseRouting();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.MapMagicOnionService();

app.Run();
