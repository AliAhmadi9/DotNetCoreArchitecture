using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using WebFramework.Api;
using WebFramework.Configuration;

namespace Web.UI.Controllers
{
    public class HomeController : BaseMvcController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}