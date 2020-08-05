using System;
using System.Collections.Generic;
using System.Reflection;

namespace Agile.Core.Infrastructure
{
    public class WebAppTypeFinder : AppDomainTypeFinder
    {

        private bool _binFolderAssembliesLoaded;


        public WebAppTypeFinder(IAgileFileProvider fileProvider = null) : base(fileProvider)
        {
        }

        public bool EnsureBinFolderAssembliesLoaded { get; set; } = true;

        public virtual string GetBinDirectory()
        {
            return AppContext.BaseDirectory;
        }

        public override IList<Assembly> GetAssemblies()
        {
            if (!EnsureBinFolderAssembliesLoaded || _binFolderAssembliesLoaded)
            {
                return base.GetAssemblies();
            }

            _binFolderAssembliesLoaded = true;
            var binPath = GetBinDirectory();
            LoadMatchingAssemblies(binPath);

            return base.GetAssemblies();
        }
    }
}
