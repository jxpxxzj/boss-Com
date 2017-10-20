namespace OSExp.ASM.Emulator
{
    public struct FlagRegisterFrame
    {
        public bool Carry;
        public bool Parity;
        public bool AuxiliaryCarry;
        public bool Zero;
        public bool Sign;
        public bool Overflow;

        public bool Trap;
        public bool InterruptEnable;
        public bool Direction;

        public override string ToString()
        {
            return $"CF={Carry} ZF={Zero}";
        }
    }
}
