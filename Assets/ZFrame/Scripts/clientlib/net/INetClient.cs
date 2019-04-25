using System;
using System.Collections.Generic;
using System.Text;

namespace clientlib.net
{
    public interface INetClient
    {
        bool send(INetMsg message);
        bool Connected { get; }
    }
}
