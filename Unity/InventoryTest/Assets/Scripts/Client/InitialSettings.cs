
using Cysharp.Net.Http;
using Grpc.Net.Client;
using MagicOnion.Unity;
using UnityEngine;

public class InitialSettings
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void OnRuntimeInitialize()
    {
        //GrpcChannelProviderHost.Initialize(new GrpcNetClientGrpcChannelProvider(new GrpcChannelOptions()
        //{
        //    HttpHandler = new YetAnotherHttpHandler()
        //    {
        //        SkipCertificateVerification = true // for test
        //    },
        //    DisposeHttpClient = true
        //}));
        GrpcChannelProviderHost.Initialize(new GrpcNetClientGrpcChannelProvider(new GrpcChannelOptions()
        {
            HttpHandler = new YetAnotherHttpHandler()
            {
                SkipCertificateVerification = false
            },
            DisposeHttpClient = true
        }));
    }

}