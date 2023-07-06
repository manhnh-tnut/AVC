using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AVC.DatabaseModels;
using AVC.Hubs;
using AVC.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

[Route("service-center")]
[ApiController]
public class ServiceCenterController : ControllerBase
{
    private readonly ILogger<ServiceCenterController> _logger;
    private readonly IHubContext<HubService, IHubService> _hubContext;
    private readonly IActionContextAccessor _accessor;
    private readonly IMachineService _machineService;
    private readonly ISummaryService _summaryService;
    private readonly ILogService _logService;
    private readonly string ip;

    public ServiceCenterController(ILogger<ServiceCenterController> logger,
            IHubContext<HubService, IHubService> hubContext,
            IActionContextAccessor accessor,
            IMachineService machineService,
            ISummaryService summaryService,
            ILogService logService)
    {
        _logger = logger;
        _hubContext = hubContext;
        _accessor = accessor;
        _machineService = machineService;
        _summaryService = summaryService;
        _logService = logService;

        // System.Net.IPAddress remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
        ip = _accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString();
    }

    [HttpGet("current-time")]
    public IActionResult GetTime()
    {
        var time = DateTime.Now;
        return new JsonResult(new { time.Year, time.Month, date = time.ToShortDateString(), time.Hour, time.Minute, time.Second });
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] GPIO gpio)
    {
        var machine = (await _machineService.GetsAsync(Builders<Machine>.Filter.Where(i => i.ip == ip && i.gpio.Any(i => i.port == gpio.port)))).FirstOrDefault();
        if (machine == null)
        {
            return NotFound();
        }

        if (!TryValidateModel(gpio))
            return BadRequest(ModelState.Values);

        var index = machine.gpio.FindIndex(i => i.port == gpio.port);
        if (gpio.value < 0 || gpio.value > 1 || index < 0)
        {
            return BadRequest();
        }

        if (machine.gpio[index].value == gpio.value)
        {
            return Ok();
        }
        else
        {
            machine.gpio[index].value = gpio.value;
            var log = new Log()
            {
                name = machine.name,
                gpio = machine.gpio[index],
            };
            await _logService.CreateAsync(log);
            await _hubContext.Clients.All.Log(log);

            machine.timeUpdate = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
            var date = ((DateTimeOffset)DateTime.Now.Date).ToUnixTimeSeconds();
            var summary = (await _summaryService
                            .GetsAsync(Builders<Summary>.Filter.Where(i => i.machine.name == machine.name && !(i.timeCreate < date)),
                                        new FindOptions<Summary, Summary>() { Limit = 1, Sort = Builders<Summary>.Sort.Descending(i => i.timeCreate) }))
                            .FirstOrDefault();

            if (summary == null)
            {
                summary = new Summary()
                {
                    machine = machine
                };
                await _summaryService.CreateAsync(summary);
            }

            switch (machine.gpio[index].type)
            {
                case GPIO_TYPE.COUTER:
                    if (machine.gpio[index].value == 0)
                    {
                        summary.count++;
                    }
                    break;
                case GPIO_TYPE.POWER:
                    if (machine.gpio[index].value == 1)
                    {
                        machine.status = false;
                        var _log = (await _logService
                            .GetsAsync(Builders<Log>.Filter.Where(i => i.gpio.value == 0 && i.name == machine.name && i.gpio.port == machine.gpio[index].port && !(i.timeCreate < date)),
                                        new FindOptions<Log, Log>() { Limit = 1, Sort = Builders<Log>.Sort.Descending(i => i.timeCreate) }))
                            .FirstOrDefault();
                        if (_log != null)
                        {
                            summary._time += (log.timeCreate - _log.timeCreate);
                        }

                    }
                    else
                    {
                        machine.status = true;
                    }
                    summary.machine = machine;
                    break;
                default:
                    break;
            }
            await _summaryService.UpdateByIdAsync(summary.id, summary);
            await _machineService.UpdateByIdAsync(machine.id, machine);
        }

        try
        {
            var date = ((DateTimeOffset)DateTime.Now.Date).ToUnixTimeSeconds();
            var summaries = new List<Summary>();

            foreach (var _machine in (await _machineService.GetsAsync()))
            {
                var summary = (await _summaryService.GetsAsync(Builders<Summary>.Filter.Where(i => i.machine.name == _machine.name && !(i.timeCreate < date)),
                                                            new FindOptions<Summary, Summary>() { Limit = 1 })).FirstOrDefault();
                if (summary == null)
                {
                    summary = new Summary() { machine = _machine };
                }
                if (_machine.status)
                {
                    var log = (await _logService
                                    .GetsAsync(Builders<Log>.Filter.Where(i => i.gpio.value == 0 && i.gpio.type == GPIO_TYPE.POWER && i.name == _machine.name && !(i.timeCreate < date)),
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

        return Ok();
    }
}