using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using clientlib.net;
#if ULUA
using LuaInterface;
#else
using XLua;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif
using ILuaState = System.IntPtr;

namespace ZFrame.Lua
{
    using NetEngine;
    public static class LibNetwork
    {
        public const string LIB_NAME = "libnetwork.cs";

        public static INetMsg readNm, writeNm;

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int OpenLib(ILuaState lua)
        {
            lua.NewTable();

            lua.SetDict("GetLocalIPs", GetLocalIPs);
            lua.SetDict("RefreshAddressFamily", RefreshAddressFamily);
            lua.SetDict("HttpGet", HttpGet);
            lua.SetDict("HttpPost", HttpPost);
            lua.SetDict("HttpDownload", HttpDownload);
            lua.SetDict("GetTcpHandler", GetTcpHandler);

            lua.SetDict("SetParam", SetParam);

            lua.SetDict("ReadU32", ReadU32);
            lua.SetDict("ReadU64", ReadU64);
            lua.SetDict("ReadFloat", ReadFloat);
            lua.SetDict("ReadString", ReadString);
            lua.SetDict("ReadBuffer", ReadBuffer);

            lua.SetDict("WriteU32", WriteU32);
            lua.SetDict("WriteU64", WriteU64);
            lua.SetDict("WriteString", WriteString);
            lua.SetDict("WriteBuffer", WriteBuffer);

            lua.SetDict("NewNetMsg", NewNetMsg);
            lua.SetDict("SendNetMsg", SendNetMsg);
            lua.SetDict("UnpackMsg", UnpackNetMsg);

            return 1;
        }

        public static string KeyValue2Param<T>(IEnumerable<KeyValuePair<string, T>> enumrable) where T : System.IConvertible
        {
            var itor = enumrable.GetEnumerator();
            System.Text.StringBuilder strbld = new System.Text.StringBuilder();
            while (itor.MoveNext()) {
                string key = itor.Current.Key;
                string value = itor.Current.Value.ToString(null);
                strbld.AppendFormat("{0}={1}&", WWW.EscapeURL(key), WWW.EscapeURL(value));
            }

            return strbld.ToString();
        }

        public static string KeyValue2Param(ILuaState lua, int index)
        {
            lua.AbsIndex(ref index);

            System.Text.StringBuilder strbld = new System.Text.StringBuilder();
            lua.PushNil();
            while (lua.Next(index)) {
                var key = lua.ToString(-2);
                var value = lua.ToString(-1);
                strbld.AppendFormat("{0}={1}&", WWW.EscapeURL(key), WWW.EscapeURL(value));
                lua.Pop(1);
            }

            return strbld.ToString();
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int GetLocalIPs(ILuaState lua)
        {
            var nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            lua.NewTable();
            for (int i = 0; i < nics.Length; ++i) {
                var ni = nics[i];
                if (ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Loopback ||
                    ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Unknown) continue;

                var uniCast = ni.GetIPProperties().UnicastAddresses;
                int n = 0;
                foreach (var uni in uniCast) {
                    var addr = uni.Address;
                    lua.SetString(-1, ++n, addr.ToString());
                }
            }

            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int RefreshAddressFamily(ILuaState lua)
        {
            var host = lua.ToString(1);
            string ip = null;
            try {
                ip = NetClient.RefreshAddressFamily(host);
            } catch (System.Exception e) {
                LogMgr.W("{0}", e);
            }

            if (!string.IsNullOrEmpty(ip)) {
                lua.PushString(ip);
            } else {
                lua.PushNil();
            }

            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int HttpGet(ILuaState lua)
        {
            string tag = lua.ChkString(1);
            string url = lua.ChkString(2);
            float timeout = (float)lua.OptNumber(4, 10);
            string param = null;
            var luaT = lua.Type(3);
            if (luaT == LuaTypes.LUA_TSTRING) {
                param = lua.ToString(3);
            } else {
                param = KeyValue2Param(lua, 3);
            }

            var httpHandler = NetworkMgr.Instance.GetHttpHandler("HTTP");
            if (httpHandler) httpHandler.StartGet(tag, url, param, timeout);

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int HttpPost(ILuaState lua)
        {
            string tag = lua.ChkString(1);
            string url = lua.ChkString(2);
            string strHeader = lua.ToString(4);
            float timeout = (float)lua.OptNumber(5, 10);

            //测试日志，输出请求host
            Debug.Log(string.Format("tag:{0};url:{1};strHeader:{2}", tag, url, strHeader));

            string postData = null;
            var luaT = lua.Type(3);
            if (luaT == LuaTypes.LUA_TSTRING) {
                postData = lua.ToString(3);
            } else if (luaT == LuaTypes.LUA_TTABLE) {
                postData = KeyValue2Param(lua, 3);
            }

            Dictionary<string, string> headers = null;
            luaT = lua.Type(4);
            if (luaT == LuaTypes.LUA_TSTRING) {
                // "key:value\nkey:value"
                headers = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(strHeader)) {
                    string[] segs = strHeader.Split('\n');
                    foreach (string seg in segs) {
                        string[] kv = seg.Split(':');
                        if (kv.Length == 2) {
                            headers.Add(kv[0].Trim(), kv[1].Trim());
                        }
                    }
                }
            } else if (luaT == LuaTypes.LUA_TTABLE) {
                headers = new Dictionary<string, string>();
                lua.PushNil();
                while (lua.Next(4)) {
                    var key = lua.ToString(-2);
                    var value = lua.ToString(-1);
                    headers.Add(key, value);
                    lua.Pop(1);
                }
            }

            var httpHandler = NetworkMgr.Instance.GetHttpHandler("HTTP");
            if (httpHandler) httpHandler.StartPost(tag, url, System.Text.Encoding.UTF8.GetBytes(postData), headers, timeout);

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int HttpDownload(ILuaState lua)
        {
            string url = lua.ChkString(1);
            string path = lua.ChkString(2);
            var md5chk = lua.OptBoolean(3, false);
            float timeout = (float)lua.OptNumber(4, 10);

            var httpHandler = NetworkMgr.Instance.GetHttpHandler("HTTP-DL");
            if (httpHandler) httpHandler.StartDownload(url, path, md5chk, timeout);

            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int GetTcpHandler(ILuaState lua)
        {
            lua.PushLightUserData(NetworkMgr.Instance.GetTcpHandler(lua.ToString(1)));
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SetParam(ILuaState lua)
        {
            //ExceptionReporter.Instance.SetParam(lua.ToLuaString(1), lua.ToLuaString(2));
            //HockeyAppMgr.Instance.SetParam(lua.ToLuaString(1), lua.ToLuaString(2));
            return 0;
        }

        #region 网络消息处理

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int ReadU32(ILuaState lua)
        {
            lua.PushInteger(((NetMsg)readNm).readU32());
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int ReadU64(ILuaState lua)
        {
            lua.PushLong(((NetMsg)readNm).readU64());
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int ReadFloat(ILuaState lua)
        {
            lua.PushNumber(((NetMsg)readNm).readFloat());
            return 1;
        }


        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int ReadString(ILuaState lua)
        {
            lua.PushString(((NetMsg)readNm).readString());
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int ReadBuffer(ILuaState lua)
        {
            lua.PushBytes(readNm.data, readNm.bodySize);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int WriteU32(ILuaState lua)
        {
            ((NetMsg)writeNm).writeU32(lua.ToInteger(2));
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int WriteU64(ILuaState lua)
        {
            ((NetMsg)writeNm).writeU64(lua.ToLong(2));
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int WriteString(ILuaState lua)
        {
            ((NetMsg)writeNm).writeString(lua.ToString(2));
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int WriteBuffer(ILuaState lua)
        {
            int len;
            var ptr = lua.ToBufferPtr(2, out len);
            if(ptr != System.IntPtr.Zero) {
                writeNm.WriteBuffer(ptr, len);
            }
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int NewNetMsg(ILuaState lua)
        {
            if (NetClient.NetMsgCreator != null) {
                writeNm = NetClient.NetMsgCreator.Invoke(lua.ToInteger(1), lua.ToInteger(2));
            } else {
                LogMgr.E("未定义消息包创建器：NetworkMgr.NetMsgCreator == null");
            }
            return 0;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int SendNetMsg(ILuaState lua)
        {
            var nmSize = writeNm.size;
            var tcp = (TcpClientHandler)lua.ToComponent(1, typeof(TcpClientHandler));
            tcp.Send(writeNm);
            writeNm = null;

            lua.PushInteger(nmSize);
            return 1;
        }

        private enum MsgField
        {
            PACK,
            Int,
            Long,
            Float,
            String,
        }

        private static void UnpackField(ILuaState lua, MsgField field, int index)
        {
            switch (field) {
                case MsgField.Int:
                    lua.PushInteger(((NetMsg)readNm).readU32());
                    break;
                case MsgField.Long:
                    lua.PushLong(((NetMsg)readNm).readU32());
                    break;
                case MsgField.Float:
                    lua.PushNumber(((NetMsg)readNm).readFloat());
                    break;
                case MsgField.String:
                    lua.PushString(((NetMsg)readNm).readString());
                    break;
                default:
                    UnpackNetMsg(lua, index);
                    break;
            }
        }

        private static void UnpackNetMsg(ILuaState lua, int index)
        {
            lua.AbsIndex(ref index);

            lua.NewTable();
            var tbIdx = lua.GetTop();

            lua.PushNil();
            while (lua.Next(index)) {
                var arr = lua.GetBoolean(-1, "arr");

                lua.GetField(-1, "name"); // push name as key
                lua.GetField(-2, "content"); // push content
                var field = (MsgField)lua.Type(-1);

                if (arr) {
                    var n = ((NetMsg)readNm).readU32();
                    lua.CreateTable(n, 0);
                    for (var i = 0; i < n; ++i) {
                        lua.PushInteger(i + 1);
                        UnpackField(lua, field, -2);
                        lua.SetTable(-3);
                    }
                } else {
                    UnpackField(lua, field, -2);
                }
                // top is value

                lua.Pop(1); // pop content
                lua.SetTable(tbIdx);
                lua.Pop(1);
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        private static int UnpackNetMsg(ILuaState lua)
        {
            UnpackNetMsg(lua, 1);
            return 1;
        }

        #endregion
    }
}
