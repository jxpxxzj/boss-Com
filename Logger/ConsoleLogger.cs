using System;

namespace OSExp.Logger
{
    public class ConsoleLogger : LoggerBase
    {
        public ConsoleLogger(Type type) : base(type)
        {
        }

        protected override void WriteLog(LogLevel level, string msg)
        {
            Console.WriteLine(Format(level, msg));
        }
    }
}
