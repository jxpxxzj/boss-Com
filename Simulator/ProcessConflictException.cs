using System;

namespace OSExp.Simulator
{
    [Serializable]
    public class ProcessConflictException : ApplicationException
    {
        public string Name { get; set; }
    }
}
