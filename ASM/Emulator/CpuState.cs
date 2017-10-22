using System.Collections.Generic;

namespace OSExp.ASM.Emulator
{
    public struct CpuState
    {
        public RegisterFrame RegisterFrame;
        public FlagRegisterFrame FlagRegisterFrame;
        public int[] Memory;
        public Stack<object> Stack;
        public Stack<int> ProcStackTrace;
        public int TimeUse;

        public CpuState(int memorySize = 1048576)
        {
            RegisterFrame = new RegisterFrame();
            FlagRegisterFrame = new FlagRegisterFrame();
            Memory = new int[memorySize];
            Stack = new Stack<object>();
            ProcStackTrace = new Stack<int>();
            TimeUse = 0;
        }

        public CpuState(Cpu cpu)
        {
            RegisterFrame = cpu.RegisterFrame;
            FlagRegisterFrame = cpu.FlagRegisterFrame;
            TimeUse = cpu.TimeUse;
            Stack = cpu.Stack;
            Memory = cpu.Memory;
            ProcStackTrace = cpu.ProcStackTrace;
        }
    }
}
