﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Web.Areas.Admin.Controllers
{
    public class PermissionController : BaseAdminController
    {
        public IActionResult List()
        {
            return View();
        }
    }
}
