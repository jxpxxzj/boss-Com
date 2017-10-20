using OSExp.ASM.Emulator;
using OSExp.ASM.Language;
using OSExp.Logger;
using OSExp.Simulator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace OSExp
{
    public partial class Form1 : Form
    {
        RRSystem system = new RRSystem();
        ILogger logger = LogManager.GetLogger(typeof(Form1));
        public Form1()
        {
            InitializeComponent();
            system.ProcessStateChanged += System_ProcessStateChanged;
            system.ProcessCreated += System_ProcessCreated;
            system.ProcessRunned += System_ProcessRunned;
            system.ProcessKilled += System_ProcessKilled;
        }

        private void System_ProcessKilled(object sender, ProcessEventArgs e)
        {
            logger.Info($"{e.Process.Name} killed.");
        }

        private void System_ProcessRunned(object sender, ProcessEventArgs e)
        {
            logger.Info($"{e.Process.Name} runned, timeRemain:{e.Process.RequestTime}.");
        }

        private void System_ProcessCreated(object sender, ProcessEventArgs e)
        {
            logger.Info($"{e.Process.Name} created, timeRequest:{e.Process.RequestTime}.");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            system.Run();
            refreshList();
        }

        private void System_ProcessStateChanged(object sender, ProcessStateEventArgs e)
        {
            logger.Info($"{e.Process.Name} changed from {e.Before} to {e.Process.State}.");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            system.CreateProcess();
            refreshList();
        }

        private void refreshList()
        {
            listView1.Items.Clear();
            foreach(var p in system.GetAllProcess())
            {
                var item = new ListViewItem(p.Name);
                item.SubItems.Add(p.Priority.ToString());
                item.SubItems.Add(p.RequestTime.ToString());
                item.SubItems.Add(p.State.ToString());
                listView1.Items.Add(item);
            }
        }

        private void saveExec(string fileName, byte[] data)
        {
            File.WriteAllBytes(fileName, data);
        }

        private List<SyntaxNode> loadExec(string fileName)
        {
            return Compiler.Decompile(File.ReadAllBytes(fileName));
        }
        private void button3_Click(object sender, EventArgs e)
        {
            var prog = Parser.Parse(textBox1.Text);
            var cpu = new Cpu();
            cpu.Interrupted += Cpu_Interrupted;
            cpu.LoadProgram(prog);
            cpu.RunToEnd();
        }

        private void Cpu_Interrupted(object sender, InterruptEventArgs e)
        {
            if (e.Code == 0x21)
            {
                Console.WriteLine(e.State.RegisterFrame.ToString());
            }
        }

        private void printByteToScreen(byte[] array)
        {
            for(var i=0;i<array.Length;i++)
            {
                if (i % 8 == 0 ) {
                    Console.WriteLine();
                }
                Console.Write(array[i].ToString("X").PadLeft(2, '0') + "  ");
            }
            Console.WriteLine();
        }
    }
}