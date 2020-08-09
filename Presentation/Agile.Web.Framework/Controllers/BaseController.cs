using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Web.Framework.Controllers
{
    public class BaseController : Controller
    {
        protected JsonResult ErrorJson(string message = "error")
        {
            return Json(new
            {
                code = 1,
                msg = message
            });
        }

        public JsonResult SuccessJson(string message = "success")
        {
            return Json(new
            {
                code = 0,
                msg = message
            });
        }

        public JsonResult SuccessJson(object data)
        {
            return Json(new
            {
                code = 0,
                data = data
            });
        }

        protected JsonResult SuccessJson(object datas, int total)
        {
            return Json(new
            {
                code = 0,
                msg = "ok",
                count = total,
                data = datas
            });
        }
    }
}
