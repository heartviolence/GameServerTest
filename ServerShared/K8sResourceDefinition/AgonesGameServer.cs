using System.Text.Json.Serialization;

namespace ServerShared.K8sResourceDefinition
{
    public class AgonesGameServer : CustomResource<AgonesGameServerSpec, AgonesGameServerStatus>
    {

    }

    public class AgonesGameServerSpec
    {

    }

    public class AgonesGameServerStatus
    {
        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("ports")]
        public List<AgonesGameServerPort> Ports { get; set; }
        [JsonPropertyName("state")]
        public string State { get; set; }
    }

    public class AgonesGameServerPort
    {

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }
    }
}
