using OSExp.ASM.Emulator;
using OSExp.Logger;
using OSExp.Processes;
using OSExp.Simulator;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OSExp
{
    public partial class Form1 : Form
    {
        Simulator.System system;
        ILogger logger = LogManager.GetLogger(typeof(Form1));
        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedItem = "Round-Robin";
            button4_Click(this, EventArgs.Empty);
            system.ProcessStateChanged += System_ProcessStateChanged;
            system.ProcessCreated += System_ProcessCreated;
            system.ProcessRunned += System_ProcessRunned;
            system.ProcessKilled += System_ProcessKilled;
            system.Interrupted += Cpu_Interrupted;
            stateMachine = system.Run().GetEnumerator();
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

        IEnumerator<int> stateMachine;
        private void button1_Click(object sender, EventArgs e)
        {
            if (!system.CanRun)
            {
                checkBox1.Checked = false;
                MessageBox.Show("No process can be scheduled now.", "Infomation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // move to next state
            if (!stateMachine.MoveNext()) {
                stateMachine = system.Run().GetEnumerator();
                return;
            }
            if (stateMachine.Current == 1)
            {
                // reset state machine
                stateMachine = system.Run().GetEnumerator();
            }
            refreshList();      
        }

        private void System_ProcessStateChanged(object sender, ProcessStateEventArgs e)
        {
            logger.Info($"{e.Process.Name} changed from {e.Before} to {e.Process.State}.");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var dialog = new AddProcessDialog();
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                system.CreateProcess(dialog.Process.Name, dialog.Process.Program, dialog.Process.Priority);
                refreshList();
            }
        }

        private void refreshList()
        {
            listView1.Items.Clear();
            foreach (var p in system.GetAllProcess())
            {
                var item = new ListViewItem(p.Name);
                item.SubItems.Add(p.Priority.ToString());
                item.SubItems.Add(p.LastRunTime.ToString());
                item.SubItems.Add(p.State.ToString());
                listView1.Items.Add(item);
            }
            toolStripStatusLabel1.Text = $"Process Count: {system.ProcessCount}";
            toolStripStatusLabel2.Text = $"CPU Time: {system.Time}";
        }

        private void Cpu_Interrupted(object sender, InterruptEventArgs e)
        {
            if (e.Code == 0x21)
            {
                Console.WriteLine(e.State.RegisterFrame.ToString());
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            button1_Click(this, EventArgs.Empty);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                timer1.Interval = int.Parse(textBox1.Text) / 2;
                timer1.Start();
            }
            else
            {
                timer1.Stop();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            switch(comboBox1.SelectedItem.ToString())
            {
                case "Round-Robin":
                    system = new RRSystem();
                    break;
                case "First-Come, First-Served":
                    system = new FPFSystem();
                    break;
            }
            timer1.Stop();
            toolStripStatusLabel3.Text = $"Algorithm: {comboBox1.SelectedItem.ToString()}";
            refreshList();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            system.KillTerminated();
            refreshList();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }
            var select = listView1.SelectedItems[0].Text;
            var findPro = system.GetAllProcess().Find(t => t.Name == select);

            if (findPro.State == State.Suspended)
            {
                system.ResumeProcess(select);
            }
            else
            {
                system.SuspendProcess(select);
            }

            refreshList();
        }
    }
}