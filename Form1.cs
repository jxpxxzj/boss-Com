using OSExp.ASM.Emulator;
using OSExp.Logger;
using OSExp.Processes;
using OSExp.Simulator;
using System;
using System.Collections.Generic;
using System.Drawing;
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
            DoubleBuffered = true;
            button4_Click(this, EventArgs.Empty);
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
            if (!stateMachine.MoveNext())
            {
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
            var allMemory = 0;
            listView1.Items.Clear();
            foreach (var p in system.GetAllProcess())
            {
                var item = new ListViewItem(p.Name);
                item.SubItems.Add(p.Priority.ToString());
                item.SubItems.Add(p.LastRunTime.ToString());
                item.SubItems.Add(p.State.ToString());
                item.SubItems.Add(p.Memory.ToString());
                listView1.Items.Add(item);
                allMemory += p.MemorySize;
            }
            toolStripStatusLabel1.Text = $"Process Count: {system.ProcessCount}";
            toolStripStatusLabel2.Text = $"CPU Time: {system.Time}";
            toolStripStatusLabel4.Text = $"Memory: {allMemory} B / {system.MaxMemory} B, {Math.Round(allMemory * 1.0 / system.MaxMemory * 100, 2)}%";
            pictureBox1.Refresh();
            tableDialogInstance.RefreshList(system.MemoryTable);
        }

        private void Cpu_Interrupted(object sender, InterruptEventArgs e)
        {
            if (e.Code == 0x21)
            {
                switch(e.State.RegisterFrame.ax)
                {
                    case 0x4c00:
                        system.Cpu.Ret();
                        break;
                    case 0x2:
                        Console.Write((char)e.State.RegisterFrame.dx);
                        break;
                }
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
            switch (comboBox1.SelectedItem.ToString())
            {
                case "Round-Robin":
                    system = new RRSystem();
                    break;
                case "First-Come, First-Served":
                    system = new FPFSystem();
                    break;
            }
            timer1.Stop();
            system.ProcessStateChanged += System_ProcessStateChanged;
            system.ProcessCreated += System_ProcessCreated;
            system.ProcessRunned += System_ProcessRunned;
            system.ProcessKilled += System_ProcessKilled;
            system.Interrupted += Cpu_Interrupted;
            stateMachine = system.Run().GetEnumerator();
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

        private void button6_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }
            var select = listView1.SelectedItems[0].Text;
            var findPro = system.GetAllProcess().Find(t => t.Name == select);
            system.KillProcess(findPro.Name);
            refreshList();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var beginHeight = 5;
            var beginWidth = 30;
            var width = 30;

            var maxHeight = pictureBox1.Height - beginHeight * 2;
            var maxMemory = system.MaxMemory;

            float scale = maxHeight * 1.0f / maxMemory;

            var font = new Font("Segoe UI", 9.5f);

            e.Graphics.DrawRectangle(new Pen(Color.Red, 3), beginWidth, beginHeight, width, maxHeight);
            e.Graphics.DrawString(system.MaxMemory.ToString(), font, Brushes.Red, beginWidth + width + 5, maxHeight - 20);

            e.Graphics.DrawString("System Occupied", font, Brushes.Orange, beginWidth + width + 5, 0);
            e.Graphics.FillRectangle(Brushes.Orange, beginWidth, beginHeight, width, system.SystemSize * scale);
            e.Graphics.DrawRectangle(Pens.Black, beginWidth, beginHeight, width, system.SystemSize * scale);
            foreach (var p in system.GetAllProcess())
            {
                var size = p.Memory;
                float length = size.Length * scale;
                float beginPoint = size.Begin * scale + beginHeight;

                var color = Color.FromArgb((int)(Math.Sqrt(Math.Sqrt(length / maxMemory * 100 * 10) * 10) / 100 * 255), Color.Black);
                var brush = new SolidBrush(color);
                e.Graphics.FillRectangle(brush, beginWidth - 2, beginPoint, width + 2, length);
                e.Graphics.DrawRectangle(Pens.Black, beginWidth, beginPoint, width, length);
                e.Graphics.DrawString(size.Begin.ToString() + " - " + p.Name, font, Brushes.Black, beginWidth + width + 5, beginPoint);
            }

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            system.CompressMemory();
            refreshList();
        }

        MemoryTable tableDialogInstance = new MemoryTable();

        private void button8_Click(object sender, EventArgs e)
        {
            tableDialogInstance.Show();
        }
    }
}