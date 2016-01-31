using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
public class UDPClient : MonoBehaviour {
    public class UdpState
    {
        public UdpClient udpClient;
        public IPEndPoint ipEndPoint;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public int counter = 0;
    }

    public class AsyncUdpSever
    {
        private IPEndPoint ipEndPoint = null;
        private IPEndPoint remoteEP = null;
        private UdpClient udpReceive = null;
        private UdpClient udpSend = null;
        private const int listenPort = 1100;
        private const int remotePort = 1101;
        UdpState udpReceiveState = null;
        UdpState udpSendState = null;
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);
        public AsyncUdpSever()
        {
            ipEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
            remoteEP = new IPEndPoint(Dns.GetHostAddresses(Dns.GetHostName())[0], remotePort);
            udpReceive = new UdpClient(ipEndPoint);
            udpSend = new UdpClient();
            udpReceiveState = new UdpState();
            udpReceiveState.udpClient = udpReceive;
            udpReceiveState.ipEndPoint = ipEndPoint;

            udpSendState = new UdpState();
            udpSendState.udpClient = udpSend;
            udpSendState.ipEndPoint = remoteEP;
        }
        public void ReceiveMsg()
        {
            Console.WriteLine("listening for messages");
            while (true)
            {
                lock (this)
                {
                    IAsyncResult iar = udpReceive.BeginReceive(new AsyncCallback(ReceiveCallback), udpReceiveState);
                    receiveDone.WaitOne();
                    Thread.Sleep(100);
                }
            }
        }
        private void ReceiveCallback(IAsyncResult iar)
        {
            UdpState udpReceiveState = iar.AsyncState as UdpState;
            if (iar.IsCompleted)
            {
                Byte[] receiveBytes = udpReceiveState.udpClient.EndReceive(iar, ref udpReceiveState.ipEndPoint);
                string receiveString = Encoding.ASCII.GetString(receiveBytes);
                Console.WriteLine("Received: {0}", receiveString);
                //Thread.Sleep(100);  
                receiveDone.Set();
                SendMsg();
            }
        }

        private void SendMsg()
        {
            udpSend.Connect(udpSendState.ipEndPoint);
            udpSendState.udpClient = udpSend;
            udpSendState.counter++;

            string message = string.Format("第{0}个UDP请求处理完成！", udpSendState.counter);
            Byte[] sendBytes = Encoding.Unicode.GetBytes(message);
            udpSend.BeginSend(sendBytes, sendBytes.Length, new AsyncCallback(SendCallback), udpSendState);
            sendDone.WaitOne();
        }
        private void SendCallback(IAsyncResult iar)
        {
            UdpState udpState = iar.AsyncState as UdpState;
            Console.WriteLine("第{0}个请求处理完毕！", udpState.counter);
            Console.WriteLine("number of bytes sent: {0}", udpState.udpClient.EndSend(iar));
            sendDone.Set();
        }

        public static void Main()
        {
            AsyncUdpSever aus = new AsyncUdpSever();
            Thread t = new Thread(new ThreadStart(aus.ReceiveMsg));
            t.Start();
            Console.Read();
        }
    }  
}
