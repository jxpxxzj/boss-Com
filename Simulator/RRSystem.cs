using OSExp.Processes;

namespace OSExp.Simulator
{
    class RRSystem : System
    {
        public static int TimeSliceUnit => 100;
        protected override int RunProcess(Process process)
        {
            var before = process.CpuState.TimeUse;
            var after = Cpu.State.TimeUse;

            while(after - before < TimeSliceUnit)
            {
                Cpu.RunStep();
                after = Cpu.State.TimeUse;
                if (Cpu.IsTerminated)
                {
                    break;
                }
            }

            if (Cpu.IsTerminated)
            {
                // process.RequestTime = 0;
                // return process.RequestTime;
            }
            else
            {
                // process.RequestTime -= TimeSliceUnit;
                // process.Priority--;
                // return TimeSliceUnit;
            }
            return after - before;
        }
    }
}
