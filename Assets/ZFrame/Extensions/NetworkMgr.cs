using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
//using clientlib.net;
#if ULUA
using LuaInterface;
#else
using XLua;
#endif
using ILuaState = System.IntPtr;
#if UNITY_5_5_OR_NEWER
using Assert = UnityEngine.Assertions.Assert;
#else
using Assert = ZFrame.Assertions.Assert;
#endif

namespace ZFrame.NetEngine
{
    using Lua;

    public class NetworkMgr : MonoSingleton<NetworkMgr>
    {      
        // TCP
        private const string F_NC_INIT = "on_nc_init";
        // HTTP
        private const string F_HTTP_RSP = "on_http_response";
        private const string F_HTTP_DOWNLOAD = "on_http_download";

        private const string luaScript = "framework/networkmgr";

        public string knownHost = "www.qq.com";
                
        private LuaTable m_Tb;

        public static void Log(string fmt, params object[] Args)
        {
            LogMgr.I("[NW] " + fmt, Args);
        }

		private void Start()
		{
            var L = LuaScriptMgr.Instance.L;
            //L.GetGlobal(LuaComponent.PKG, "network/msgdef");
            //NetSession.HEART_BEAT_MSG = (int)L.GetNumber(-1, "COM.CS.KEEP_HEART");
            //L.Pop(1);

            L.GetGlobal(LuaComponent.PKG, luaScript);

            m_Tb = L.ToLuaTable(-1);
            L.Pop(1);
            Assert.IsNotNull(m_Tb);

            m_Tb.CallFunc(0, F_NC_INIT, this);
        }

        private void OnHttpResponse(string tag, WWW resp, bool isDone, string error)
        {            
            var lua = m_Tb.PushField(F_HTTP_RSP);
            int errFunc = lua.BeginPCall();
            lua.PushString(resp.url);
            lua.PushString(tag);
            lua.PushString(isDone ? resp.text : string.Empty);
            lua.PushBoolean(isDone);
            lua.PushString(error);
            lua.ExecPCall(5, 0, errFunc);
        }

        private void OnHttpDownload(string url, bool isDone, HttpRequester httpReq)
        {
            var lua = m_Tb.PushField(F_HTTP_DOWNLOAD);
            int errFunc = lua.BeginPCall();
            lua.PushString(url);            
            lua.PushLong(httpReq.current);
            lua.PushLong(httpReq.total);
            if (isDone && httpReq.param != null) {
                lua.PushString(httpReq.md5);
            } else {
                lua.PushBoolean(isDone);
            }
            lua.PushString(httpReq.error);
            lua.ExecPCall(5, 0, errFunc);
        }
        
        public TcpClientHandler GetTcpHandler(string tcpName)
        {
            TcpClientHandler tcpHandler = null;
            var trans = transform.Find(tcpName);
            if (!trans) {
                var go = GoTools.NewChild(gameObject);
                go.name = tcpName;
                tcpHandler = go.AddComponent<TcpClientHandler>();
            } else {
                tcpHandler = trans.GetComponent<TcpClientHandler>();
            }
            return tcpHandler;
        }

        public HttpHandler GetHttpHandler(string httpName)
        {
            HttpHandler httpHandler = null;
            var trans = transform.Find(httpName);
            if (!trans) {
                var go = GoTools.NewChild(gameObject);
                go.name = httpName;
                httpHandler = go.AddComponent<HttpHandler>();
                httpHandler.onHttpResp = OnHttpResponse;
                httpHandler.onHttpDL = OnHttpDownload;
            } else {
                httpHandler = trans.GetComponent<HttpHandler>();
            }
            return httpHandler;
        }
    }
}
