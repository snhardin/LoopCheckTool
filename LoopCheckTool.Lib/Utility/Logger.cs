using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopCheckTool.Lib.Utility
{
    public static class Logger
    {
        private static bool configLoaded = false;
        public static log4net.ILog GetOrLoadLogger(Type type)
        {
            if (!configLoaded)
            {
                FileInfo configFileInfo = new FileInfo("log4net.config");
                log4net.Config.XmlConfigurator.ConfigureAndWatch(configFileInfo);
                configLoaded = true;
            }

            return log4net.LogManager.GetLogger(type);
        }
    }
}
