using System;

namespace OSExp.Logger
{
    class LogManager
    {
        public static LogLevel Level { get; set; } = LogLevel.Info;
        public static ILogger GetLogger(Type type)
        {
            return new ConsoleLogger(type);
        }
    }
}
