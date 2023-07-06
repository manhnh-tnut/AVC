using System;
using System.Threading.Tasks;

namespace AVC.Hubs
{
    public static class HubServiceConfig
    {
        public static string HubUrl => "/hubs/live";

        public static class Events
        {
            public static string Log => nameof(IHubService.Log);
            public static string Summaries => nameof(IHubService.Summaries);
            public static string OnClientConnected => nameof(IHubService.OnClientConnected);
        }
    }
}