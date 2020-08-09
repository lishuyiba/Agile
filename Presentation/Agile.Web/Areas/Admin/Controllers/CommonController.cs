using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agile.Core;
using Agile.Web.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Web.Areas.Admin.Controllers
{
    public class CommonController : BaseAdminController
    {
        private readonly IWebHelper _webHelper;

        public CommonController(IWebHelper webHelper)
        {
            _webHelper = webHelper;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RestartApplication()
        {
            _webHelper.RestartAppDomain();

            return new EmptyResult();
        }

        public IActionResult CheckRestart()
        {
            return new EmptyResult();
        }

        public IActionResult PageNotFound()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult Theme()
        {
            return View();
        }
    }
}
