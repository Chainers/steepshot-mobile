using log4net.Config;
using System;

namespace Common.Logging
{
    public class LogManager
    {
        public static void InitLogger()
        {
            //XmlConfigurator.Configure();
        }

        public static ILog GetLogger(Type type)
        {
            return log4net.LogManager.GetLogger(type) as ILog;
        }
    }
}