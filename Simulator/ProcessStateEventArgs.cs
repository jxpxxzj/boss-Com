using OSExp.Processes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSExp.Simulator
{
    public class ProcessStateEventArgs : EventArgs
    {
        public State Before { get; set; }
        public Process Process { get; set; }
    }
}
