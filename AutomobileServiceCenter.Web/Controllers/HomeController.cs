using AutomobileServiceCenter.Utilities;
using AutomobileServiceCenter.Web.Configuration;
using AutomobileServiceCenter.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;


namespace AutomobileServiceCenter.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOptions<ApplicationSettings> _settings;

        public HomeController(IOptions<ApplicationSettings> settings)
        {
            _settings = settings;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Index()
        {
            // Set Session
            HttpContext.Session.SetSession("Test", _settings.Value);

            // Get Session
            var settings = HttpContext.Session.GetSession<ApplicationSettings>("Test");

            ViewBag.Title = _settings.Value.ApplicationTitle;

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
