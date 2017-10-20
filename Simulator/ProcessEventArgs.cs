using OSExp.Processes;
using System;

namespace OSExp.Simulator
{
    public class ProcessEventArgs : EventArgs
    {
        public Process Process { get; set; }
    }
}
