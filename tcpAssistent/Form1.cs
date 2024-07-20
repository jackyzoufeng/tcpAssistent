using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tcpAssistent
{
    public enum TCP_TYPE
    {
        TCP_TYPE_SERVER,
        TCP_TYPE_CLIENT
    }
    public partial class Form1 : Form
    {
        ContextMenuStrip tcpServerMS = new ContextMenuStrip();
        ContextMenuStrip tcpClientMS = new ContextMenuStrip();

        TreeNode treeNodeTcpServer = new TreeNode("TCP Server");
        TreeNode treeNodeTcpClient = new TreeNode("TCP Client");

        ContextMenuStrip nodeMenu = new ContextMenuStrip();
        public Form1()
        {
            InitializeComponent();

            ToolStripMenuItem SMSCreate = new ToolStripMenuItem();
            SMSCreate.Text = "Create Service";
            SMSCreate.Click += SMSCreate_Click;
            tcpServerMS.Items.Add(SMSCreate);

            ToolStripMenuItem CMSCreate = new ToolStripMenuItem();
            CMSCreate.Text = "Create Client";
            CMSCreate.Click += CMSCreate_Click;
            tcpClientMS.Items.Add(CMSCreate);

            ToolStripMenuItem closeServer = new ToolStripMenuItem();
            closeServer.Text = "Close";
            closeServer.Click += Close_Click;
            nodeMenu.Items.Add(closeServer);

            treeNodeTcpServer.Tag = TCP_TYPE.TCP_TYPE_SERVER;
            treeNodeTcpServer.ContextMenuStrip = tcpServerMS;
            treeView1.Nodes.Add(treeNodeTcpServer);

            treeNodeTcpClient.Tag = TCP_TYPE.TCP_TYPE_CLIENT;
            treeNodeTcpClient.ContextMenuStrip = tcpClientMS;
            treeView1.Nodes.Add(treeNodeTcpClient);
        }

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point ClickPoint = new Point(e.X, e.Y);
                TreeNode CurrentNode = treeView1.GetNodeAt(ClickPoint);
                if (CurrentNode != null)
                {
                    treeView1.SelectedNode = CurrentNode;//选中这个节点
                }
            }
        }

        private void SMSCreate_Click(object sender, EventArgs e)
        {
            CreateTcpServer createTcpServer = new CreateTcpServer();
            if (createTcpServer.ShowDialog() == DialogResult.OK)
            {
                TcpListenerManager tlm = new TcpListenerManager();
                if (tlm.start(createTcpServer.port))
                {
                    tlm.setContext(WindowsFormsSynchronizationContext.Current, acceptCallback);
                    TreeNode server = new TreeNode($"port:{createTcpServer.port}");
                    server.ContextMenuStrip = nodeMenu;
                    server.Tag = tlm;
                    treeNodeTcpServer.Nodes.Add(server);
                    treeNodeTcpServer.Expand();
                }
            }
        }

        private void acceptCallback(TcpListenerManager manager, TcpClient client)
        {
            foreach (TreeNode item in treeNodeTcpServer.Nodes)
            {
                if ((item.Tag.GetType() == typeof(TcpListenerManager)) && (manager == (TcpListenerManager)item.Tag))
                {
                    TcpSClientManager tscm = new TcpSClientManager();
                    if (tscm.start(client))
                    {
                        TreeNode ct = new TreeNode($"{tscm.ToString()}");
                        ct.ContextMenuStrip = nodeMenu;
                        ct.Tag = tscm;
                        item.Nodes.Add(ct);
                        item.Expand();
                    }
                    break;
                }
            }
        }

        private void CMSCreate_Click(object sender, EventArgs e)
        {
            CreateTcpClient createTcpClient = new CreateTcpClient();
            if (createTcpClient.ShowDialog() == DialogResult.OK)
            {
                TcpSClientManager tcm = new TcpSClientManager();
                if (tcm.start(createTcpClient.serverip, createTcpClient.port))
                {
                    TreeNode ct = new TreeNode($"{tcm.ToString()}");
                    ct.ContextMenuStrip = nodeMenu;
                    ct.Tag = tcm;
                    treeNodeTcpClient.Nodes.Add(ct);
                    treeNodeTcpClient.Expand();
                }
            }
        }

        private void Close_Click(object sender, EventArgs e)
        {
            TreeNode CurrentNode = treeView1.SelectedNode;
            CloseNode(CurrentNode);
        }

        private void CloseNode(TreeNode node)
        {
            if (node.Tag.GetType() == typeof(TcpListenerManager))
            {
                TcpListenerManager tlm = (TcpListenerManager)node.Tag;
                tlm.stop();

                foreach (TreeNode item in node.Nodes)
                {
                    if (item.Tag.GetType() == typeof(TcpSClientManager))
                    {
                        TcpSClientManager tscm = (TcpSClientManager)item.Tag;
                        tscm.stop();
                        foreach (TabPage page in tabControl1.TabPages)
                        {
                            if (tscm == page.Tag)
                            {
                                tabControl1.TabPages.Remove(page);
                            }
                        }
                    }
                }
                node.Parent.Nodes.Remove(node);
                //treeNodeTcpServer.Nodes.Remove(CurrentNode);
            }
            else if (node.Tag.GetType() == typeof(TcpSClientManager))
            {
                TcpSClientManager tscm = (TcpSClientManager)node.Tag;
                tscm.stop();
                foreach (TabPage page in tabControl1.TabPages)
                {
                    if (tscm == page.Tag)
                    {
                        tabControl1.TabPages.Remove(page);
                    }
                }
                node.Parent.Nodes.Remove(node);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode CurrentNode = e.Node;
            if (CurrentNode != null)
            {
                if (CurrentNode.Tag.GetType() == typeof(TcpSClientManager))
                {
                    bool found = false;
                    TcpSClientManager tscm = (TcpSClientManager)CurrentNode.Tag;
                    foreach (TabPage page in tabControl1.TabPages)
                    {
                        if (tscm == page.Tag)
                        {
                            tabControl1.SelectTab(page);
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        TabPage client_page = new TabPage($"{tscm.ToString()}");
                        client_page.Tag = tscm;
                        client_page.Dock = DockStyle.Fill;
                        TcpSClientUC tsuc = new TcpSClientUC(tscm);
                        tsuc.Dock = DockStyle.Fill;
                        tsuc.Parent = client_page;
                        tabControl1.TabPages.Add(client_page);
                        tabControl1.SelectTab(client_page);
                    }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            TreeNode[] nodes1 = new TreeNode[treeNodeTcpServer.Nodes.Count];
            treeNodeTcpServer.Nodes.CopyTo(nodes1, 0);
            TreeNode[] nodes2 = new TreeNode[treeNodeTcpClient.Nodes.Count];
            treeNodeTcpClient.Nodes.CopyTo(nodes2, 0);
            foreach (TreeNode item in nodes1)
            {
                CloseNode(item);
            }
            foreach (TreeNode item in nodes2)
            {
                CloseNode(item);
            }
        }
    }
}
