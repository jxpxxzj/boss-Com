using System;

namespace OSExp.Simulator
{
    public class ProcessNotExistException : ApplicationException
    {
        public string Name { get; set; }
    }
}
