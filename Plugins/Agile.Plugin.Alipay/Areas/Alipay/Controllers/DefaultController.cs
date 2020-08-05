using Agile.Web.Framework.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Plugin.Alipay.Areas.Alipay.Controllers
{
    [Area("Alipay")]
    public class DefaultController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
