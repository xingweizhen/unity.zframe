using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace clientlib.net
{
    /// <summary>
    /// 消息
    /// </summary>
    public class NetMsg : INetMsg
    {
        private const int HEAD_SIZE = 6;

        [DllImport(IoBuffer.DLL_NAME)]
        public static extern int getVer();

        [DllImport(IoBuffer.DLL_NAME)]
        private static extern int putHead(byte[] buf,byte sign, int len, int flag);

        private static int sendNum = new Random().Next(256);

        private static List<NetMsg> m_Pool = new List<NetMsg>();
        private static int m_CountAll;
        public static string GetPoolInfo()
        {
            return string.Format("Nm Used: {0}/{1}", m_CountAll - m_Pool.Count, m_CountAll);
        }

        public static void Release(NetMsg nm)
        {
            lock (m_Pool) {
#if UNITY_EDITOR
                foreach (var elm in m_Pool) {
                    if (ReferenceEquals(elm, nm)) {
                        UnityEngine.Debug.LogErrorFormat(
                            "Internal error. Trying to destroy {0}({1}) that is already released to pool.", 
                            nm, nm.GetHashCode());
                    }
                }
#endif
                var length = nm._buffer.array.Length;
                for (int i = 0; i < m_Pool.Count; ++i) {
                    if (length <= m_Pool[i]._buffer.array.Length) {
                        m_Pool.Insert(i, nm);
                        return;
                    }
                }

                m_Pool.Add(nm);
            }
        }

        public static NetMsg Get(int size)
        {
            lock (m_Pool) {
                for (int i = 0; i < m_Pool.Count; ++i) {
                    var nm = m_Pool[i];
                    if (size <= nm._buffer.array.Length) {
                        m_Pool.RemoveAt(i);
                        return nm;
                    }
                }
            }

            m_CountAll += 1;
            return new NetMsg();
        }

        /// <summary>
        /// 接收消息初始化
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static NetMsg createReadMsg(int size)
        {
            //NetMsg msg = new NetMsg();
            var msg = Get(size);
            msg.type = 0;
            msg.msgSize = size;
            msg.initRead(size);
            return msg;
        }

        /// <summary>
        /// 创建发送消息
        /// </summary>
        /// <returns></returns>
        public static NetMsg createMsg(int type)
        {
            return createMsg(type, IoBuffer.BLOCK_SIZE);
        }

        /// <summary>
        /// 创建发送消息（自定义缓冲区大小）
        /// </summary>
        /// <returns></returns>
        public static NetMsg createMsg(int type, int size)
        {
            //NetMsg msg = new NetMsg();
            var msg = Get(size);
            msg.type = type;
            msg.initWrite(size);
            return msg;
        }

        /// <summary>
        /// 读取消息长度，临时变量
        /// </summary>
        public int msgSize;
        public int readSize { get { return msgSize; } }
        public short writeSize { get { return (short)_buffer.position; } }

        private int _type;
        private IoBuffer _buffer;


        /// <summary>
        /// 默认的消息，缓冲区长度1024
        /// </summary>
        private NetMsg()
        {
        }

        private void init(int size)
        {
            int n = size / IoBuffer.BLOCK_SIZE + ((size % IoBuffer.BLOCK_SIZE > 0) ? 1 : 0);
            var buffSize = n * IoBuffer.BLOCK_SIZE;
            if (_buffer == null || _buffer.array.Length < buffSize) {
                _buffer = new IoBuffer(buffSize);
            } else {
                _buffer.Initialize(buffSize);
            }
        }

        private void initRead(int size)
        {
            init(size);
        }

        private void initWrite(int size)
        {
            if (_type < 1) throw new Exception("msg type mast > 0!");
            init(size);

            _buffer.position = HEAD_SIZE;
            writeU32(_type);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        public void deserialization()
        {
            _buffer.flip();
            _type = readU32();
        }

        public void reset(int type = 0)
        {
            _buffer.position = 0;
            if (type > 0) {
                _type = type;                
            }
            writeU32(_type);
        }

        /// <summary>
        /// 序列化
        /// </summary>
        public void serialization(byte sign)
        {
            _buffer.flip();

            int len = _buffer.limit - HEAD_SIZE;
            int offset = putHead(_buffer.array,sign, len, sendNum++);
            if (offset > 0)
            {
                _buffer.position = offset;
            }
        }

        public int type { 
            get{
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        public byte read()
        {
            return _buffer.read();
        }

        public int readU32()
        {
            return _buffer.readU32();
        }
        public int[] readU32s()
        {
            int n = _buffer.readU32();
            if(n<1)return new int[0];
            int[] ret = new int[n];
            for (int i = 0; i < n; i++)
            {
                ret[i] = _buffer.readU32();
            }
            return ret;
        }
        
        public long[] readU64s()
        {
            int n = _buffer.readU32();
            if (n < 1) return new long[0];
            long[] ret = new long[n];
            for (int i = 0; i < n; i++)
            {
                ret[i] = _buffer.readU64();
            }
            return ret;
        }

        public long readU64()
        {
            return _buffer.readU64();
        }

        public double readDouble()
        {
            return _buffer.readDouble();
        }

        public float readFloat()
        {
            return _buffer.readFloat();
        }

        public String readString()
        {
            return _buffer.readString();
        }

        public T readParser<T>() where T : INetMsgParser,new()
        {
            T t = new T();
            t.parseMsg(this);
            return t;
        }
        public T[] readParsers<T>() where T : INetMsgParser, new()
        {
            int n = this.readU32();
            T[] ret = (T[])Array.CreateInstance(typeof(T), n);
            for (int i = 0; i < n; i++)
            {
                ret[i] = new T();
                ret[i].parseMsg(this);
            }
            return ret;
        }

        public INetMsg writeFuller(INetMsgFuller fuller)
        {
            fuller.fullMsg(this);
            return this;
        }

        public INetMsg writeFuller(INetMsgFuller[] fuller)
        {
            if (fuller == null || fuller.Length < 1)
            {
                this.writeU32(0);
                return this;
            }
            this.writeU32(fuller.Length);
            foreach (INetMsgFuller f in fuller)
            {
                f.fullMsg(this);
            }
            return this;
        }

        public INetMsg write(byte value)
        {
            _buffer.write(value);
            return this;
        }

        public INetMsg write(byte[] buffer, int offset, int length)
        {
            _buffer.write(buffer, offset, length);
            return this;
        }

        public INetMsg writeU32(int value)
        {
            _buffer.writeU32(value);
            return this;
        }

        public INetMsg writeU64(long value)
        {
            _buffer.writeU64(value);
            return this;
        }
        
        public INetMsg writeString(String value)
        {
            _buffer.writeString(value);
            return this;
        }

        public IoBuffer buffer
        {
            get{
                return _buffer;
            }
        }

        public int limit
        {
            get
            {
                return _buffer.limit;
            }
        }

        public int posession
        {

            get
            {
                return _buffer.position;
            }
        }
    }
}
