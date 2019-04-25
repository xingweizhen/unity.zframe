using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace clientlib.net
{
    public delegate void NetCallback(NetSession session);
    /// <summary>
    /// Session
    /// </summary>
    public class NetSession
    {
        public static int HEART_BEAT_MSG = 0;

        [DllImport(IoBuffer.DLL_NAME)]
        private static extern byte readHead(byte[] buf,out int len);
        [DllImport(IoBuffer.DLL_NAME)]
        private static extern int readLen(byte[] buf, byte ver);
        [DllImport(IoBuffer.DLL_NAME)]
        private static extern int putHands(byte[] buf, int len);

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

        private const int READ_CON = 0;
        private const int READ_HANDS = 1;
        private const int READ_WAIT = 2;
        private const int READ_HEAD = 3;
        private const int READ_LEN = 4;
        private const int READ_BODY = 5;

        public byte ver;
        public byte sign;
        public NetMsg msg;

        private TcpClient _tcp;
        private IoBuffer _headBuf;

        private NetCallback _connectFunc;
        private NetCallback _readFunc;


        private bool _isFree = false;
        private Exception _lastErr;
        private int _waitReadSize;
        private int _readSize;
        private int _nowAction;
        private bool _isHands = false;

        public System.Action<Exception> onException;

        public long sendTicks { get; private set; }
        public int latency;

        public NetSession(){
            _headBuf = new IoBuffer(32);
        }

        /// <summary>
        /// 最新的错误信息
        /// </summary>
        public Exception LastErr
        {
            get { return _lastErr; }
        }

        public bool isConnected
        {
            get
            {
                return _tcp != null && _tcp.Connected && _isHands;
            }
        }

        public bool connecting { get; private set; }

        /// <summary>
        /// 连接到某个网络地址
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="connectFunc"></param>
        /// <param name="readFunc"></param>
        /// <param name="addressFamily"></param>
        public void Connect(String host, int port, NetCallback connectFunc, NetCallback readFunc, 
            AddressFamily addressFamily = AddressFamily.InterNetwork)
        {
            if (_isFree) throw new Exception("netSession is free!");
            if (String.IsNullOrEmpty(host) || port < 1)
            {
                throw new Exception("error host or port!");
            }

            _connectFunc = connectFunc;
            _readFunc = readFunc;

            if (_tcp != null)
            {
                _tcp.Close();
                _tcp = null;
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

                _tcp = new TcpClient(addressFamily) { NoDelay = nodelay };
                _tcp.Client.NoDelay = nodelay;
                _tcp.BeginConnect(host, port, TcpConnectCallback, this);
                connecting = true;
            } catch (Exception ex) {
                OnException(ex);
            }
            
        }

        public void BeginReadMsg()
        {
            if (_isFree) throw new Exception("netSession is free!");

            DoRead(null);
        }


        /// <summary>
        /// 连接完成处理
        /// </summary>
        /// <param name="ar"></param>
        private void TcpConnectCallback(IAsyncResult ar)
        {
            connecting = false;
            if (_isFree) return;
            try {
                _tcp.EndConnect(ar);

                //发送握手协议
                Random rand = new Random();
                byte[] bHands = new byte[1024];
                for(int i = 0; i < 12; i++)
                {
                    bHands[i] = (byte)rand.Next();
                }
                putHands(bHands, 12);
                send(bHands, 0, 12);

                //读取握手返回
                DoRead(null);
            } catch (Exception ex) {
                OnException(ex);
            }
        }

        /// <summary>
        /// 同步完成链接回掉
        /// </summary>
        private void syncConnected()
        {
            if (_isFree) return;
            try
            {
                _connectFunc(this);
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        /// <summary>
        /// 有错误发生
        /// </summary>
        /// <param name="ex"></param>
        private void OnException(Exception ex)
        {
            _lastErr = ex;
            if (onException != null) onException.Invoke(ex);
            if (msg != null) {
                NetMsg.Release(msg);
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
            if (_readFunc != null)
            {
                _readFunc(this);
            }
        }

        private void DoRead(IAsyncResult ar)
        {
            if (_isFree) return;
            if (ar == null)
            {
                if (_isHands)
                {
                    //初始化消息读取，从头开始读取
                    _nowAction = READ_WAIT;
                }
                else
                {
                    //需要读取握手信息
                    _nowAction = READ_CON;
                }
            }

            int len = 0;
            switch (_nowAction)
            {
                case READ_CON:
                    //刚链接
                    _nowAction = READ_HANDS;
                    AsyncRead(_headBuf, 0, 4);
                    break;
                case READ_HANDS:
                    //处理握手协议
                    sign = _headBuf.array[2];
                    _isHands = true;

                    //完成握手，通知外部，完成链接
                    this.syncConnected();
                    break;
                case READ_WAIT:
                    //读取2个字节的版本信息
                    _nowAction = READ_HEAD;
                    AsyncRead(_headBuf, 0,2);
                    break;
                case READ_HEAD:
                    //解析版本信息，并读取消息长度
                    ver = readHead(_headBuf.array,out len);
                    if (ver == 0)
                    {
                        OnException(new Exception("error msg ver!"));
                        OnClosed();
                        return;
                    }
                    _nowAction = READ_LEN;
                    AsyncRead(_headBuf, 0,len);
                    break;
                case READ_LEN:
                    len = readLen(_headBuf.array,ver);
                    //创建消息
                    msg = NetMsg.createReadMsg(len);
                    _nowAction = READ_BODY;
                    AsyncRead(msg.buffer, 0, len);
                    break;
                case READ_BODY:
                    //完成消息读取
                    _readFunc(this);                    
                    //开始下一个消息读取
                    DoRead(null);
                    break;
            }
        }


        /// <summary>
        /// 读取指定字节数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        private void AsyncRead(IoBuffer buffer, int pos, int size)
        {
            if (_isFree) return;
            _readSize = pos;
            _waitReadSize = size;

            try
            {
                NetworkStream stream = _tcp.GetStream();
                stream.BeginRead(buffer.array, _readSize, _waitReadSize - _readSize, new AsyncCallback(DoAsyncRead), buffer);
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }

        private void DoAsyncRead(IAsyncResult ar)
        {
            try
            {
                IoBuffer buffer = (IoBuffer)ar.AsyncState;
                NetworkStream stream = _tcp.GetStream();
                int len = stream.EndRead(ar);
                if (len < 1)
                {
                    //网络断开
                    OnException(new Exception("disconnected!"));
                    return;
                }
                _readSize += len;

                if (_readSize < _waitReadSize)
                {
                    //不足，需要继续读取
                    stream.BeginRead(buffer.array, _readSize, _waitReadSize - _readSize, new AsyncCallback(DoAsyncRead), buffer);
                    return;
                }
                else if (_readSize > _waitReadSize)
                {
                    //粘包处理
                    System.Console.WriteLine("out read wait:{0},read:{1}", _waitReadSize, _readSize);
                }

                //循环读取
                DoRead(ar);
            }
            catch (Exception ex)
            {
                OnException(ex);
            }
        }


        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public bool send(INetMsg message)
        {
            if (!isConnected)
            {
                return false;
            }
            NetMsg msg = (NetMsg)message;
            if (msg == null) return false;

            //序列化
            msg.serialization(sign);

            NetworkStream stream = _tcp.GetStream();
            if (stream.CanWrite)
            {
                try
                {
                    stream.BeginWrite(msg.buffer.array, msg.buffer.position, msg.buffer.limit - msg.buffer.position, new AsyncCallback(AsyncWrite), msg);
                }
                catch (Exception ex)
                {
                    NetMsg.Release(msg);
                    OnException(ex);
                    return false;
                }
            }

            return true;
        }

        public bool send(byte[] buffer,int offset,int len)
        {
            NetworkStream stream = _tcp.GetStream();
            if (stream.CanWrite)
            {
                try
                {
                    stream.BeginWrite(buffer, offset, len, new AsyncCallback(AsyncWrite), null);
                }
                catch (Exception ex)
                {
                    OnException(ex);
                    return false;
                }
            }
            return true;
        }

        private void AsyncWrite(IAsyncResult ar)
        {
            try
            {
                NetworkStream stream = _tcp.GetStream();
                stream.EndWrite(ar);
                var nm = ar.AsyncState as INetMsg;
                if (nm != null && nm.type == HEART_BEAT_MSG) {
                    sendTicks = DateTime.Now.Ticks;
                }
            }
            catch (System.Exception ex)
            {
                OnException(ex);
            }

            var msg = ar.AsyncState as NetMsg;
            if (msg != null) NetMsg.Release(msg);
        }

        public void Free()
        {
            if (_tcp != null)
            {
                _tcp.Close();
                _tcp = null;
            }
            _isFree = true;
        }
    }
}
