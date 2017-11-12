using System;

namespace OSExp.Simulator
{
    [Serializable]
    public class ProcessNotExistException : ApplicationException
    {
        public string Name { get; set; }
    }
}
