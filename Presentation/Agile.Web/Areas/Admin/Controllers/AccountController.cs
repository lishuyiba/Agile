using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agile.Web.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Web.Areas.Admin.Controllers
{
    public class AccountController : BaseAdminController
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
