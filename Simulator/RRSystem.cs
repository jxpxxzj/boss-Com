using OSExp.Processes;
using System.Linq;

namespace OSExp.Simulator
{
    class RRSystem : System
    {
        public static int TimeSliceUnit => 100;
        public override void SortList()
        {
            var rList = ProcessList.Where(t => t.State != State.Terminated).ToList();
            var tList = ProcessList.Where(t => t.State == State.Terminated);
            rList.Sort((p1, p2) =>
            {
                return p1.LastRunTime > p2.LastRunTime ? 1 : -1;
            });
            ProcessList = rList.Concat(tList).ToList();
        }
        protected override int RunProcess(Process process)
        {
            var before = process.CpuState.TimeUse;
            var after = Cpu.State.TimeUse;

            while (after - before < TimeSliceUnit)
            {
                Cpu.RunStep();
                after = Cpu.State.TimeUse;
                if (Cpu.IsTerminated)
                {
                    break;
                }
            }
            return after - before;
        }
    }
}
