using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tcpAssistent
{
    public class TcpListenerManager
    {
        TcpListener listener;
        public int tcpPort;
        Thread rec;
        bool stopThread = true;
        SynchronizationContext context = null;
        System.Action<TcpListenerManager, TcpClient> callback;
        List<TcpClient> clients = new List<TcpClient>();
        public TcpListenerManager()
        {
        }

        public bool start(int port)
        {
            bool ret = true;
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                tcpPort = port;
                stopThread = false;
                rec = new Thread(() => { accept(); });
                rec.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                ret = false;
            }
            return ret;
        }

        public void setContext(SynchronizationContext cxt, System.Action<TcpListenerManager, TcpClient> tc)
        {
            context = cxt;
            callback = tc;
        }

        public void stop()
        {
            stopThread = true;
            listener.Stop();
            rec.Join();
            Console.WriteLine($"{this} was stopped");
        }

        private void accept()
        {
            listener.Start();
            while (!stopThread)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    clients.Add(client);
                    if (context != null)
                    {
                        context.Post(new SendOrPostCallback((o) =>
                        {
                            callback(this, client);
                        }), null);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
