using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Core.Logging
{
    public interface ILogger
    {
        void Information(Type type, string message, Exception exception = null);

        void Warning(Type type, string message, Exception exception = null);

        void Error(Type type, string message, Exception exception = null);

        void Debug(Type type, string message, Exception exception = null);
    }
}
