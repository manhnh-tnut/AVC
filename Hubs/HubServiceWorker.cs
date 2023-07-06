using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AVC.DatabaseModels;
using AVC.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace AVC.Hubs
{
    public class HubServiceWorker : BackgroundService
    {
        private readonly ILogger<HubServiceWorker> _logger;
        private readonly IHubContext<HubService, IHubService> _hubContext;
        private readonly ILogService _logService;
        private readonly ISummaryService _summaryService;
        private readonly IMachineService _machineService;

        public HubServiceWorker(ILogger<HubServiceWorker> logger,
        IHubContext<HubService, IHubService> hubContext,
        ISummaryService summaryService,
        ILogService logService,
        IMachineService machineService)
        {
            _logger = logger;
            _hubContext = hubContext;
            _logService = logService;
            _summaryService = summaryService;
            _machineService = machineService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var date = ((DateTimeOffset)DateTime.Now.Date).ToUnixTimeSeconds();
                    var summaries = new List<Summary>();

                    foreach (var machine in (await _machineService.GetsAsync()))
                    {
                        var summary = (await _summaryService.GetsAsync(Builders<Summary>.Filter.Where(i => i.machine.name == machine.name && !(i.timeCreate < date)),
                                                            new FindOptions<Summary, Summary>() { Limit = 1 })).FirstOrDefault();
                        if (summary == null)
                        {
                            summary = new Summary() { machine = machine };
                        }
                        if (machine.status)
                        {
                            var log = (await _logService
                                            .GetsAsync(Builders<Log>.Filter.Where(i => i.gpio.value == 0 && i.gpio.type == GPIO_TYPE.POWER && i.name == summary.machine.name && !(i.timeCreate < date)),
                                                        new FindOptions<Log, Log>() { Limit = 1, Sort = Builders<Log>.Sort.Descending(i => i.timeCreate) }))
                                            .FirstOrDefault();
                            if (log != null)
                            {
                                summary._time += (((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds() - log.timeCreate);
                            }
                        }
                        summaries.Add(summary);
                    }
                    await _hubContext.Clients.All.Summaries(summaries);
                }
                catch (System.Exception e)
                {
                    _logger.LogError(e.Message);
                }
                await Task.Delay(1000);
            }
        }
    }
}