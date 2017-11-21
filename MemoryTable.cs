using OSExp.Simulator;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OSExp
{
    public partial class MemoryTable : Form
    {
        public MemoryTable()
        {
            InitializeComponent();
        }

        public void RefreshList(List<MemoryAllocation> list)
        {
            listView1.Items.Clear();
            foreach (var i in list)
            {
                var item = new ListViewItem(i.Type.ToString());
                item.SubItems.Add(i.Begin.ToString());
                item.SubItems.Add(i.End.ToString());
                item.SubItems.Add(i.Length.ToString());
                item.SubItems.Add(i.Process != null ? i.Process.Name : "");
                listView1.Items.Add(item);
            }
        }
        public MemoryTable(List<MemoryAllocation> list)
        {
            InitializeComponent();
            RefreshList(list);
        }

        private void MemoryTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Visible = false;
        }
    }
}
