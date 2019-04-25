using System;
using System.Collections.Generic;
using System.Text;

namespace clientlib.net
{
    public interface INetMsgFuller
    {
        void fullMsg(INetMsg msg);
    }
}
