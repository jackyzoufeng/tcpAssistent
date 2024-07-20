using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace tcpAssistent
{
    public partial class CreateTcpClient : Form
    {
        public string serverip;
        public int port;
        public CreateTcpClient()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length <= 0)
            {
                MessageBox.Show($"IP is empty");
                return;
            }
            if (textBox2.Text.Length <= 0)
            {
                MessageBox.Show($"Port is empty");
                return;
            }
            bool isNum1 = Int32.TryParse(textBox2.Text, out port);
            if (!isNum1)
            {
                MessageBox.Show($"{textBox2.Text} is not a digit");
                return;
            }
            serverip = textBox1.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
