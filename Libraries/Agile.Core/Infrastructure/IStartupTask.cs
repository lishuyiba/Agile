using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Core.Infrastructure
{
    public interface IStartupTask
    {
        void Execute();

        int Order { get; }
    }
}
