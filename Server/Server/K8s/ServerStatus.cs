using Agones;
using Amazon.Route53;
using Amazon.Route53.Model;
using k8s;
using k8s.Models;
using Server.GameSystems;
using ServerShared.K8sResourceDefinition;

namespace Server.K8s
{
    public class ServerStatus
    {
        public string Address { get; set; } = "https://localhost:9999";               

        public ServerStatus()
        {
            
        }
    }
}
