using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AVC.DatabaseModels;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AVC.Hubs
{
    public partial class HostedService : IHubService, IHostedService
    {
        private readonly ILogger<HostedService> _logger;
        private HubConnection _connection;
        public HostedService(ILogger<HostedService> logger)
        {
            _logger = logger;

            _connection = new HubConnectionBuilder()
                .WithUrl(HubServiceConfig.HubUrl)
                .Build();

            _connection.On<Log>(HubServiceConfig.Events.Log, Log);
            _connection.On<List<Summary>>(HubServiceConfig.Events.Summaries, Summaries);
            _connection.On<IEnumerable<Machine>>(HubServiceConfig.Events.OnClientConnected, OnClientConnected);
        }

        public Task Log(Log log)
        {
            return Task.CompletedTask;
        }
        public Task Summaries(IEnumerable<Summary> summaries)
        {
            return Task.CompletedTask;
        }
        public Task OnClientConnected(IEnumerable<Machine> machines)
        {
            return Task.CompletedTask;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Loop is here to wait until the server is running
            while (true)
            {
                try
                {
                    await _connection.StartAsync(cancellationToken);
                    break;
                }
                catch
                {
                    await Task.Delay(1000);
                }
            }
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _connection.DisposeAsync();
        }
    }
}