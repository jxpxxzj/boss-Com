using OSExp.ASM.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OSExp.ASM.Emulator
{
    public class Cpu
    {
        private RegisterFrame registerFrame = new RegisterFrame();
        public RegisterFrame RegisterFrame { get => registerFrame; protected set => registerFrame = value; }

        private FlagRegisterFrame flagRegisterFrame = new FlagRegisterFrame();
        public FlagRegisterFrame FlagRegisterFrame { get => flagRegisterFrame; protected set => flagRegisterFrame = value; }

        public int TimeUse { get; protected set; } = 0;

        public Stack<object> Stack { get; protected set; } = new Stack<object>();

        public Stack<int> ProcStackTrace { get; protected set; } = new Stack<int>();

        public List<SyntaxNode> Program { get; protected set; }

        public event EventHandler<InterruptEventArgs> Interrupted;
        

        public int[] Memory { get; protected set; } = new int[1048576];

        public SyntaxNode CurrentLine => Program[registerFrame.ip];

        public Cpu()
        {

        }

        public Cpu(List<SyntaxNode> program, CpuState state = new CpuState())
        {
            LoadProgram(program, state);
        }

        public CpuState State
        {
            get => new CpuState(this);
            set {
                registerFrame = value.RegisterFrame;
                flagRegisterFrame = value.FlagRegisterFrame;
                TimeUse = value.TimeUse;
                Memory = value.Memory;
                Stack = value.Stack;
                ProcStackTrace = value.ProcStackTrace;
            }
        }

        protected virtual void OnInterrupt(int code)
        {
            Interrupted?.Invoke(this, new InterruptEventArgs()
            {
                State = State,
                Code = code
            });
        }

        public void LoadProgram(List<SyntaxNode> program)
        {
            LoadProgram(program, new CpuState(1048576));
        }

        public void LoadProgram(List<SyntaxNode> program, CpuState state)
        {
            Program = program;
            State = state;
        }
        public void RunStep()
        {
            if (RegisterFrame.ip < Program.Count)
            {
                runLine(CurrentLine);
                registerFrame.ip++;
            }
        }

        public void RunToEnd()
        {
            while(RegisterFrame.ip < Program.Count)
            {
                RunStep();
            }
        }

        private void runLine(SyntaxNode code)
        {
            if (code.Type == NodeType.Operation)
            {
                var ops = (Ops)code.Value;
                var op1 = code.Children[0];
                var op2 = code.Children.Count == 2 ? code.Children[1]: null;
                switch(ops)
                {
                    case Ops.Mov:
                        if (op1.Type == NodeType.Register && op2.Type == NodeType.Register)
                        {
                            Mov((Register)op1.Value, (Register)op2.Value);
                        }
                        if (op1.Type == NodeType.Register && op2.Type == NodeType.MemorySeek)
                        {
                            Mov((Register)op1.Value, (MemorySeek)op2.Value);
                        }
                        if (op1.Type == NodeType.MemorySeek && op2.Type == NodeType.Register)
                        {
                            Mov((MemorySeek)op1.Value, (Register)op2.Value);                         
                        }
                        if (op1.Type == NodeType.Register && op2.Type == NodeType.Number)
                        {
                            Mov((Register)op1.Value, (int)op2.Value);
                        }
                        if (op1.Type == NodeType.MemorySeek && op2.Type == NodeType.Number)
                        {
                            Mov((MemorySeek)op1.Value, (int)op2.Value);
                        }
                        break;
                    case Ops.Xchg:
                        Xchg((Register)op1.Value, (Register)op2.Value);
                        break;

                    case Ops.Add:
                        if (op2.Type == NodeType.Register)
                        {
                            Add((Register)op1.Value, (Register)op2.Value);
                        }
                        if (op2.Type == NodeType.Number)
                        {
                            Add((Register)op1.Value, (int)op2.Value);
                        }
                        break;
                    case Ops.Mul:
                        if (op1.Type == NodeType.Register)
                        {
                            Mul((Register)op1.Value);
                        }
                        if (op1.Type == NodeType.Number)
                        {
                            Mul((int)op1.Value);
                        }
                        if (op1.Type == NodeType.MemorySeek)
                        {
                            Mul((MemorySeek)op1.Value);
                        }
                        break;
                    case Ops.Sub:
                        if (op2.Type == NodeType.Register)
                        {
                            Sub((Register)op1.Value, (Register)op2.Value);
                        }
                        if (op2.Type == NodeType.Number)
                        {
                            Sub((Register)op1.Value, (int)op2.Value);
                        }
                        break;
                    case Ops.Div:
                        if (op1.Type == NodeType.Register)
                        {
                            Div((Register)op1.Value);
                        }
                        if (op1.Type == NodeType.Number)
                        {
                            Div((int)op1.Value);
                        }
                        if (op1.Type == NodeType.MemorySeek)
                        {
                            Div((MemorySeek)op1.Value);
                        }
                        break;
                    case Ops.Inc:
                        Inc((Register)op1.Value);
                        break;
                    case Ops.Dec:
                        Dec((Register)op1.Value);
                        break;

                    case Ops.And:
                        if (op1.Type == NodeType.Register && op2.Type == NodeType.Register)
                        {
                            And((Register)op1.Value, (Register)op2.Value);
                        }
                        if (op1.Type == NodeType.Register && op2.Type == NodeType.MemorySeek)
                        {
                            And((Register)op1.Value, (MemorySeek)op2.Value);
                        }
                        if (op1.Type == NodeType.MemorySeek && op2.Type == NodeType.Register)
                        {
                            And((MemorySeek)op1.Value, (Register)op2.Value);
                        }
                        if (op1.Type == NodeType.Register && op2.Type == NodeType.Number)
                        {
                            And((Register)op1.Value, (int)op2.Value);
                        }
                        if (op1.Type == NodeType.MemorySeek && op2.Type == NodeType.Number)
                        {
                            And((MemorySeek)op1.Value, (int)op2.Value);
                        }
                        break;
                    case Ops.Or:
                        if (op1.Type == NodeType.Register && op2.Type == NodeType.Register)
                        {
                            Or((Register)op1.Value, (Register)op2.Value);
                        }
                        if (op1.Type == NodeType.Register && op2.Type == NodeType.MemorySeek)
                        {
                            Or((Register)op1.Value, (MemorySeek)op2.Value);
                        }
                        if (op1.Type == NodeType.MemorySeek && op2.Type == NodeType.Register)
                        {
                            Or((MemorySeek)op1.Value, (Register)op2.Value);
                        }
                        if (op1.Type == NodeType.Register && op2.Type == NodeType.Number)
                        {
                            Or((Register)op1.Value, (int)op2.Value);
                        }
                        if (op1.Type == NodeType.MemorySeek && op2.Type == NodeType.Number)
                        {
                            Or((MemorySeek)op1.Value, (int)op2.Value);
                        }
                        break;
                    case Ops.Not:
                        if (op1.Type == NodeType.Register)
                        {
                            Not((Register)op1.Value);
                        }
                        if (op1.Type == NodeType.MemorySeek)
                        {
                            Not((MemorySeek)op1.Value);
                        }
                        break;
                    case Ops.Xor:
                        if (op1.Type == NodeType.Register && op2.Type == NodeType.Register)
                        {
                            Xor((Register)op1.Value, (Register)op2.Value);
                        }
                        if (op1.Type == NodeType.Register && op2.Type == NodeType.MemorySeek)
                        {
                            Xor((Register)op1.Value, (MemorySeek)op2.Value);
                        }
                        if (op1.Type == NodeType.MemorySeek && op2.Type == NodeType.Register)
                        {
                            Xor((MemorySeek)op1.Value, (Register)op2.Value);
                        }
                        if (op1.Type == NodeType.Register && op2.Type == NodeType.Number)
                        {
                            Xor((Register)op1.Value, (int)op2.Value);
                        }
                        if (op1.Type == NodeType.MemorySeek && op2.Type == NodeType.Number)
                        {
                            Xor((MemorySeek)op1.Value, (int)op2.Value);
                        }
                        break;

                    case Ops.Shl:
                        break;
                    case Ops.Shr:
                        break;
                    case Ops.Rol:
                        break;
                    case Ops.Ror:
                        break;

                    case Ops.Push:
                        if (op1.Type == NodeType.Register)
                        {
                            Push((Register)op1.Value);
                        }
                        else
                        {
                            Push(op1.Value);
                        } 
                        break;
                    case Ops.Pop:
                        Pop((Register)op1.Value);
                        break;

                    case Ops.Call:
                        Call((string)op1.Value);
                        break;
                    case Ops.Ret:
                        Ret();
                        break;
                    case Ops.Loop:
                        Loop((string)op1.Value);
                        break;
                    case Ops.Int:
                        Int((int)op1.Value);
                        break;

                    case Ops.Jmp:
                        Jmp((string)op1.Value);
                        break;
                    case Ops.Je:
                        Je((string)op1.Value);
                        break;
                    case Ops.Jne:
                        Jne((string)op1.Value);
                        break;
                    case Ops.Jb:
                        Jb((string)op1.Value);
                        break;
                    case Ops.Jnb:
                        Jnb((string)op1.Value);
                        break;
                    case Ops.Ja:
                        Ja((string)op1.Value);
                        break;
                    case Ops.Jna:
                        Jna((string)op1.Value);
                        break;

                    case Ops.Cmp:
                        if (op1.Type == NodeType.Register && op2.Type == NodeType.Register)
                        {
                            Cmp((Register)op1.Value, (Register)op2.Value);
                        }
                        if (op1.Type == NodeType.Register && op2.Type == NodeType.Number)
                        {
                            Cmp((Register)op1.Value, (int)op2.Value);
                        }
                        if (op1.Type == NodeType.Number && op2.Type == NodeType.Register)
                        {
                            Cmp((int)op1.Value, (Register)op2.Value);
                        }
                        break;
                    case Ops.Nop:
                        Nop();
                        break;
                   
                }
            }
        }

        public void Nop()
        {
            TimeUse += 3;
        }

        public void Mov(Register target, Register source)
        {
            var val = getRegister(source);
            setRegister(target, val);
            TimeUse += 2;
        }

        public void Mov(Register target, MemorySeek source)
        {
            var val = getMemory(source);
            setRegister(target, val);
            TimeUse += 8;
        }

        public void Mov(MemorySeek target, Register source)
        {
            var val = getRegister(source);
            setMemory(target, val);
            TimeUse += 9;
        }

        public void Mov(Register target, int value)
        {
            setRegister(target, value);
            TimeUse += 4;
        }

        public void Mov(MemorySeek target, int value)
        {
            setMemory(target, value);
            TimeUse += 10;
        }

        public void Xchg(Register r1, Register r2)
        {
            var tmp = getRegister(r1);
            Mov(r1, r2);
            Mov(r2, tmp);
        }

        public void Add(Register target, int value)
        {
            var o = getRegister(target);
            setRegister(target, o + value);
            TimeUse += 4;
        }

        public void Add(Register target, Register source)
        {
            var o = getRegister(target);
            var s = getRegister(source);
            setRegister(target, o + s);
            TimeUse += 3;
        }

        public void Sub(Register target, int value)
        {
            var o = getRegister(target);
            setRegister(target, o - value);
            TimeUse += 4;
        }

        public void Sub(Register target, Register source)
        {
            var o = getRegister(target);
            var s = getRegister(source);
            setRegister(target, o - s);
            TimeUse += 3;
        }

        public void Mul(Register source)
        {
            var o = getRegister(Register.ax);
            var s = getRegister(source);
            setRegister(Register.ax, o * s);
            TimeUse += 133;
        }

        public void Mul(int value)
        {
            var o = getRegister(Register.ax);
            setRegister(Register.ax, o * value);
            TimeUse += 133;
        }

        public void Mul(MemorySeek memory)
        {
            var o = getRegister(Register.ax);
            var m = getMemory(memory);
            setRegister(Register.ax, o * m);
            TimeUse += 139;
        }

        public void Div(Register source)
        {
            var o = getRegister(Register.ax);
            var s = getRegister(source);
            setRegister(Register.ax, o / s);
            setRegister(Register.dx, o % s);
            TimeUse += 162;
        }
    
        public void Div(int value)
        {
            var o = getRegister(Register.ax);
            setRegister(Register.ax, o / value);
            setRegister(Register.dx, o % value);
            TimeUse += 162;
        }

        public void Div(MemorySeek memory)
        {
            var o = getRegister(Register.ax);
            var m = getMemory(memory);
            setRegister(Register.ax, o / m);
            setRegister(Register.dx, o % m);
            TimeUse += 168;
        }

        public void And(Register target, Register source)
        {
            var t = getRegister(target);
            var s = getRegister(source);
            setRegister(target, t & s);
            TimeUse += 3;
        }

        public void And(MemorySeek target, Register source)
        {
            var t = getMemory(target);
            var s = getRegister(source);
            setMemory(target, t & s);
            TimeUse += 16;
        }
        public void And(Register target, MemorySeek source)
        {
            var t = getRegister(target);
            var s = getMemory(source);
            setRegister(target, t & s);
            TimeUse += 9;
        }

        public void And(Register target, int number)
        {
            var t = getRegister(target);
            setRegister(target, t & number);
            TimeUse += 4;
        }

        public void And(MemorySeek target, int number)
        {
            var t = getMemory(target);
            setMemory(target, t & number);
            TimeUse += 17;
        }

        public void Or(Register target, Register source)
        {
            var t = getRegister(target);
            var s = getRegister(source);
            setRegister(target, t | s);
            TimeUse += 3;
        }

        public void Or(MemorySeek target, Register source)
        {
            var t = getMemory(target);
            var s = getRegister(source);
            setMemory(target, t | s);
            TimeUse += 16;
        }
        public void Or(Register target, MemorySeek source)
        {
            var t = getRegister(target);
            var s = getMemory(source);
            setRegister(target, t | s);
            TimeUse += 9;
        }

        public void Or(Register target, int number)
        {
            var t = getRegister(target);
            setRegister(target, t | number);
            TimeUse += 4;
        }

        public void Or(MemorySeek target, int number)
        {
            var t = getMemory(target);
            setMemory(target, t | number);
            TimeUse += 17;
        }

        public void Not(Register target)
        {
            var t = getRegister(target);
            setRegister(target, ~t);
        }

        public void Not(MemorySeek target)
        {
            var t = getMemory(target);
            setMemory(target, ~t);
        }

        public void Xor(Register target, Register source)
        {
            var t = getRegister(target);
            var s = getRegister(source);
            setRegister(target, t ^ s);
            TimeUse += 3;
        }

        public void Xor(MemorySeek target, Register source)
        {
            var t = getMemory(target);
            var s = getRegister(source);
            setMemory(target, t ^ s);
            TimeUse += 16;
        }
        public void Xor(Register target, MemorySeek source)
        {
            var t = getRegister(target);
            var s = getMemory(source);
            setRegister(target, t ^ s);
            TimeUse += 9;
        }

        public void Xor(Register target, int number)
        {
            var t = getRegister(target);
            setRegister(target, t ^ number);
            TimeUse += 4;
        }

        public void Xor(MemorySeek target, int number)
        {
            var t = getMemory(target);
            setMemory(target, t ^ number);
            TimeUse += 17;
        }

        public void Call(string name)
        {
            if(name.Contains(".")) // invoke clr methods
            {
                var result = invokeCLRMethod(name);
                if (result != null)
                {
                    Push(result);
                }
            }
            else // call proc
            {
                ProcStackTrace.Push(RegisterFrame.ip);
                var line = Program.FindIndex(t => t.Label == name);
                registerFrame.ip = line - 1;
            }
            TimeUse += 37;
        }

        public void Ret()
        {
            if (ProcStackTrace.Count == 0) // terminate program
            {
                registerFrame.ip = Program.Count - 1;
            }
            else // return from proc
            {
                var line = ProcStackTrace.Pop();
                registerFrame.ip = line - 1;
            }
        }

        public void Int(int num)
        {
            OnInterrupt(num);
            TimeUse += 52;
        }

        public void Inc(Register register)
        {
            Add(register, 1);
            TimeUse -= 1;
        }
        public void Dec(Register register)
        {
            Sub(register, 1);
            TimeUse -= 1;
        }

        public void Jmp(string label)
        {
            var line = Program.FindIndex(t => t.Label == label);
            registerFrame.ip = line-1;
            TimeUse += 15;
        }

        public void Je(string label)
        {
            if (FlagRegisterFrame.Zero) Jmp(label);
            TimeUse += 1;
        }

        public void Jne(string label)
        {
            if (!FlagRegisterFrame.Zero) Jmp(label);
            TimeUse += 1;
        }

        public void Jb(string label)
        {
            if (FlagRegisterFrame.Carry) Jmp(label);
            TimeUse += 1;
        }

        public void Jnb(string label)
        {
            if (!FlagRegisterFrame.Carry) Jmp(label);
            TimeUse += 1;
        }

        public void Ja(string label)
        {
            if (!FlagRegisterFrame.Carry && !FlagRegisterFrame.Zero) Jmp(label);
            TimeUse += 1;
        }

        public void Jna(string label)
        {
            if (FlagRegisterFrame.Carry || !FlagRegisterFrame.Zero) Jmp(label);
            TimeUse += 1;
        }

        public void Cmp(Register r1, Register r2)
        {
            var v1 = getRegister(r1);
            var v2 = getRegister(r2);

            setCmpFlag(v1, v2);
            TimeUse += 3;
        }

        public void Cmp(Register r1, int value)
        {
            var v1 = getRegister(r1);

            setCmpFlag(v1, value);
            TimeUse += 4;
        }

        public void Cmp(int value, Register r2)
        {
            var v2 = getRegister(r2);
            setCmpFlag(value, v2);
            TimeUse += 4;
        }
        public void Push<T>(T value)
        {
            Stack.Push(value);
            TimeUse += 16;
        }

        public void Push(Register register)
        {
            Stack.Push(getRegister(register));
            TimeUse += 11;
        }

        public void Loop(string label)
        {
            if (RegisterFrame.cx > 0)
            {
                registerFrame.cx--;
                Jmp(label);
            }
            TimeUse += 2;
        }
        
        public void Pop(Register register)
        {
            setRegister(register, pop<int>());
            TimeUse += 17;
        }

        private T pop<T>()
        {
            if (Stack.Count > 0)
                return (T)Stack.Pop();
            else
                throw new IndexOutOfRangeException();
        }

        private int getRegister(Register r)
        {
            var field = typeof(RegisterFrame).GetField(r.ToString());
            return (int)field.GetValue(RegisterFrame);
        }

        private void setRegister(Register r, int value)
        {
            var field = typeof(RegisterFrame).GetField(r.ToString());
            var box = (object)RegisterFrame;
            field.SetValue(box, value);
            RegisterFrame = (RegisterFrame)box;
        }

        private int getMemory(MemorySeek source)
        {
            var reg = getRegister(source.Register);
            var pos = RegisterFrame.ds * 16 + reg + source.Offset;

            return Memory[pos];
        }

        private void setMemory(MemorySeek source, int value)
        {
            var reg = getRegister(source.Register);
            var pos = RegisterFrame.ds * 16 + reg + source.Offset;

            Memory[pos] = value;
        }

        private void setCmpFlag(int v1, int v2)
        {
            if (v1 == v2)
            {
                flagRegisterFrame.Zero = true;
            }
            else
            {
                flagRegisterFrame.Zero = false;
            }

            if (v1 < v2)
            {
                flagRegisterFrame.Carry = true;
            }
            else
            {
                flagRegisterFrame.Carry = false;
            }
        }

        private object invokeCLRMethod(string methodFullName)
        {
            var className = methodFullName;
            if (methodFullName.Contains("("))
            {
                className = methodFullName.Replace(Parser.between(methodFullName, "(", ")"), string.Empty).Replace("()", string.Empty);
            }
            var sp = className.Split('.').ToList();
            var metName = sp.Last();
            sp.Remove(sp.Last());
            var asm = Assembly.GetAssembly(typeof(Console));
            var typeList = new List<Type>();
            if (methodFullName.Contains("("))
            {
                var mtdTypeInfo = Parser.between(methodFullName, "(", ")");
                var typeSplit = mtdTypeInfo.Split(',');

                foreach (var t in typeSplit)
                {
                    typeList.Add(asm.GetType(t.Trim()));
                }
            }
            
            var clsName = string.Join(".", sp.ToArray());  
            var cls = asm.GetType(clsName.Split('(')[0]);
            if (cls != null)
            {
                var method = cls.GetMethod(metName, typeList.ToArray());
                if (method != null)
                {
                    var parmList = new List<object>();
                    var parmInfo = method.GetParameters();
                    if (parmInfo.Length > 0)
                    {
                        for (var i = 0; i < parmInfo.Length; i++)
                        {
                            parmList.Add(pop<object>());
                        }
                    }

                    var result = method.Invoke(null, parmList.ToArray());
                    return result;
                }
            }
            return null;
        }
    }
}