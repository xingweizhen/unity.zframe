using System;
using System.Collections.Generic;
using System.Text;

namespace clientlib.net
{
    public interface INetMsgParser
    {
        void parseMsg(INetMsg msg);
    }
}
