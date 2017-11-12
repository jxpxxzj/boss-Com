using System;

namespace OSExp.Simulator
{
    public class MemoryAllocation
    {
        public int Begin { get; protected set; }
        public int End { get; protected set; }
        public MemoryAllocationType Type { get; protected set; }

        public MemoryAllocation(int begin, int end, MemoryAllocationType type)
        {
            Begin = begin;
            End = end;
            Type = type;
        }

        public int Length => End - Begin + 1;

        public void Move(int offset, int maxSize = -1)
        {
            if (maxSize == -1) // ignore overflow
            {
                Begin += offset;
                End += offset;
            }
            else
            {
                if (Begin + offset >= 0 && End + offset < maxSize)
                {
                    Begin += offset;
                    End += offset;
                }
                else
                {
                    throw new OutOfMemoryException();
                }
            }
        }

        public override string ToString() =>
            $"{Length} B @ [{Begin}, {End}]";
    }
}
