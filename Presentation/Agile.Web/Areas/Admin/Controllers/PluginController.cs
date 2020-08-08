using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agile.Core;
using Agile.Core.Infrastructure;
using Agile.Models.Plugins;
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
            return View();
        }

        public IActionResult GetData(PluginSearchModel search, int pageIndex, int pageSize)
        {
            var datas = _pluginService.GetPluginModels(search, pageIndex, pageSize, out int total);
            return Json(new
            {
                code = 0,
                msg = "ok",
                count = total,
                data = datas
            });
        }

        public IActionResult GetIsRestartRequired()
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
            var isRestartRequired = _pluginService.ChangePlugin(pluginDescriptor.SystemName);
            if (isRestartRequired)
            {
                return Content("重启状态已存在，请在重启系统后再操作！");
            }
            _pluginService.PreparePluginToInstall(pluginDescriptor.SystemName);
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
            var isRestartRequired = _pluginService.ChangePlugin(pluginDescriptor.SystemName);
            if (isRestartRequired)
            {
                return Content("重启状态已存在，请在重启系统后再操作！");
            }
            _pluginService.PreparePluginToUninstall(pluginDescriptor.SystemName);
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
            var isRestartRequired = _pluginService.ChangePlugin(pluginDescriptor.SystemName);
            if (isRestartRequired)
            {
                return Content("重启状态已存在，请在重启系统后再操作！");
            }
            _pluginService.PreparePluginToDelete(pluginDescriptor.SystemName);
            return Content("ok");
        }
    }
}
