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
    /// <summary>
    /// 插件控制器
    /// </summary>
    public class PluginController : BaseAdminController
    {
        #region 字段
        /// <summary>
        /// 插件服务
        /// </summary>
        private readonly IPluginService _pluginService;

        /// <summary>
        /// 上传服务
        /// </summary>
        private readonly IUploadService _uploadService;

        /// <summary>
        /// 插件信息
        /// </summary>
        private readonly IPluginsInfo _pluginsInfo;

        /// <summary>
        /// web帮助
        /// </summary>
        private readonly IWebHelper _webHelper;
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pluginService">插件服务</param>
        /// <param name="uploadService">上传服务</param>
        /// <param name="webHelper">web帮助</param>
        public PluginController(IPluginService pluginService, IUploadService uploadService, IWebHelper webHelper)
        {
            _webHelper = webHelper;
            _pluginService = pluginService;
            _uploadService = uploadService;
            _pluginsInfo = Singleton<IPluginsInfo>.Instance;
        }
        #endregion

        #region 方法

        /// <summary>
        /// 列表
        /// </summary>
        /// <returns>视图</returns>
        public IActionResult List()
        {
            return View();
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="search">搜索数据模型</param>
        /// <param name="pageIndex">页面索引</param>
        /// <param name="pageSize">页面大小</param>
        /// <returns>Json数据</returns>
        public IActionResult GetData(PluginSearchModel search, int pageIndex, int pageSize)
        {
            var datas = _pluginService.GetPluginModels(search, pageIndex, pageSize, out int total);

            return SuccessJson(datas, total);
        }

        /// <summary>
        /// 获取重启系统标识
        /// </summary>
        /// <returns>Json数据</returns>
        public IActionResult CheckIsRestartRequired()
        {
            var result = _pluginService.CheckIsRestartRequired();
            return SuccessJson(result);
        }

        /// <summary>
        /// 安装插件
        /// </summary>
        /// <param name="systemName">系统名称</param>
        /// <returns>Json数据</returns>
        public IActionResult Install(string systemName)
        {
            var pluginDescriptor = _pluginService.GetPluginDescriptorBySystemName(systemName);
            if (pluginDescriptor == null)
            {
                return ErrorJson("插件不存在！");
            }
            if (pluginDescriptor.Installed)
            {
                return ErrorJson("插件已安装，请勿重新安装！");
            }
            var actionReuslt = CheckPlugin(systemName);
            if (actionReuslt != null)
            {
                return actionReuslt;
            }
            _pluginService.PreparePluginToInstall(pluginDescriptor.SystemName);
            return SuccessJson("操作成功！");
        }

        /// <summary>
        /// 卸载插件
        /// </summary>
        /// <param name="systemName">系统名称</param>
        /// <returns>Json数据</returns>
        public IActionResult UnInstall(string systemName)
        {
            var pluginDescriptor = _pluginService.GetPluginDescriptorBySystemName(systemName);
            if (pluginDescriptor == null)
            {
                return ErrorJson("插件不存在！");
            }
            if (!pluginDescriptor.Installed)
            {
                return ErrorJson("插件未安装，无需卸载！");
            }
            var actionReuslt = CheckPlugin(systemName);
            if (actionReuslt != null)
            {
                return actionReuslt;
            }
            _pluginService.PreparePluginToUninstall(pluginDescriptor.SystemName);
            return SuccessJson("操作成功！");
        }

        /// <summary>
        /// 上传插件页面
        /// </summary>
        /// <returns>视图</returns>
        public IActionResult UploadPlugins()
        {
            return View();
        }

        /// <summary>
        /// 上传插件提交
        /// </summary>
        /// <param name="archivefile">文件</param>
        /// <returns>Json数据</returns>
        [HttpPost]
        public IActionResult UploadPlugins(IFormFile archivefile)
        {
            var uploadResult = _uploadService.UploadPlugins(archivefile);
            if (uploadResult.descriptor != null)
            {
                var descriptor = uploadResult.descriptor;
                if (!_pluginsInfo.PluginDescriptors.Where(s => s.SystemName.Contains(descriptor.SystemName)).ToList().Any())
                {
                    _pluginsInfo.PluginDescriptors.Add(descriptor);
                    _pluginService.ClearPlugins(descriptor.SystemName);
                }
                if (uploadResult.IsUpdate)
                {
                    _pluginService.PreparePluginToUpdate(descriptor.SystemName);
                }
            }
            return SuccessJson("操作成功！");
        }

        /// <summary>
        /// 删除插件
        /// </summary>
        /// <param name="systemName">系统名称</param>
        /// <returns>Json数据</returns>
        public IActionResult Delete(string systemName)
        {
            var pluginDescriptor = _pluginService.GetPluginDescriptorBySystemName(systemName);
            if (pluginDescriptor.Installed)
            {
                return ErrorJson("插件已安装，无法删除！");
            }
            var actionReuslt = CheckPlugin(systemName);
            if (actionReuslt != null)
            {
                return actionReuslt;
            }
            _pluginService.PreparePluginToDelete(pluginDescriptor.SystemName);
            return SuccessJson("操作成功！");
        }

        /// <summary>
        /// 检查插件
        /// </summary>
        /// <param name="systemName">系统名称</param>
        /// <returns>结果，返回NULL表示检查通过，否则返回IActionResult对象</returns>
        private IActionResult CheckPlugin(string systemName)
        {
            var changePluginMessage = _pluginService.ChangePlugin(systemName);
            if (!string.IsNullOrWhiteSpace(changePluginMessage))
            {
                return ErrorJson(changePluginMessage);
            }
            return null;
        }
        #endregion
    }
}
