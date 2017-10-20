using System;

namespace OSExp.ASM.Emulator
{
    public class InterruptEventArgs : EventArgs
    {
        public CpuState State { get; set; }
        public int Code { get; set; }
    }
}