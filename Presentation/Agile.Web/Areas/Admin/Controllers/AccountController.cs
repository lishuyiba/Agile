using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agile.Web.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Web.Areas.Admin.Controllers
{
    [Area(AreaNames.Admin)]
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
