﻿using OSExp.Processes;
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
                    return 0;
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

        public void Run()
        {
            SortList();
            var process = GetRunnableProcess();
            if (process == null)
            {
                return;
            }
            var stateBefore = process.State;
            process.State = State.Running;

            var timeCost = RunProcess(process);

            process.CpuTime += timeCost;
            Time += timeCost;
            process.State = State.Ready;
            if (process.RequestTime == 0)
            {
                process.State = State.Terminated;
                OnProcessStateChange(process, stateBefore);
            }
            OnProcessRun(process);
            SortList();
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
        public void UnsuspendProcess(string processName)
        {
            var find = SuspendedList.Find(t => t.Name == processName);
            if (find != null)
            {
                SuspendedList.Remove(find);
                find.State = State.Waiting;
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

        private void addNewTask()
        {
            var (name, time) = TaskPool.GenerateTask();
            CreateProcess(name, time);
        }
        public void CreateProcess()
        {
            addNewTask();
        }
        public void CreateProcess(string processName, int requestTime, Priority priority = Priority.Normal)
        {
            CheckChannel();
            var findInProcess = ProcessList.Find(t => t.Name == processName);
            var findInSuspend = SuspendedList.Find(t => t.Name == processName);
            if (findInProcess == null && findInSuspend == null)
            {
                var process = new Process()
                {
                    Name = processName,
                    RequestTime = requestTime,
                    Priority = priority,
                    State = State.Created,
                    CreateTime = Time
                };
                ProcessList.Add(process);
                SortList();
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
                ProcessList.FindAll(t => t.State == State.Terminated).ForEach(t => KillProcess(t.Name));
            }
            if (ProcessCount == ChannelCount)
            {
                throw new TooManyProcessesException();
            }
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
    }
}