using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AVC.Models;
using AVC.Interfaces;
using System.Threading.Tasks;
using System;
using AVC.DatabaseModels;
using MongoDB.Driver;
using System.Linq;
using System.Collections.Generic;

namespace AVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISummaryService _summaryService;
        private readonly IMachineService _machineService;
        private readonly ILogService _logService;

        public HomeController(ILogger<HomeController> logger,
        ISummaryService summaryService,
        IMachineService machineService,
        ILogService logService)
        {
            _logger = logger;
            _summaryService = summaryService;
            _machineService = machineService;
            _logService = logService;
        }
        public IActionResult Index() => RedirectToAction("Page");

        [HttpGet("[controller]/machines")]
        public async Task<IActionResult> Machines()
        {
            return Ok(await _machineService.GetsAsync());
        }

        [ValidateAntiForgeryToken]
        [HttpPost("[controller]/logs")]
        public async Task<IActionResult> Logs(string name)
        {
            return Ok(await _logService
                            .GetsAsync(Builders<Log>.Filter.Where(i => i.gpio.type == GPIO_TYPE.POWER && i.name == name),
                                        new FindOptions<Log, Log>() { Limit = 5, Sort = Builders<Log>.Sort.Descending(i => i.timeCreate) }));
        }

        [HttpGet("[controller]/summaries")]
        public async Task<IActionResult> Summaries()
        {
            List<Summary> summaries = new List<Summary>();
            try
            {
                var date = ((DateTimeOffset)DateTime.Now.Date).ToUnixTimeSeconds();
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
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
            }
            return Json(summaries);
        }

        [HttpGet("[controller]/report")]
        public IActionResult Report()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost("[controller]/report")]
        public async Task<IActionResult> Report(string from, string to)
        {
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            {
                return BadRequest();
            }
            var _from = ((DateTimeOffset)Convert.ToDateTime(from)).ToUnixTimeSeconds();
            var _to = ((DateTimeOffset)Convert.ToDateTime(to).AddDays(1)).ToUnixTimeSeconds();
            return Ok(await _summaryService.GetsAsync(Builders<Summary>.Filter.Where(i => !(i.timeCreate < _from) && !(i.timeCreate > _to)),
                                                                new FindOptions<Summary, Summary>() { Sort = Builders<Summary>.Sort.Ascending(i => i.timeCreate) }));
        }
        public IActionResult Page()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
