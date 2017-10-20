using System;

namespace OSExp.Simulator
{
    public static class TaskPool
    {
        public static int TotalTaskCount { get; private set; } = 0;
        public static (string, int) GenerateTask()
        {
            var rand = new Random(DateTime.Now.Millisecond);
            return ($"Process {++TotalTaskCount}", rand.Next(5)+5);
        }
    }
}
