using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AVC.DatabaseModels;

namespace AVC.Hubs
{
    public interface IHubService
    {
        Task Log(Log log);
        Task Summaries(IEnumerable<Summary> summaries);
        Task OnClientConnected(IEnumerable<Machine> machines);
    }
}