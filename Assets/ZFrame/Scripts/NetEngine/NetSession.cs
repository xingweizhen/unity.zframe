using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using clientlib.net;
using UnityEngine.Assertions;

namespace ZFrame.NetEngine
{
    public class NetSession : INetSession
    {
        public delegate INetMsg CreateNetMsgDelegate(byte[] buffer, int index, int length);

        public bool connecting { get; private set; }

        public bool connected { get { return m_Tcp != null && m_Tcp.Connected; } }

        public Exception lastError { get; private set; }

        public int latency { get; set; }

        public long sendTicks { get; private set; }

        public Action<Exception> onException { get; set; }
        public INetMsg msg { get; set; }

        private CreateNetMsgDelegate m_MsgCreator;

        private INetSessionEvent m_Event;
        private AsyncCallback m_WriteCallback;        
        private TcpClient m_Tcp;

        private const int BUFF_LEN = 1024 * 10;
        private byte[] m_ReadBuffer = new byte[BUFF_LEN];
        private int m_StartPos = 0;
        private int m_EndPos = 0;

        public NetSession(CreateNetMsgDelegate nmCreator)
        {
            Assert.IsNotNull(nmCreator);

            m_MsgCreator = nmCreator;
            m_WriteCallback = new AsyncCallback(OnWriteStream);
        }

        public void Connect(string host, int port, AddressFamily addressFamily, INetSessionEvent sessionEvent)
        {
            m_Event = sessionEvent;

            if (m_Tcp != null) {
                m_Tcp.Close();
                m_Tcp = null;
            }
            try {
                bool nodelay = true;
#if UNITY_EDITOR || UNITY_STANDALONE
                const string TCP_SETTINGS = "tcp-settings.txt";
                if (System.IO.File.Exists(TCP_SETTINGS)) {
                    string cfg = System.IO.File.ReadAllText(TCP_SETTINGS);
                    var js = TinyJSON.JSON.Load(cfg);
                    nodelay = js["nodelay"];
                }
#endif

                m_Tcp = new TcpClient(addressFamily) { NoDelay = nodelay };
                m_Tcp.Client.NoDelay = nodelay;
                m_Tcp.BeginConnect(host, port, s_TcpConnectCallback, this);
                connecting = true;
            } catch (Exception ex) {
                OnException(ex);
            }
        }

        public void Free()
        {
            if (m_Tcp != null) {
                m_Tcp.Close();
                m_Tcp = null;
            }
        }

        public bool Send(INetMsg msg)
        {
            var stream = m_Tcp.GetStream();
            if (stream.CanWrite) {
                try {
                    msg.Serialize();
                    stream.BeginWrite(msg.data, 0, msg.size, m_WriteCallback, msg);
                } catch (Exception ex) {
                    msg.Recycle();
                    OnException(ex);
                    return false;
                }
            }
            return true;
        }

        private void OnException(Exception ex)
        {
            lastError = ex;
            if (m_Event != null) m_Event.OnException(this, ex);

            if (onException != null) onException.Invoke(ex);
            if (msg != null) {
                msg.Recycle();
                msg = null;
            }

            OnClosed();
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        private void OnClosed()
        {
            Free();

            //调用完成读取来通知外部，连接断开
            if (m_Event != null) m_Event.OnMessageRead(this, null);
        }

        private void OnConnected(IAsyncResult ar)
        {
            connecting = false;
            try {
                m_Tcp.EndConnect(ar);
                if (m_Tcp.Connected) {
                    if (m_Event != null) m_Event.OnConnected(this);
                    BeginRead();
                } else {
                    OnException(new Exception("TCP连接已完成，但未建立连接。"));
                }
            } catch (Exception ex) {
                OnException(ex);
            }
        }

        private void OnWriteStream(IAsyncResult ar)
        {
            var nm = ar.AsyncState as INetMsg;
            try {
                var stream = m_Tcp.GetStream();
                stream.EndWrite(ar);
                if (m_Event != null) m_Event.OnMessageWrite(this, nm);
            } catch (System.Exception ex) {
                OnException(ex);
            }

            if (nm != null) nm.Recycle();
        }

        private void BeginRead()
        {
            var stream = m_Tcp.GetStream();
            Assert.IsFalse(m_StartPos > m_EndPos);
            // 循环使用buffer
            if (m_StartPos < m_EndPos) {
                if (m_EndPos == m_ReadBuffer.Length) {
                    m_EndPos -= m_StartPos;
                    Array.Copy(m_ReadBuffer, m_StartPos, m_ReadBuffer, 0, m_EndPos);
                    m_StartPos = 0;
                }
            } else {
                m_StartPos = m_EndPos = 0;
            }
            stream.BeginRead(m_ReadBuffer, m_EndPos, m_ReadBuffer.Length - m_EndPos,
                m_TcpStreamReadCallback, this);
        }

        private void OnReadStream(IAsyncResult ar)
        {
            try {
                var stream = m_Tcp.GetStream();
                m_EndPos += stream.EndRead(ar);

                while (m_StartPos < m_EndPos) {
                    var bufferLen = m_EndPos - m_StartPos;
                    if (msg == null) {
                        msg = m_MsgCreator.Invoke(m_ReadBuffer, m_StartPos, bufferLen);
                    }
                    if (msg != null) {
                        int nRead = msg.ReadBuffer(m_ReadBuffer, m_StartPos, bufferLen);
                        if (nRead <= 0) {
                            throw new Exception("read data error");
                        }

                        m_StartPos += nRead;
                        if (msg.IsComplete()) {
                            msg.Deserialize();
                            if (m_Event != null) m_Event.OnMessageRead(this, msg);
                            msg = null;
                        }
                    } else break;
                }

                BeginRead();
            } catch (Exception ex) {
                OnException(ex);
            }
        }

        private static AsyncCallback s_TcpConnectCallback = (ar) => {
            var session = ar.AsyncState as NetSession;
            if (session != null) {
                session.OnConnected(ar);
            } else {

            }
        };

        private static AsyncCallback m_TcpStreamReadCallback = (ar) => {
            var session = ar.AsyncState as NetSession;
            if (session != null) {
                session.OnReadStream(ar);
            } else {

            }
        };
    }
}
