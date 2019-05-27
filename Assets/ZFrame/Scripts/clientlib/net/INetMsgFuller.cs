using System;
using System.Collections.Generic;
using System.Text;
using ZFrame.NetEngine;

namespace clientlib.net
{
    public interface INetMsgFuller
    {
        void fullMsg(INetMsg msg);
    }
}
