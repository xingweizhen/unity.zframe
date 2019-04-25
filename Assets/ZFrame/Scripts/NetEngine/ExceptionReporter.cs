using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.NetEngine
{
    public class ExceptionReporter : MonoSingleton<ExceptionReporter>
    {
#if false
        [SerializeField]
        private string reportUri = null;
        [SerializeField]
        private Util.TimeCounter m_ErrChkCounter;

        private Dictionary<string, string> m_ReportParams = new Dictionary<string, string>();
        private Queue<string> m_ErrorTrace = new Queue<string>();

        /// <summary>
        /// Unity日志回调，用于捕获异常和错误
        /// </summary>
        private void CatchingException(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error) {

                string errorStr = condition + "\n" + stackTrace;
                if (!m_ErrorTrace.Contains(errorStr)) {
                    m_ErrorTrace.Enqueue(errorStr);
                }
            }
        }

        /// <summary>
        /// 向服务器发送异常和错误报告
        /// </summary>
        private void ReportException()
        {
            if (m_ErrorTrace.Count == 0) return;

            string param = string.Format("{0}{1}={2}",
                   LibNetwork.KeyValue2Param(m_ReportParams),
                   "error", WWW.EscapeURL(m_ErrorTrace.Dequeue()));

#if !UNITY_EDITOR
            var httpHandler = NetworkMgr.Instance.GetHttpHandler("HTTP");
            if (httpHandler) {
                httpHandler.StartPost("report", reportUri, System.Text.Encoding.UTF8.GetBytes(param), null, 60);
            }
#else
            LogMgr.D("[Report] {0}?{1}", reportUri, param);
#endif
        }

        protected override void Awaking()
        {
            Application.logMessageReceived += CatchingException;
        }

        private void Update()
        {
            if (m_ErrChkCounter.Count(Time.unscaledDeltaTime)) {
                ReportException();
            }
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= CatchingException;
        }
        
        public void SetParam(string key, string value)
        {
            if (m_ReportParams.ContainsKey(key)) {
                m_ReportParams[key] = value;
            } else {
                m_ReportParams.Add(key, value);
            }
        }
#endif
    }
}
