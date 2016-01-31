using UnityEngine;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Runtime.CompilerServices;
public class TCPClient : MonoBehaviour{

    public TcpClient client;
    private NetworkStream stream;

    public class TCPStreamState
    {
        public NetworkStream WorkStream;
        public byte[] RecvBytes = new byte[1048576];

        public TCPStreamState(NetworkStream workStream)
        {
            WorkStream = workStream;
        }
    }

    public TCPClient(string ipAddress, int port, AsyncCallback connectionHandler)
    {
        client = new TcpClient();
        Debug.Log("lool");
        client.BeginConnect(ipAddress, port, connectionHandler, client);
        Debug.Log("begin");
    }

    public byte[] AppendLengthByte(byte[] origin)
    {
        int i = 0;
        byte[] length = BitConverter.GetBytes(origin.Length);
        length = Support.bytesReverse(length);
        byte[] extended = new byte[origin.Length + sizeof(int)];

        for (i = 0; i < sizeof(int); i++)
        {
            extended[i] = length[i];
        }
        for (; i < origin.Length + sizeof(int); i++)
        {
            extended[i] = origin[i - sizeof(int)];
        }

        return extended;
    }

    public void Send(byte[] data, AsyncCallback sentCallback)
    {
        stream = client.GetStream();
        data = AppendLengthByte(data);
        stream.BeginWrite(data, 0, data.Length, sentCallback, stream);
    }

    public void Read(AsyncCallback readCallback)
    {
        stream = client.GetStream();
        TCPStreamState state = new TCPStreamState(stream);
        stream.BeginRead(state.RecvBytes, 0, state.RecvBytes.Length, readCallback, state);
    }    

    private class Support
    {
        public static byte[] bytesReverse(byte[] origin)
        {
            Debug.Log("byte Count:" + origin.Length);
            var reverse = new byte[origin.Length];
            for(int i=0;i<origin.Length;i++)
            {
                reverse[i] = origin[origin.Length - i - 1];
            }

            return reverse;
        }
    }
}

