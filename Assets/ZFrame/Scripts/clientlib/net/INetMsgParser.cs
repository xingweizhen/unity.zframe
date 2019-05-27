using System;
using System.Collections.Generic;
using System.Text;
using ZFrame.NetEngine;

namespace clientlib.net
{
    public interface INetMsgParser
    {
        void parseMsg(INetMsg msg);
    }
}
