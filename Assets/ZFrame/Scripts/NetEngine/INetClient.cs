using System;
using System.Collections.Generic;
using System.Text;

namespace ZFrame.NetEngine
{
    public interface INetClient
    {
        bool Send(INetMsg message);
        bool Connected { get; }
    }
}
