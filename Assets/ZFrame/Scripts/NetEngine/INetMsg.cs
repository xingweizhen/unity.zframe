
namespace ZFrame.NetEngine
{
    public interface INetMsg
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        int type { get; }

        /// <summary>
        /// 数据缓冲区
        /// </summary>
        byte[] data { get; }

        /// <summary>
        /// 包大小
        /// </summary>
        int size { get; }

        /// <summary>
        /// 消息大小
        /// </summary>
        int bodySize { get; }

        int ReadBuffer(byte[] buffer, int offset, int length);
        bool IsComplete();
        INetMsg WriteBuffer(byte[] buffer, int offset, int length);
        INetMsg WriteBuffer(System.IntPtr ptr, int len);

        /// <summary>
        /// 序列化：在发送前执行
        /// </summary>
        void Serialize();

        /// <summary>
        /// 反序列化：在接收完成后执行
        /// </summary>
        void Deserialize();

        /// <summary>
        /// 回收：对象池管理
        /// </summary>
        void Recycle();
    }

    public interface INetMsgHandler
    {
        /// <summary>
        /// 尝试解析消息
        /// </summary>
        /// <param name="cli"></param>
        /// <param name="nm"></param>
        /// <returns>返回真表示已解析</returns>
        bool TryHandle(INetMsg nm);
    }
}
