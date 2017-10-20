using System;
using OSExp.Processes;

namespace OSExp.Simulator
{
    class FPFSystem : System
    {
        protected override int RunProcess(Process process)
        {
            process.RequestTime--;
            if (process.Priority != Priority.RealTime && process.Priority > Priority.Low)
            {
                process.Priority--;
            }
            return 1;
        }
    }
}
