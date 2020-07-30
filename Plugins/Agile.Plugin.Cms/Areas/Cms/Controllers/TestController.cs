using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Plugin.Cms.Areas.Cms.Controllers
{
    [Area("Cms")]
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
