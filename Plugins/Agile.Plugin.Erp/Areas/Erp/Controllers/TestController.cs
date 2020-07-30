using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Plugin.Erp.Areas.Erp.Controllers
{
    [Area("Erp")]
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
