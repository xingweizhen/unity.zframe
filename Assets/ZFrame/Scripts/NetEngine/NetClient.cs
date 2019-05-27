using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

// warning CS0168: 声明了变量，但从未使用
// warning CS0219: 给变量赋值，但从未使用
#pragma warning disable 0168, 0219, 0414
namespace ZFrame.NetEngine
{ 
    /// <summary>
    /// 网络客户端
    /// </summary>
    public class NetClient : INetClient, INetSessionEvent
    {
        public delegate void ConnectedHnadler(NetClient client);
        public delegate void DisconnectedHnadler(NetClient client);

        public ConnectedHnadler onConnected;
        public DisconnectedHnadler onDisconnected;

        private INetSession _nowSession;

        private Queue<INetMsg> _receiveQueue;

        public System.Action<string> errLogger;
        string mErr;
        public string error { get { return mErr; } private set { mErr = value.Trim(); if (errLogger != null) errLogger(mErr); } }
            
        /// <summary>
        /// 
        /// </summary>
        /// <param name="receiveQueue">接受消息队列</param>
        /// <param name="onConnected">连接上服务器端回调</param>
        /// <param name="onDisconnected">连接断开回调</param>
        public NetClient(Queue<INetMsg> receiveQueue, ConnectedHnadler onConnected, DisconnectedHnadler onDisconnected)
        {
            _receiveQueue = receiveQueue;
            this.onDisconnected = onDisconnected;
            this.onConnected = onConnected;
            this.errLogger = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="receiveQueue">接受消息队列</param>
        /// <param name="onConnected">连接上服务器端回调</param>
        /// <param name="onDisconnected">连接断开回调</param>
        public NetClient(Queue<INetMsg> receiveQueue, ConnectedHnadler onConnected, DisconnectedHnadler onDisconnected, System.Action<string> logger)
        {
            _receiveQueue = receiveQueue;
            this.onDisconnected = onDisconnected;
            this.onConnected = onConnected;
            this.errLogger = logger;
        }

        /// <summary>
        /// 是否连接上服务器
        /// </summary>
        public bool Connected {

            get {
                return _nowSession != null && _nowSession.connected;
            }
        }

        public bool connecting { get { return _nowSession != null && _nowSession.connecting; } }

        public int latency { get { return _nowSession != null ? _nowSession.latency : 0; } }

        /// <summary>
        /// 连接到某个网络地址
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="addressFamily"></param>
        public void Connect(string host, int port, INetSession session, AddressFamily addressFamily = AddressFamily.InterNetwork)
        {
            if (string.IsNullOrEmpty(host) || port < 1) return;
            mErr = null;
            if (_nowSession != null) {
                _nowSession.Free();
            }

            _nowSession = session;;
            _nowSession.Connect(host, port, addressFamily, this);
        }

        void INetSessionEvent.OnConnected(INetSession session)
        {
            if (session != _nowSession) {
                session.Free();
                return;
            }

            if (session.connected) {
                if (onConnected != null) onConnected(this);
            }
        }

        void INetSessionEvent.OnMessageRead(INetSession session, INetMsg msg)
        {
            if (session != _nowSession) {
                session.Free();
                return;
            }

            //判断session状态，只有保持连接状态才能进行消息读取，否则是连接断开或者发生错误
            if (!session.connected) {
                //连接断开，需要判断错误信息
                if (session.lastError != null) {
                    //错误信息
                    error = session.lastError.ToString();
                }

                if (onDisconnected != null) {
                    onDisconnected(this);
                }
                return;
            }

            //获得消息
            _receiveQueue.Enqueue(msg);
        }

        void INetSessionEvent.OnMessageWrite(INetSession session, INetMsg msg)
        {

        }

        void INetSessionEvent.OnException(INetSession session, Exception e)
        {
            error = e.Message;
        }

        public void UnpackMsgs(Action<INetMsg> unpacker)
        {
            while (_receiveQueue != null && _receiveQueue.Count > 0) {
                var read = false;
                var nm = _receiveQueue.Dequeue();
                if (nm != null) {
                    //if (nm.type == NetSession.HEART_BEAT_MSG) {
                    //    var recalcLatency = (int)((recvTicks - _nowSession.sendTicks) / 10000);
                    //    LogMgr.D("<color=yellow>LATENCY: {0}({1}-{2}={3})</color>",
                    //        latency, _nowSession.sendTicks, recvTicks, recalcLatency);

                    //    if (recalcLatency >= 0 && recalcLatency < latency) _nowSession.latency = recalcLatency;
                    //}
                    unpacker.Invoke(nm);
                    nm.Recycle();
                }
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            //关闭连接
            try {
                if (_nowSession != null) {
                    _nowSession.Free();
                }
                _nowSession = null;
            } finally {
            }

            if (onDisconnected != null) {
                onDisconnected(this);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        public bool Send(INetMsg message)
        {
            if (_nowSession == null) return false;
            return _nowSession.Send(message);
        }

        public override string ToString()
        {
            return string.Format("{0}|{1}", base.ToString(),
                _nowSession != null && _nowSession.connected ? "Connected" : "Unconnected");
        }

        public static bool IsIPV6 { get; private set; }
        public static string RefreshAddressFamily(string host)
        {
            var IPs = Dns.GetHostAddresses(host);
            if (IPs != null && IPs.Length > 0) {
                IsIPV6 = IPs[0].AddressFamily == AddressFamily.InterNetworkV6;
                return IPs[0].ToString();
            }

            return null;
        }

    }
}
