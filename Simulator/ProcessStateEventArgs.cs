using OSExp.Processes;
using System;

namespace OSExp.Simulator
{
    public class ProcessStateEventArgs : EventArgs
    {
        public State Before { get; set; }
        public Process Process { get; set; }
    }
}
