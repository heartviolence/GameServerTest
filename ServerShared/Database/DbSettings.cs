using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServerShared.Database
{
    public class DbSettings
    {
        public enum Env
        {
            LOCAL,
            TEST,
            FINAL
        }
        public class DBSecret
        {
            [JsonPropertyName("DefaultDBConnectionString")]
            public string connectionString { get; set; }
        }
        public string ConnectionString { get; private set; } = string.Empty;
        ILogger logger;
        public DbSettings(ILogger<DbSettings> logger, Env env)
        {
            this.logger = logger;
            ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=GameDatabase;Trusted_Connection=True;";
            if (Env.LOCAL != env)
            {
                InitializeSecretConnectionStringAsync().Wait();
            }
        }
        private async Task InitializeSecretConnectionStringAsync()
        {
            string secretName = "db";
            string region = "ap-northeast-2";

            IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
            };
            GetSecretValueResponse response;

            response = await client.GetSecretValueAsync(request);

            string secret = response.SecretString;

            var dbsecret = JsonSerializer.Deserialize<DBSecret>(secret);
            ConnectionString = dbsecret.connectionString;
        }
    }
}
