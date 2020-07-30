using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agile.Core.Infrastructure;
using Agile.Services.Plugins;
using Agile.Web.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Web.Areas.Admin.Controllers
{
    [Area(AreaNames.Admin)]
    public class PluginController : Controller
    {
        private readonly IPluginService _pluginService;
        private readonly IUploadService _uploadService;
        private readonly IPluginsInfo _pluginsInfo;

        public PluginController(IPluginService pluginService, IUploadService uploadService)
        {
            _pluginService = pluginService;
            _uploadService = uploadService;
            _pluginsInfo = Singleton<IPluginsInfo>.Instance;
        }
        public IActionResult List()
        {
            return View();
        }

        public IActionResult GetData()
        {
            var results = new List<object>();
            var pluginDescriptors = _pluginService.GetPluginDescriptors().ToList();
            foreach (var pluginDescriptor in pluginDescriptors)
            {
                var item = new
                {
                    pluginDescriptor.Author,
                    pluginDescriptor.SystemName,
                    pluginDescriptor.Description,
                    State = pluginDescriptor.Installed == true ? "已安装" : "未安装"
                };
                results.Add(item);
            }
            return Json(new
            {
                code = 0,
                msg = "",
                count = results.Count,
                data = results
            });
        }

        public IActionResult ApplyChanges()
        {
            _pluginService.UninstallPlugins();

            return new EmptyResult();
        }

        public IActionResult IsRestartRequired()
        {
            var result = _pluginService.IsRestartRequired();
            return Json(result);
        }

        public IActionResult Install(string systemName)
        {
            var pluginDescriptor = _pluginService.GetPluginDescriptorBySystemName(systemName);
            if (pluginDescriptor == null)
            {
                return View("List");
            }

            if (pluginDescriptor.Installed)
            {
                return View("List");
            }
            _pluginService.PreparePluginToInstall(pluginDescriptor.SystemName);
            pluginDescriptor.ShowInPluginsList = false;

            return Content("ok");
        }

        public IActionResult UnInstall(string systemName)
        {
            var pluginDescriptor = _pluginService.GetPluginDescriptorBySystemName(systemName);
            if (pluginDescriptor == null)
                return View("List");

            //check whether plugin is installed
            if (!pluginDescriptor.Installed)
                return View("List");

            _pluginService.PreparePluginToUninstall(pluginDescriptor.SystemName);
            pluginDescriptor.ShowInPluginsList = false;

            return Content("ok");
        }

        public IActionResult UploadPlugins()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UploadPlugins(IFormFile archivefile)
        {
            var descriptors = _uploadService.UploadPlugins(archivefile);


            foreach (var descriptor in descriptors)
            {
                if (_pluginsInfo.PluginDescriptors.Where(s => s.SystemName.Contains(descriptor.SystemName)).ToList().Any())
                {
                    continue;
                }
                _pluginsInfo.PluginDescriptors.Add(descriptor);
                //_pluginService.PreparePluginToInstall(descriptor.SystemName);
                //_pluginService.InstallPlugins();
            }


            return Content("ok");
        }

        public IActionResult Delete(string systemName)
        {
            var pluginDescriptor = _pluginService.GetPluginDescriptorBySystemName(systemName);
            if (pluginDescriptor.Installed)
                return View("List");

            _pluginService.PreparePluginToDelete(pluginDescriptor.SystemName);
            pluginDescriptor.ShowInPluginsList = false;

            return Content("ok");
        }
    }
}
