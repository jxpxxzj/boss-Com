using System;

namespace OSExp.Simulator
{
    public class ProcessConflictException : ApplicationException
    {
        public string Name { get; set; }
    }
}
