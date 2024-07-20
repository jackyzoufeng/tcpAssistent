using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tcpAssistent
{
    public partial class TcpSClientUC : UserControl
    {
        TcpSClientManager tscmm;
        public TcpSClientUC(TcpSClientManager tscm)
        {
            InitializeComponent();

            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.Columns.Add("no", 60, HorizontalAlignment.Center);
            listView1.Columns.Add("context", 300, HorizontalAlignment.Center);

            tscm.setContext(WindowsFormsSynchronizationContext.Current, ThreadCallback, connCallback);
            tscmm = tscm;

            label1.Text = "connected";

            button2.Visible = tscmm.reconnectEnable;
        }
        public void ThreadCallback(recvData cd)
        {
            ListViewItem item = new ListViewItem((listView1.Items.Count + 1).ToString());
            item.Tag = cd;
            item.SubItems.Add(Encoding.UTF8.GetString(cd.data));
            listView1.Items.Add(item);
        }

        public void connCallback(bool bconn)
        {
            button1.Enabled = bconn;
            label1.Text = bconn ? "connected" : "disconnected";
            button2.Enabled = !bconn;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tscmm.disconnect();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0)
            {
                tscmm.sendmessage(textBox1.Text);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tscmm.reconnect();
        }
    }
}
