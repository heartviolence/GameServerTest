using ServerShared.Database;
using ServerShared.Redis;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();
builder.Services.AddMagicOnion();
builder.Services.AddSingleton<RedisSettings>();
builder.Services.AddSingleton<DbSettings>(sp =>
{
    var env = DbSettings.Env.LOCAL;
#if (TEST || FINAL)
    env = DbSettings.Env.FINAL;
#endif
    return new DbSettings(sp.GetService<ILogger<DbSettings>>(), env);
});

builder.Logging.AddConsole();
var app = builder.Build();

app.MapMagicOnionService();
app.MapGet("/", () => "Hello World!");

app.UseStaticFiles(new StaticFileOptions()
{
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});

X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
store.Open(OpenFlags.ReadWrite);
store.Add(new X509Certificate2(X509Certificate2.CreateFromCertFile("Cert/ca.crt")));
store.Close();

app.Run();
