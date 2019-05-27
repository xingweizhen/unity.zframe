using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using clientlib.net;

namespace ZFrame.NetEngine
{
    public delegate void NetCallback(INetSession session);

    public interface INetSessionEvent
    {
        void OnConnected(INetSession session);
        void OnMessageRead(INetSession session, INetMsg msg);
        void OnMessageWrite(INetSession session, INetMsg msg);
        void OnException(INetSession session, System.Exception e);
    }

    public interface INetSession
    {
        bool connecting { get; }
        bool connected { get; }
        System.Exception lastError { get; }

        int latency { get; }

        System.Action<System.Exception> onException { get; set; }
        INetMsg msg { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="onConnected"></param>
        /// <param name="onReadMsg">消息已读完</param>
        /// <param name="onWriteMsg">消息已写入</param>
        /// <param name="addressFamily"></param>
        void Connect(string host, int port, AddressFamily addressFamily, INetSessionEvent sessionEvent);
        bool Send(INetMsg msg);
        void Free();
    }

}
