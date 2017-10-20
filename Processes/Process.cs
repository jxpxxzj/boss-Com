using OSExp.Simulator;

namespace OSExp.Processes
{
    public class Process
    {
        public string Name { get; set; }
        public int RequestTime { get; set; }
        public Priority Priority { get; set; }
        public State State { get; set; }

        public int CreateTime { get; set; }
        public int CpuTime { get; set; }

        public ProcessEventArgs ToEventArgs() => new ProcessEventArgs()
        {
            Process = this
        };

        public override string ToString()
        {
            return $"Process: {Name} RequestTime: {RequestTime} Priority: {Priority} State: {State} CreateTime: {CreateTime} CpuTime: {CpuTime}";
        }
    }
}
