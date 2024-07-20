using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tcpAssistent
{
    public partial class TcpSClientManager
    {
        TcpClient tcpClient;
        NetworkStream stream;
        Thread rec;
        bool stopThread = true;
        string description;
        SynchronizationContext context = null;
        System.Action<recvData> callback;
        System.Action<bool> conncallback;
        List<recvData> recvs = new List<recvData>();
        public TcpSClientManager() 
        { 
        }
        public bool start(TcpClient client)
        {
            bool ret = true;
            try
            {
                tcpClient = client;
                stream = tcpClient.GetStream();
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
        public void setContext(SynchronizationContext cxt, System.Action<recvData> rd, System.Action<bool> bd)
        {
            context = cxt;
            callback = rd;
            conncallback = bd;
            foreach (recvData data1 in recvs)
            {
                context.Post(new SendOrPostCallback((o) =>
                {
                    callback((recvData)o);
                }), data1);
            }
            context.Post(new SendOrPostCallback((o) =>
            {
                conncallback((bool)o);
            }), tcpClient.Connected);
        }
        public void stop()
        {
            stopThread = true;
            stream.Close();
            tcpClient.Close();
            rec.Join();
            Console.WriteLine($"{this} was stopped");
        }
        public void disconnect()
        {
            stop();
        }
        public void sendmessage(string msg)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(msg);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Console.WriteLine($"connect:{tcpClient.Connected}");
                if (context != null)
                {
                    context.Post(new SendOrPostCallback((o) =>
                    {
                        conncallback((bool)o);
                    }), tcpClient.Connected);
                }
            }
        }
        public override string ToString()
        {
            return description;
        }
        private void ReadMsg()
        {
            while (!stopThread)
            {
                try
                {
                    byte[] data = new byte[1024];
                    //0 表示从数组的哪个索引开始存放数据
                    //1024表示最大读取的字节数
                    int length = stream.Read(data, 0, 1024);//读取数据
                    if (length <= 0)
                    {
                        //stopThread = true;
                        stream.Close();
                        Console.WriteLine($"connect:{tcpClient.Connected}");
                        //tcpClient.Close();
                        if (context != null)
                        {
                            context.Post(new SendOrPostCallback((o) =>
                            {
                                conncallback((bool)o);
                            }), tcpClient.Connected);
                        }
                        break;
                    }
                    recvData data1 = new recvData();
                    data1.data = data;
                    recvs.Add(data1);
                    if (context != null)
                    {
                        context.Post(new SendOrPostCallback((o) =>
                        {
                            callback((recvData)o);
                        }), data1);
                    }
                    string message = Encoding.UTF8.GetString(data, 0, length);
                    Console.WriteLine($"receive data `{message}`");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine($"connect:{tcpClient.Connected}");
                    if (context != null)
                    {
                        context.Post(new SendOrPostCallback((o) =>
                        {
                            conncallback((bool)o);
                        }), tcpClient.Connected);
                    }
                    break;
                }
            }
        }
    }
    public class recvData
    {
        public byte[] data;
    }
}
