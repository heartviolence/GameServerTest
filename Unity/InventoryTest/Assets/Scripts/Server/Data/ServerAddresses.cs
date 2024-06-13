
namespace Shared.Data
{
    public class ServerAddresses
    {
#if (FINAL || TEST)
        static public string LoginAndMatchServer { get; set; } = "https://matchserver.fireflyagonestest.com:7777";
#else
        static public string LoginAndMatchServer { get; set; } = "https://localhost:7777";
#endif        
    }
}