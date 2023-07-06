using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AVC.Models;
using AVC.Interfaces;
using AVC.DatabaseModels;
using Newtonsoft.Json;
using System.Linq;
using MongoDB.Driver;

namespace AVC.Controllers
{
    public class SettingController : Controller
    {
        private readonly ILogger<SettingController> _logger;
        private readonly IMachineService _machineService;
        public SettingController(ILogger<SettingController> logger,
        IMachineService machineService)
        {
            _logger = logger;
            _machineService = machineService;
        }
        public IActionResult Index() => RedirectToAction("Page");
        public IActionResult Page()
        {
            return View();
        }

        [HttpGet("[controller]/machines")]
        public async Task<IActionResult> Machines()
        {
            return Ok(await _machineService.GetsAsync());
        }

        [HttpGet("[controller]/types")]
        public IActionResult Types()
        {
            return Ok(System.Enum.GetValues(typeof(GPIO_TYPE))
                        .Cast<GPIO_TYPE>()
                        .Select(i => new
                        {
                            id = (int)i,
                            name = i.ToString()
                        }));
        }

        [ValidateAntiForgeryToken]
        [HttpPost("[controller]/create-machine")]
        public async Task<IActionResult> CreateMachine(string values)
        {
            var machine = new Machine();
            JsonConvert.PopulateObject(values, machine);

            if (!TryValidateModel(machine))
                return BadRequest(ModelState.Values);
            if ((await _machineService.GetsAsync(Builders<Machine>.Filter.Where(i => i.name == machine.name))).Count() > 0)
            {
                return BadRequest("Lỗi! Trùng tên máy!");
            }
            await _machineService.CreateAsync(machine);
            return Ok();
        }

        [ValidateAntiForgeryToken]
        [HttpPut("[controller]/update-machine")]
        public async Task<IActionResult> UpdateMachine(string key, string values)
        {
            var machine = await _machineService.FindByIdAsync(key);
            if (machine == null)
            {
                return NotFound();
            }

            var _machine = JsonConvert.DeserializeObject<Machine>(values);
            machine.status = _machine.status;
            if (!machine.status)
            {
                machine.gpio = _machine.gpio;
                var index=machine.gpio.FindIndex(i => i.type == GPIO_TYPE.POWER);
                if (index!=-1)
                {
                    machine.gpio[index].value=1;
                }
            }
            if (!TryValidateModel(machine))
                return BadRequest(ModelState.Values);

            var sames = await _machineService.GetsAsync(Builders<Machine>.Filter.Where(i => i.id != machine.id && i.ip==machine.ip));
            if (machine.gpio != null && sames.Where(i => i.gpio != null && i.gpio.Any(p => machine.gpio.Any(g => g.port == p.port))).Count() > 0)
            {
                return BadRequest("Lỗi! Trùng số chân!");
            }
            await _machineService.UpdateByIdAsync(key, machine);
            return Ok();
        }

        [ValidateAntiForgeryToken]
        [HttpDelete("[controller]/delete-machine")]
        public async Task<IActionResult> DeleteMachine(string key)
        {
            await _machineService.DeleteByIdAsync(key);
            return Ok();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
