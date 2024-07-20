using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tcpAssistent
{
    public partial class TcpSClientManager
    {
        public string serverip;
        public int serverport;
        public bool reconnectEnable = false;
        public bool start(string ip, int port)
        {
            this.serverip = ip;
            this.serverport = port;
            bool ret = true;
            try
            {
                tcpClient = new TcpClient(serverip, serverport);
                stream = tcpClient.GetStream();
                reconnectEnable = true;
                description = tcpClient.Client.RemoteEndPoint.ToString();
                stopThread = false;
                rec = new Thread(() => { ReadMsg(); });
                rec.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                ret = false;
            }
            return ret;
        }
        public void reconnect()
        {
            if (!reconnectEnable)
            {
                return;
            }
            start(serverip, serverport);
            if (context != null)
            {
                context.Post(new SendOrPostCallback((o) =>
                {
                    conncallback((bool)o);
                }), tcpClient.Connected);
            }
        }
    }
}
