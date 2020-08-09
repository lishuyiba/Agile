using Agile.Core.Infrastructure;
using log4net;
using log4net.Config;
using log4net.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Agile.Core.Logging
{
    public class DefaultLogger : ILogger
    {
        public static ILoggerRepository LoggerRepository { get; set; }

        public DefaultLogger()
        {
            LoggerRepository = LogManager.CreateRepository(LoggerDefaults.Repository);
            XmlConfigurator.Configure(LoggerRepository, new FileInfo("Config/log4net.config"));
            BasicConfigurator.Configure(LoggerRepository);
        }

        public void Error(Type type, string message, Exception exception = null)
        {
            ILog log = LogManager.GetLogger(LoggerDefaults.Repository, type);
            log.Error(message, exception);
        }

        public void Information(Type type, string message, Exception exception = null)
        {
            ILog log = LogManager.GetLogger(LoggerDefaults.Repository, type);
            log.Info(message, exception);
        }

        public void Warning(Type type, string message, Exception exception = null)
        {
            ILog log = LogManager.GetLogger(LoggerDefaults.Repository, type);
            log.Warn(message, exception);
        }

        public void Debug(Type type, string message, Exception exception = null)
        {
            ILog log = LogManager.GetLogger(LoggerDefaults.Repository, type);
            log.Debug(message, exception);
        }
    }
}
