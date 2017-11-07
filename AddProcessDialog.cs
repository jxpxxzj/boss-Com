using OSExp.ASM.Language;
using OSExp.Processes;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace OSExp
{
    public partial class AddProcessDialog : Form
    {

        public Process Process => new Process()
        {
            Program = Parser.Parse(textBox2.Text),
            Priority = (Priority)Enum.Parse(typeof(Priority), comboBox1.SelectedItem.ToString()),
            Name = textBox3.Text
        };

        public AddProcessDialog()
        {
            InitializeComponent();
            comboBox1.SelectedItem = "Normal";
            textBox3.Text = $"Process {(new Random(DateTime.Now.Millisecond).Next(99999)).ToString()}";

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var number = int.Parse(textBox1.Text);
            if(!string.IsNullOrWhiteSpace(textBox2.Text))
            {
                textBox2.Text += "\r\n";
            }
            var builder = new StringBuilder();
            for(int i = 0; i < number; i++)
            {
                builder.Append("nop\r\n");
            }
            textBox2.Text += builder.ToString();
           
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var selectDialog = new OpenFileDialog();
            selectDialog.Filter = "Assembly code (*.asm)|*.asm|Boss assembly code (*.bossasm)|*.bossasm|All files (*.*)|*.*";
            selectDialog.Multiselect = false;
            selectDialog.InitialDirectory = Application.StartupPath;
            selectDialog.Title = "Open code file...";
            var result = selectDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                var text = File.ReadAllText(selectDialog.FileName);
                textBox2.Text = text;
                textBox3.Text = $"{Path.GetFileNameWithoutExtension(selectDialog.FileName)} {(new Random(DateTime.Now.Millisecond).Next(99999)).ToString()}";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Process name can't be empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
