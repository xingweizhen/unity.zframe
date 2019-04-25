using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using clientlib.net;
#if ULUA
using LuaInterface;
#else
using XLua;
#endif
using ILuaState = System.IntPtr;
#if UNITY_5
using Assert = UnityEngine.Assertions.Assert;
#else
using Assert = ZFrame.Assertions.Assert;
#endif

namespace ZFrame.NetEngine
{
    public class NetworkMgr : MonoSingleton<NetworkMgr>
    {
        [SerializeField]
        private string luaScript = null;
        // TCP
        [SerializeField]
        private string onInit = null;
        // HTTP
        [SerializeField]
        private string onHttpRes = null, onHttpDownload = null;

        public string knownHost = "www.qq.com";
        
        [Description("消息池子")]
        public string netMsgPool { get { return NetMsg.GetPoolInfo(); } }
                
        public static NetworkMgr Inst { get { return Instance; } }

        private LuaTable m_Tb;

        [BlackList]
        public static void Log(string fmt, params object[] Args)
        {
            LogMgr.I("[NW] " + fmt, Args);
        }

		private void Start()
		{
            var L = LuaScriptMgr.Instance.L;
            L.GetGlobal(LuaComponent.PKG, "network/msgdef");
            NetSession.HEART_BEAT_MSG = (int)L.GetNumber(-1, "COM.CS.KEEP_HEART");
            L.Pop(1);

            L.GetGlobal(LuaComponent.PKG, luaScript);
            
            m_Tb = L.ToLuaTable(-1);
            L.Pop(1);
            Assert.IsNotNull(m_Tb);
            
            m_Tb.CallFunc(0, onInit, this);
		}

        private void OnHttpResponse(string tag, WWW resp, bool isDone, string error)
        {            
            var lua = m_Tb.PushField(onHttpRes);
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
            var lua = m_Tb.PushField(onHttpDownload);
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
