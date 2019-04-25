using System;
using System.Collections.Generic;
using System.Text;

namespace clientlib.net
{
    public interface INetMsg
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        int type { get;}
        byte read();
        int readU32();
        long readU64();
        int[] readU32s();
        long[] readU64s();
        float readFloat();
        double readDouble();
        string readString();

        T readParser<T>() where T : INetMsgParser, new();
        T[] readParsers<T>() where T : INetMsgParser, new();

        INetMsg write(byte value);
        INetMsg writeU32(int value);
        INetMsg writeU64(long value);
        INetMsg writeString(string value);

        INetMsg writeFuller(INetMsgFuller fuller);
        INetMsg writeFuller(INetMsgFuller[] fuller);
    }
}
