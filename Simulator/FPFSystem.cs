using OSExp.Processes;

namespace OSExp.Simulator
{
    class FPFSystem : System
    {
        protected override int RunProcess(Process process)
        {
            var before = process.CpuState.TimeUse;
            Cpu.RunToEnd();
            var after = Cpu.State.TimeUse;

            ProcessList.ForEach(increasePriority);
            SuspendedList.ForEach(increasePriority);

            if (process.Priority != Priority.RealTime && process.Priority > Priority.Low)
            {
                process.Priority--;
            }

            return after - before;
        }

        private void increasePriority(Process p)
        {
            if (p.Priority < Priority.High && p.State != State.Terminated)
            {
                p.Priority++;
            }
        }
    }
}
