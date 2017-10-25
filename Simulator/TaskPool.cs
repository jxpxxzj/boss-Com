using OSExp.ASM.Language;
using System;
using System.Collections.Generic;

namespace OSExp.Simulator
{
    public static class TaskPool
    {
        public static int TotalTaskCount { get; private set; } = 0;
        public static (string, List<SyntaxNode>) GenerateTask()
        {
            var rand = new Random(DateTime.Now.Millisecond).Next(50)+50;
            var prog = new List<SyntaxNode>();
            for(var i=0;i<rand;i++)
            {
                prog.Add(new SyntaxNode()
                {
                    Type = NodeType.Operation,
                    Value = Ops.Nop
                });
            }
            
            return ($"Process {++TotalTaskCount}", prog);
        }
    }
}
