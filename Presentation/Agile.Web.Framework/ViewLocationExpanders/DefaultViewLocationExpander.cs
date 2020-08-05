using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agile.Web.Framework.ViewLocationExpanders
{
    public class DefaultViewLocationExpander : IViewLocationExpander
    {
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            viewLocations = new[]
            {
                $"/Plugins/Agile.Plugin.{context.AreaName}/Areas/{context.AreaName}/Views/{{1}}/{{0}}.cshtml",
                $"/Plugins/Agile.Plugin.{context.AreaName}/Areas/{context.AreaName}/Views/Shared/{{0}}.cshtml",
            }.Concat(viewLocations);
            return viewLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }
    }
}
