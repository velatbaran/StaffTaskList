using Microsoft.AspNetCore.Mvc;
using StaffTaskList.UI.Models;
using System.Diagnostics;

namespace StaffTaskList.UI.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
        }

        [Route("anasayfa")]
        public IActionResult Index()
        {
            return View();
        }

    }
}
