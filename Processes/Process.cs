using OSExp.ASM.Emulator;
using OSExp.ASM.Language;
using OSExp.Simulator;
using System.Linq;
using System.Collections.Generic;

namespace OSExp.Processes
{
    public class Process
    {
        public string Name { get; set; }
        public int RequestTime { get; set; }
        public Priority Priority { get; set; }
        public State State { get; set; }

        public int CreateTime { get; set; }

        public int LastRunTime { get; set; }

        public List<SyntaxNode> Program { get; set; }

        public CpuState CpuState { get; set; }

        public MemoryAllocation Memory { get; set; }

        public ProcessEventArgs ToEventArgs() => new ProcessEventArgs()
        {
            Process = this
        };

        public int MemorySize => Program.Select(t => t.Length).Aggregate(0, (sum, x) => sum + x);

        public override string ToString()
        {
            return $"Process: {Name} CommandCount: {Program.Count} Priority: {Priority} State: {State} CreateTime: {CreateTime} LastRunTime: {LastRunTime} CpuTime: {CpuState.TimeUse}";
        }
    }
}
