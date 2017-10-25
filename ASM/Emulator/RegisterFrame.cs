namespace OSExp.ASM.Emulator
{
    public struct RegisterFrame
    {
        public int ax;
        public int bx;
        public int cx;
        public int dx;

        public int si;
        public int di;
        public int bp;
        public int sp;

        public int ip;

        public int cs;
        public int ds;
        public int es;
        public int ss;

        public override string ToString()
        {
            return $"ax={ax} bx={bx} cx={cx} dx={dx} ip={ip}";
        }
    }
}
