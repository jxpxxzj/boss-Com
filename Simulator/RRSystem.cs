using OSExp.Processes;

namespace OSExp.Simulator
{
    class RRSystem : System
    {
        public static int TimeSliceUnit => 5;
        protected override int RunProcess(Process process)
        {
            if (process.RequestTime <= TimeSliceUnit)
            {
                process.RequestTime = 0;
                return process.RequestTime;
            }
            else
            {
                process.RequestTime -= TimeSliceUnit;
                process.Priority--;
                return TimeSliceUnit;
            }
        }
    }
}
