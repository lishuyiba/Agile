using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agile.Core;
using Agile.Core.Infrastructure;
using Agile.Services.Plugins;
using Agile.Web.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Agile.Web.Areas.Admin.Controllers
{
    public class PluginController : BaseAdminController
    {
        private readonly IPluginService _pluginService;
        private readonly IUploadService _uploadService;
        private readonly IPluginsInfo _pluginsInfo;
        private readonly IWebHelper _webHelper;

        public PluginController(IPluginService pluginService, IUploadService uploadService, IWebHelper webHelper)
        {
            _webHelper = webHelper;
            _pluginService = pluginService;
            _uploadService = uploadService;
            _pluginsInfo = Singleton<IPluginsInfo>.Instance;
        }
        public IActionResult List()
        {
            ViewBag.IsRestartRequired = _pluginService.IsRestartRequired();
            return View();
        }

        public IActionResult GetData()
        {
            var results = new List<object>();
            var pluginDescriptors = _pluginService.GetPluginDescriptors().ToList();
            foreach (var pluginDescriptor in pluginDescriptors)
            {
                var installed = _pluginsInfo.InstalledPlugins.Where(s => s.SystemName.Contains(pluginDescriptor.SystemName)).Any();
                var install = _pluginsInfo.PluginNamesToInstall.Where(s => s.Contains(pluginDescriptor.SystemName)).Any();
                var unInstall = _pluginsInfo.PluginNamesToUninstall.Where(s => s.Contains(pluginDescriptor.SystemName)).Any();
                var deleted = _pluginsInfo.PluginNamesToDelete.Where(s => s.Contains(pluginDescriptor.SystemName)).Any();
                var state = "未知";
                if (installed)
                {
                    state = "已安装";
                }
                if (installed && unInstall)
                {
                    state = "已安装（重启系统后将卸载）";
                }
                if (installed && deleted)
                {
                    state = "已安装（重启系统后将删除）";
                }
                if (!installed)
                {
                    state = "未安装";
                }
                if (!installed && deleted)
                {
                    state = "未安装（重启系统后将删除）";
                }
                if (!installed && install)
                {
                    state = "未安装（重启系统后将安装）";
                }
                var item = new
                {
                    pluginDescriptor.Author,
                    pluginDescriptor.SystemName,
                    pluginDescriptor.Description,
                    state
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
            _pluginService.DeletePlugins();
            _webHelper.RestartAppDomain();
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
                return Content("插件不存在！");
            }
            if (pluginDescriptor.Installed)
            {
                return Content("插件已安装，请勿重新安装！");
            }
            var unInstall = _pluginsInfo.PluginNamesToUninstall.Where(s => s.Contains(pluginDescriptor.SystemName)).Any();
            if (unInstall)
            {
                return Content("插件已在卸载队列，无法安装！");
            }
            var deleted = _pluginsInfo.PluginNamesToDelete.Where(s => s.Contains(pluginDescriptor.SystemName)).Any();
            if (deleted)
            {
                return Content("插件已在删除队列，无法安装！");
            }
            _pluginService.PreparePluginToInstall(pluginDescriptor.SystemName);
            pluginDescriptor.IsRestartActivate = true;

            return Content("ok");
        }

        public IActionResult UnInstall(string systemName)
        {
            var pluginDescriptor = _pluginService.GetPluginDescriptorBySystemName(systemName);
            if (pluginDescriptor == null)
            {
                return Content("插件不存在！");
            }
            if (!pluginDescriptor.Installed)
            {
                return Content("插件未安装，无需卸载！");
            }
            var deleted = _pluginsInfo.PluginNamesToDelete.Where(s => s.Contains(pluginDescriptor.SystemName)).Any();
            if (deleted)
            {
                return Content("插件已在删除队列，无法安装！");
            }
            _pluginService.PreparePluginToUninstall(pluginDescriptor.SystemName);
            pluginDescriptor.ShowInPluginsList = false;
            pluginDescriptor.IsRestartActivate = true;

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
                _pluginService.ClearPlugins(descriptor.SystemName);
            }
            return Content("ok");
        }

        public IActionResult Delete(string systemName)
        {
            var pluginDescriptor = _pluginService.GetPluginDescriptorBySystemName(systemName);
            if (pluginDescriptor.Installed)
            {
                return Content("插件已安装，无法删除！");
            }
            var unInstall = _pluginsInfo.PluginNamesToUninstall.Where(s => s.Contains(pluginDescriptor.SystemName)).Any();
            if (unInstall)
            {
                return Content("插件已在卸载队列，无法安装！");
            }
            _pluginService.PreparePluginToDelete(pluginDescriptor.SystemName);
            pluginDescriptor.ShowInPluginsList = false;

            return Content("ok");
        }
    }
}
