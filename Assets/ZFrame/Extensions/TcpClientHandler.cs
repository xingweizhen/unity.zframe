using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using NoToLua = XLua.BlackListAttribute;

namespace ZFrame.NetEngine
{
    using Lua;
    public class NetMsgHandler: INetMsgHandler
    {
        private readonly List<int> m_IDs = new List<int>();
        private readonly UnityAction<INetMsg> m_Handler;
        public NetMsgHandler(UnityAction<INetMsg> handler)
        {
            m_Handler = handler;
        }
        public NetMsgHandler Track(int msgId)
        {
            m_IDs.Add(msgId);
            return this;
        }
        public NetMsgHandler Untarck(int msgId)
        {
            m_IDs.Remove(msgId);
            return this;
        }

        bool INetMsgHandler.TryHandle(INetMsg nm)
        {
            if (m_IDs.Contains(nm.type)) {
                m_Handler.Invoke(nm);
                return true;
            }
            return false;
        }
    }

    public class TcpClientHandler : MonoBehavior, INetMsgHandler
    {
        [XLua.CSharpCallLua]
        public delegate void TcpClientEvent(TcpClientHandler tcp);

        [XLua.CSharpCallLua]
        public delegate void DelegateNetMsgHandler(TcpClientHandler tcp, int type, int readSize, int writeSize);

        [Description("当前连接")]
        private NetClient m_NC;
        private Queue<INetMsg> m_Msgs = new Queue<INetMsg>();
        private Coroutine m_Coro;
        private IPAddress m_BaseIp;

        public bool IsConnected { get { return m_NC.Connected; } }
        public string Error { get { return m_NC.error; } }
        public int latency { get { return m_NC.latency; } }

        [System.NonSerialized]
        public bool autoRecieve;

        [NoToLua]
        public bool supportIPv6 {
            get { return !string.IsNullOrEmpty(NetworkMgr.Instance.knownHost); }
        }

        public DelegateNetMsgHandler messageHandler;

        public TcpClientEvent onConnected;
        public TcpClientEvent onDisconnected;

        private Dictionary<string, INetMsgHandler> m_ExtHandler = new Dictionary<string, INetMsgHandler>();
        public void AddExtHandler(string name, INetMsgHandler handler)
        {
            if (!m_ExtHandler.ContainsKey(name)) {
                m_ExtHandler.Add(name, handler);
            } else {
                m_ExtHandler[name] = handler;
            }
        }

        public void DelExtHandler(string name)
        {
            m_ExtHandler.Remove(name);
        }

        private string m_Name;
        private void Logger(string message)
        {
            LogMgr.W("{0}:{1}", m_Name, message);
        }

        private void Awake()
        {
            m_NC = new NetClient(m_Msgs, null, null, Logger);
            autoRecieve = true;
        }

        private void Start()
        {
            m_Name = this.name;
        }

        private void Update()
        {
            if (autoRecieve) {
                RecieveAll();
            }
        }

        private void OnDestroy()
        {
            if (m_NC != null) m_NC.Close();
        }

        private void OnConnected()
        {
            if (onConnected != null) onConnected.Invoke(this);
        }

        private void OnDisconnected()
        {
            if (onDisconnected != null) onDisconnected.Invoke(this);
        }

        private IEnumerator CoroNetworkState(string host, int port, float timeout)
        {
            var addressFamily = AddressFamily.InterNetwork;
            if (supportIPv6) {
                IPAddress ipAddr;
                if (IPAddress.TryParse(host, out ipAddr)) {
                    LogMgr.D("GetAddressFamily: {0}", NetworkMgr.Instance.knownHost);
                    var ar = AsyncAddressFamily(this);
                    while (!ar.IsCompleted) yield return null;
                    if (m_BaseIp != null) {
                        addressFamily = m_BaseIp.AddressFamily;
                        if (addressFamily == AddressFamily.InterNetworkV6) {
                            var ipv4Bytes = ipAddr.GetAddressBytes();
                            var ipv6Bytes = m_BaseIp.GetAddressBytes();
                            for (int i = 0; i < 4; i++) {
                                ipv6Bytes[i + 12] = ipv4Bytes[i];
                            }
                            var ipv6 = new IPAddress(ipv6Bytes).ToString();
                            LogMgr.D("Convert ip address: {0} -> {1}", host, ipv6);
                            host = ipv6;
                        }
                    } else {
                        LogMgr.W("GetAddressFamily Failure, ignore ...");
                    }
                }
            }

            if (NetClient.SessionCreator == null) {
                LogMgr.E("未定义消息包创建器：NetClient.SessionCreator == null");
                yield break;
            }

            LogMgr.D("Connect -> {0}:{1} (timeout:{2})", host, port, timeout);
            m_NC.Connect(host, port, addressFamily);

            timeout += Time.realtimeSinceStartup;
            for (; ; ) {
                yield return null;

                if (timeout < Time.realtimeSinceStartup || m_NC.error != null) {
                    // 超时，关闭连接
                    m_NC.Close();
                    break;
                }

                if (m_NC.Connected) {
                    NetworkMgr.Log("Connected");
                    // 清空前一次连接的消息
                    m_Msgs.Clear();
                    OnConnected();
                    break;
                }
            }

            for (; ; ) {
                if (!m_NC.Connected) {
                    OnDisconnected();
                    break;
                }
                yield return null;
            }

            // 网络未连接
        }

        public void Connect(string host, int port, float timeout)
        {
            if (m_NC != null && !m_NC.connecting) {
                if (m_Coro != null) StopCoroutine(m_Coro);
                m_Coro = StartCoroutine(CoroNetworkState(host, port, timeout));
            }
        }

        [NoToLua]
        public void RecieveAll()
        {
            m_NC.UnpackMsgs(this);
        }

        public void Disconnect()
        {
            if (m_NC != null) {
                m_NC.Close();
            }
        }

        public void Send(INetMsg nm)
        {
            if (m_NC != null && m_NC.Connected) {
                m_NC.Send(nm);
            }
        }

        bool INetMsgHandler.TryHandle(INetMsg nm)
        {
            LibNetwork.readNm = nm;
            var read = false;
            try {
                foreach (var handler in m_ExtHandler.Values) {
                    if (handler.TryHandle(nm)) {
                        read = true;
                        break;
                    }
                }

                if (!read && messageHandler != null) {
                    UnityEngine.Profiling.Profiler.BeginSample("Unpacking NetMsg");
                    messageHandler.Invoke(this, nm.type, nm.bodySize, nm.size);
                    UnityEngine.Profiling.Profiler.EndSample();
                }
            } catch (System.Exception e) {
                Debug.LogException(e);
            } finally {
                LibNetwork.readNm = null;
            }
            return true;
        }

        private static System.IAsyncResult AsyncAddressFamily(TcpClientHandler tcp)
        {
            tcp.m_BaseIp = null;
            return Dns.BeginGetHostAddresses(NetworkMgr.Instance.knownHost, ar => {
                var addresses = Dns.EndGetHostAddresses(ar);
                if (addresses != null && addresses.Length > 0) {
                    ((TcpClientHandler)ar.AsyncState).m_BaseIp = addresses[0];
                }
            }, tcp);
        }
    }
}
