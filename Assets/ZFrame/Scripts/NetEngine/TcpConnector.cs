using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ZFrame.NetEngine
{
    public class TcpConnector
    {
        public delegate void ConnectedDelegate(string name, string host, ushort port, bool isConnected);

        string name;
        public string Name { get { return name; } }
        public TcpConnector(string name)
        {
            this.name = name;
        }

        string mHost;
        ushort mPort;
        TcpClient mTcp;
        ConnectedDelegate OnConnected;

        const int PER_SIZE = 1024;
        const int MAX_SIZE = PER_SIZE * 10;
        // Total received buffer
        byte[] TotalBuffer = new byte[MAX_SIZE];
        int totalSize = 0;
        // buffer to received
        // byte[] RecvBuffer = new byte[PER_SIZE];
        // int recvSize = 0;
        // buffer to send
        byte[] SendBuffer = new byte[PER_SIZE];
        int sendSize = 0;

        string info { get { return string.Format("[TCP={0}:{1}]", mHost, mPort); } }

        // Read received data from system buffer to custom buffer.
        void doRead()
        {
            NetworkStream stream = mTcp.GetStream();
            if (stream.CanRead) {
                if (MAX_SIZE - totalSize >= PER_SIZE) {
                    stream.BeginRead(TotalBuffer, totalSize, PER_SIZE, new AsyncCallback(asyncRecv), null);
                } else {
                    LogMgr.W("{0} Buffer is full, blocking...", info);
                }
            }
        }

        private void asyncConnect(IAsyncResult iar)
        {
            TcpClient tcp = iar.AsyncState as TcpClient;
            try {
                if (tcp.Connected) {
                    LogMgr.I("{0} Connection successful!", info);
                    tcp.EndConnect(iar);
                } else {
                    LogMgr.W("{0} Connection failure!", info);
                }
                if (OnConnected != null) OnConnected(name, mHost, mPort, tcp.Connected);
            } catch (SocketException e) {
                LogMgr.E("{0} Connetion error: {1}", info, e.Message);
            }
        }

        private void asyncRecv(IAsyncResult iar)
        {
            if (mTcp != null && mTcp.Connected) {
                int bytesRead = mTcp.GetStream().EndRead(iar);
                if (bytesRead > 0) {
                    totalSize += bytesRead;
                    doRead();
                } else {
                    LogMgr.W("{0} Remote connection closed.", info);
                }
            } else {
                LogMgr.W("{0} Did you close the connection while receiving?", info);
            }
        }

        private void asyncSend(IAsyncResult iar)
        {
            if (mTcp != null && mTcp.Connected) {
                mTcp.GetStream().EndWrite(iar);
            } else {
                LogMgr.W("{0} Did you close the connection while sending?", info);
            }
        }

        // Create asnyc connection.
        public void Connect(string host, ushort port, ConnectedDelegate onConnected)
        {
            if (mTcp == null) {
                mTcp = new TcpClient(AddressFamily.InterNetwork);
            } else {
                Disconnect();
            }

            mHost = host;
            mPort = port;
            OnConnected = onConnected;
            LogMgr.I("{0} Begin connection.", info);
            mTcp.BeginConnect(mHost, mPort, new AsyncCallback(asyncConnect), mTcp);
        }

        // Close connection
        public void Disconnect()
        {
            if (mTcp != null && mTcp.Connected) {
                LogMgr.I("{0} Close connection.", info);
                mTcp.Close();
            }
        }

        // Send a Protocol Package - Directly
        public void Send(byte[] data, int offset = 0, int size = 0)
        {
            if (mTcp != null && mTcp.Connected) {
                if (size == 0) size = data.Length;
                if (offset + size > data.Length) size = data.Length - offset;
                mTcp.GetStream().BeginWrite(data, offset, size, new AsyncCallback(asyncSend), null);
            } else {
                LogMgr.W("Try to Send when TCP is {0}, {1}", mTcp, mTcp == null ? false : mTcp.Connected); 
            }
        }

        /* Send a Protocol Package - Push
         * Usage:
         * 1. SendStart()
         * 2. PushData() * n
         * 3. SendEnd()
         */
        public void SendStart()
        {
            sendSize = 0;
        }

        public void PushData(byte[] data)
        {
            Array.Copy(data, 0, SendBuffer, sendSize, data.Length);
            sendSize += data.Length;
        }

        public void SendEnd()
        {
            Send(SendBuffer);
        }

        // Try to receive a Protocol Package - Directly
        public byte[] Recv()
        {
            if (mTcp != null && mTcp.Connected) {
                if (totalSize > 0) {
                    // Unpacking
                    doRead();
                }
            }
            return null;
        }
    }
}
