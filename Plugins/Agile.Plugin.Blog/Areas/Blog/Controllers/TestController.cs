using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Plugin.Blog.Areas.Cms.Controllers
{
    [Area("Blog")]
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
