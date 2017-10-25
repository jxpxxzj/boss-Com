using System;

namespace OSExp.Logger
{
    public abstract class LoggerBase : ILogger
    {
        public Type Type { get; set; }
        public LoggerBase(Type type)
        {
            Type = type;
        }
        protected abstract void WriteLog(LogLevel level, string msg);

        protected void Log(LogLevel level, string msg)
        {
            if (LogManager.Level <= level)
            {
                WriteLog(level, msg);
            }
        }

        public virtual void Debug(string msg)
        {
            Log(LogLevel.Debug, msg);
        }
        public virtual void Error(string msg)
        {
            Log(LogLevel.Error, msg);
        }
        public virtual void Info(string msg)
        {
            Log(LogLevel.Info, msg);
        }
        public virtual void Warn(string msg)
        {
            Log(LogLevel.Warn, msg);
        }

        public virtual string Format(LogLevel level, string msg)
        {
            return $"[{level}] ({Type.FullName}) {DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()} - {msg}";
        }

    }
}
