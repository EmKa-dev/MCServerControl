using McServerControlAPI.Models;
using McServerControlAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace McServerControlAPI.Controllers
{
    [Route("ServerControls")]
    public class ServerControlsController : Controller
    {
        private static IMinecraftServerService _MinecraftServerService;

        public ServerControlsController(ILogger<ServerControlsController> logger, IMinecraftServerService serverService)
        {
            _MinecraftServerService = serverService;
        }

        // GET :ServerControls/Index
        [Route("")]
        [Route("~/")]
        [Route("Index")]
        public IActionResult Index()
        {

            ViewData["Title"] = ConfigReader.GetConfigProperty("Title");
            ViewData["TwitchIncluded"] = ConfigReader.GetConfigProperty("IncludeTwitchMCProfile");

            return View("ServerControls");
        }

        [HttpGet("StartServer/{password}")]
        public IActionResult StartServer(string password)
        {
            if (ConfigReader.GetConfigProperty("Password") == password)
            {
                _MinecraftServerService.StartServer();
            }
            else
            {
                TempData["Message"] = "Wrong password";
            }

            return RedirectToAction("Index");
        }

        [HttpGet("StopServer/{password}")]
        public IActionResult StopServer(string password)
        {

            if (ConfigReader.GetConfigProperty("Password") == password)
            {
                _MinecraftServerService.StopServer();
            }
            else
            {
                TempData["Message"] = "Wrong password";
            }

            return RedirectToAction("Index");
        }

        [HttpGet("GetStatus")]
        public JsonResult GetStatus()
        {
            var j = new
            {
                Status = Enum.GetName(typeof(ServerStatus), _MinecraftServerService.ServerStatus),
                IsInputPossible = _MinecraftServerService.IsInputPossible
            };
            
            return Json(j, new JsonSerializerOptions());
        }
    }
}