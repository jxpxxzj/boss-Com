using System.Collections.Generic;

namespace OSExp.ASM.Emulator
{
    public struct CpuState
    {
        public RegisterFrame RegisterFrame;
        public FlagRegisterFrame FlagRegisterFrame;
        public int[] Memory;
        public Stack<object> Stack;
        public int TimeUse;

        public CpuState(int memorySize = 1048576)
        {
            RegisterFrame = new RegisterFrame();
            FlagRegisterFrame = new FlagRegisterFrame();
            Memory = new int[memorySize];
            Stack = new Stack<object>();
            TimeUse = 0;
        }

        public CpuState(Cpu cpu)
        {
            RegisterFrame = cpu.RegisterFrame;
            FlagRegisterFrame = cpu.FlagRegisterFrame;
            TimeUse = cpu.TimeUse;
            Stack = cpu.Stack;
            Memory = cpu.Memory;
        }
    }
}
