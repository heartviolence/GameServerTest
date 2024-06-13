using CloudStructures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ServerShared.Redis
{
    public class RedisSettings
    {
        public RedisConnection Connection { get; }
        ILogger logger;
        public RedisSettings(IConfiguration configuration, ILogger<RedisSettings> logger)
        {
            this.logger = logger;
            string connectionString = configuration["redisConnectionString"];
            logger.LogInformation($"RedisConnectionString: {connectionString}");
            var config = new RedisConfig("RedisConnectionString", connectionString);
            this.Connection = new RedisConnection(config);
        }
    }
}
