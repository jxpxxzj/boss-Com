using OSExp.Processes;

namespace OSExp.Simulator
{
    class FPFSystem : System
    {
        protected override int RunProcess(Process process)
        {
            var before = process.CpuState.TimeUse;
            Cpu.RunStep();
            var after = Cpu.State.TimeUse;
            if (process.Priority != Priority.RealTime && process.Priority > Priority.Low)
            {
                process.Priority--;
            }
            return after - before;
        }
    }
}
