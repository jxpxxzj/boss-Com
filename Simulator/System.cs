using OSExp.ASM.Emulator;
using OSExp.ASM.Language;
using OSExp.Processes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OSExp.Simulator
{
    public abstract class System
    {
        public List<Process> ProcessList { get; protected set; } = new List<Process>();
        public List<Process> SuspendedList { get; protected set; } = new List<Process>();

        public int Time { get; protected set; } = 0;

        public virtual int ChannelCount { get; set; } = 16;
        public int MaxMemory { get; set; } = 2048;

        public int ProcessCount => ProcessList.Count + SuspendedList.Count;
        public List<Process> GetAllProcess() => new List<Process>(ProcessList.Concat(SuspendedList));

        public event EventHandler<ProcessStateEventArgs> ProcessStateChanged;
        public event EventHandler<ProcessEventArgs> ProcessCreated;
        public event EventHandler<ProcessEventArgs> ProcessScheduled;
        public event EventHandler<ProcessEventArgs> ProcessRunned;
        public event EventHandler<ProcessEventArgs> ProcessTerminated;
        public event EventHandler<ProcessEventArgs> ProcessSuspended;
        public event EventHandler<ProcessEventArgs> ProcessUnsuspended;
        public event EventHandler<ProcessEventArgs> ProcessKilled;

        public event EventHandler<InterruptEventArgs> Interrupted;

        public bool CanRun => ProcessList.Where(p => p.State == State.Ready || p.State == State.Running || p.State == State.Created).FirstOrDefault() != null;

        public System()
        {
            Cpu.Interrupted += Cpu_Interrupted;
        }

        private void Cpu_Interrupted(object sender, InterruptEventArgs e)
        {
            Interrupted?.Invoke(sender, e);
        }

        public Cpu Cpu { get; protected set; } = new Cpu();

        protected void OnProcessStateChange(Process process, State before)
        {
            if (process.State == before)
                return;
            ProcessStateChanged?.Invoke(this, new ProcessStateEventArgs()
            {
                Before = before,
                Process = process
            });
            switch (process.State)
            {
                case State.Created:
                    OnProcessCreate(process);
                    break;
                case State.Ready:
                    OnProcessSchedule(process);
                    break;
                case State.Waiting:
                    break;
                case State.Running:
                    OnProcessRun(process);
                    break;
                case State.Terminated:
                    OnProcessTerminate(process);
                    break;
                case State.Suspended:
                    OnProcessSuspend(process);
                    break;
            }
            if (before == State.Suspended)
            {
                OnProcessUnsuspend(process);
            }
        }

        protected void OnProcessCreate(Process process)
        {
            ProcessCreated?.Invoke(this, process.ToEventArgs());
        }

        protected void OnProcessSchedule(Process process)
        {
            ProcessScheduled?.Invoke(this, process.ToEventArgs());
        }

        protected void OnProcessRun(Process process)
        {
            ProcessRunned?.Invoke(this, process.ToEventArgs());
        }
        protected void OnProcessTerminate(Process process)
        {
            ProcessTerminated?.Invoke(this, process.ToEventArgs());
        }
        protected void OnProcessSuspend(Process process)
        {
            ProcessSuspended?.Invoke(this, process.ToEventArgs());
        }
        protected void OnProcessUnsuspend(Process process)
        {
            ProcessUnsuspended?.Invoke(this, process.ToEventArgs());
        }
        protected void OnProcessKilled(Process process)
        {
            ProcessKilled?.Invoke(this, process.ToEventArgs());
        }


        public virtual void SortList()
        {
            var rList = ProcessList.Where(t => t.State != State.Terminated).ToList();
            var tList = ProcessList.Where(t => t.State == State.Terminated);
            rList.Sort((p1, p2) =>
            {
                if (p1.Priority == p2.Priority)
                    return p1.LastRunTime > p2.LastRunTime ? 1 : -1;
                return p1.Priority < p2.Priority ? 1 : -1;
            });
            ProcessList = rList.Concat(tList).ToList();
        }

        public virtual Process GetRunnableProcess()
        {
            var result = ProcessList.Where(p => p.State == State.Ready).FirstOrDefault();
            if (ProcessCount < ChannelCount || result == null)
            {
                ScheduleProcess();
            }

            result = ProcessList.Where(p => p.State == State.Ready).FirstOrDefault();
            return result;
        }

        protected abstract int RunProcess(Process process);

        public IEnumerable<int> Run()
        {
            SortList();
            var process = GetRunnableProcess();

            if (process == null)
            {
                yield break;
            }
            // recover 
            Cpu.LoadProgram(process.Program, process.CpuState);

            var stateBefore = process.State;
            process.State = State.Running;

            var timeCost = RunProcess(process);

            yield return 0;

            process.LastRunTime = Time;
            Time += timeCost;
            process.State = State.Ready;
            if (Cpu.IsTerminated)
            {
                process.State = State.Terminated;
                OnProcessStateChange(process, stateBefore);
            }
            OnProcessRun(process);

            // save state
            process.CpuState = Cpu.State;
            SortList();

            yield return 1;
        }

        public void SuspendProcess(string processName)
        {
            var find = ProcessList.Find(t => t.Name == processName);
            var beforeState = find.State;
            if (find != null)
            {
                ProcessList.Remove(find);
                find.State = State.Suspended;
                SuspendedList.Add(find);
                OnProcessStateChange(find, beforeState);
            }
            else
            {
                throw new ProcessNotExistException()
                {
                    Name = processName
                };
            }
        }
        public void ResumeProcess(string processName)
        {
            var find = SuspendedList.Find(t => t.Name == processName);
            if (find != null)
            {
                SuspendedList.Remove(find);
                find.State = State.Ready;
                ProcessList.Add(find);
                OnProcessStateChange(find, State.Suspended);
            }
            else
            {
                throw new ProcessNotExistException()
                {
                    Name = processName
                };
            }
        }

        public void ScheduleProcess(string processName)
        {
            if (ProcessCount == ChannelCount)
            {
                ProcessList.FindAll(t => t.State == State.Terminated).ForEach(t => KillProcess(t.Name));
            }

            var find = ProcessList.Find(t => t.State == State.Created || t.Name == processName);
            if (find != null)
            {
                find.State = State.Ready;
                OnProcessStateChange(find, State.Created);
            }
            else
            {
                throw new ProcessNotExistException()
                {
                    Name = processName
                };
            }
        }

        public void ScheduleProcess()
        {
            var processList = ProcessList.FindAll(t => t.State == State.Created);
            foreach (var p in processList)
            {
                ScheduleProcess(p.Name);
            }
        }

        public void CreateProcess(string processName, List<SyntaxNode> program, Priority priority = Priority.Normal)
        {
            CheckChannel();
            var findInProcess = ProcessList.Find(t => t.Name == processName);
            var findInSuspend = SuspendedList.Find(t => t.Name == processName);
            if (findInProcess == null && findInSuspend == null)
            {
                var process = new Process()
                {
                    Name = processName,
                    Program = program,
                    Priority = priority,
                    State = State.Created,
                    CpuState = new CpuState(1048576),
                    CreateTime = Time
                };
                ProcessList.Add(process);
                SortList();
                AllocateMemory(process.Name);
                OnProcessCreate(process);
            }
            else
            {
                throw new ProcessConflictException()
                {
                    Name = processName
                };
            }
        }

        public void CheckChannel()
        {
            if (ProcessCount == ChannelCount)
            {
                KillTerminated();
            }
            if (ProcessCount == ChannelCount)
            {
                throw new TooManyProcessesException();
            }
        }

        public void KillTerminated()
        {
            ProcessList.FindAll(t => t.State == State.Terminated).ForEach((t) =>
            {
                KillProcess(t.Name);
            });
        }

        public void KillProcess(string processName)
        {
            var find = ProcessList.Find(t => t.Name == processName);
            if (find == null)
            {
                find = SuspendedList.Find(t => t.Name == processName);
                if (find == null)
                {
                    throw new ProcessNotExistException()
                    {
                        Name = processName
                    };
                }
                else
                {
                    SuspendedList.Remove(find);
                    OnProcessKilled(find);
                }

            }
            else
            {
                ProcessList.Remove(find);
                OnProcessKilled(find);
            }
        }



        public void AllocateMemory(string processName)
        {
            var process = ProcessList.Find(t => t.Name == processName);
            if (process == null) process = SuspendedList.Find(t => t.Name == processName);
            if (process == null) throw new ProcessNotExistException();

            var processSize = process.MemorySize;
            var hole = FindHole(processSize);

            process.Memory = new MemoryAllocation(hole.Item1, hole.Item2, MemoryAllocationType.Process);
        }

        public void CompressMemory()
        {
            var posPointer = 0;
            var processAllocated = GetAllProcess().Where(t => t.Memory != null).Select(t => t.Memory).ToList().OrderBy(t => t.Begin);
            foreach (var p in processAllocated)
            {
                if (p.Begin > posPointer) // should move
                {
                    p.Move(posPointer - p.Begin, MaxMemory);
                }
                if (p.Begin == posPointer)
                {
                    posPointer = p.End + 1;
                }
            }
        }

        protected virtual (int, int) FindHole(int expectedSize)
        {
            var posPointer = 0;
            var processAllocated = GetAllProcess().Where(t => t.Memory != null).Select(t => t.Memory).ToList().OrderBy(t => t.Begin);
            foreach (var p in processAllocated)
            {
                if (posPointer < p.Begin) // there is empty space
                {
                    var size = p.Begin - posPointer;
                    if (size >= expectedSize) // can allocate
                    {
                        return (posPointer, posPointer + expectedSize - 1);
                    }
                    else
                    {
                        posPointer = p.End;
                    }
                }

                if (posPointer == p.Begin) // such as 0 -> 0
                {
                    posPointer += p.Length;
                }
            }

            // all process are travelled
            if (posPointer < MaxMemory && posPointer + expectedSize < MaxMemory) // allocate at the end
            {
                return (posPointer, posPointer + expectedSize - 1);
            }

            // not enough memory to allocate
            throw new OutOfMemoryException();
        }

    }
}