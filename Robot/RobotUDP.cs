using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Robot
{
    internal class RobotUDP
    {
        private UdpClient udpClient;
        private int port;
        public RobotUDP(int port)
        {
            this.port = port;
            udpClient = new UdpClient(port);
        }
        public void StartListening()
        {
            udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            if (udpClient.Client != null)
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
                byte[] receivedBytes = udpClient.EndReceive(ar, ref remoteEndPoint);
                string receivedMessage = Encoding.Default.GetString(receivedBytes);

                // 处理接收到的消息，比如触发事件或调用回调函数
                 OnMessageReceived(receivedMessage, remoteEndPoint);

                // 继续接收下一个数据报
                udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            }
    
        }
        public void SendMessage(string message, string ipAddress)
        {
            if (udpClient.Client != null)
            {
                byte[] data = Encoding.Default.GetBytes(message);
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                udpClient.Send(data, data.Length, remoteEndPoint);
            }
                
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        protected virtual void OnMessageReceived(string message, IPEndPoint remoteEndPoint)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message, remoteEndPoint));
        }

        public void Close()
        {
            udpClient.Close();
        }
    }
    public class MessageReceivedEventArgs : EventArgs
    {
        public string Message { get; }
        public IPEndPoint RemoteEndPoint { get; }

        public MessageReceivedEventArgs(string message, IPEndPoint remoteEndPoint)
        {
            Message = message;
            RemoteEndPoint = remoteEndPoint;
        }
    }

}
