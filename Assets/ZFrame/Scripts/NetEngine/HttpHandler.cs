using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace ZFrame.NetEngine
{
    public delegate void DelegateHttpResponse(string tag, WWW resp, bool isDone, string error);
    public delegate void DelegateHttpDownload(string url, bool isDone, HttpRequester httpReq);

    public class HttpHandler : MonoBehaviour
    {
        public DelegateHttpResponse onHttpResp;
        public DelegateHttpDownload onHttpDL;
        private readonly List<HttpRequester> m_UsingHttp = new List<HttpRequester>();

        public HttpRequester Get()
        {
            var req = HttpRequester.Get();
            m_UsingHttp.Add(req);
            return req;
        }

        public void Release(HttpRequester req)
        {
            HttpRequester.Release(req);
            m_UsingHttp.Remove(req);
        }

        private void OnDestroy()
        {
            for (var i = 0; i < m_UsingHttp.Count; ++i) {
                m_UsingHttp[i].Stop();
            }
            m_UsingHttp.Clear();
        }

        private void HandleHttpResp(WWW www, string tag)
        {
            if (onHttpResp != null) {
                var isDone = www.isDone;
                if (isDone && www.error == null) {
                    onHttpResp.Invoke(tag, www, isDone, null);
                } else {
                    onHttpResp.Invoke(tag, www, isDone, www.error);
                }
            }
        }

        private IEnumerator CoroHttpGet(string tag, string uri, string param, float timeout)
        {
            float time = Time.realtimeSinceStartup + timeout;
            if (!string.IsNullOrEmpty(param)) {
                uri = uri + "?" + param;
            }

            LogMgr.I(this, "WWW Get: {0}", uri);
            using (WWW www = new WWW(uri)) {
                while (www.error == null && !www.isDone) {
                    if (time < Time.realtimeSinceStartup) {
                        break;
                    }
                    yield return null;
                }

                HandleHttpResp(www, tag);
            }
        }

        private IEnumerator CoroHttpPost(string tag, string uri, byte[] postData, Dictionary<string, string> headers, float timeout)
        {
            float time = Time.realtimeSinceStartup + timeout;

            LogMgr.I(this, "WWW Post: {0}\n{1}", uri, System.Text.Encoding.UTF8.GetString(postData));

            var www = headers != null ? new WWW(uri, postData, headers) : new WWW(uri, postData);
            using (www) {
                while (www.error == null && !www.isDone) {
                    if (time < Time.realtimeSinceStartup) {
                        break;
                    }
                    yield return null;
                }

                HandleHttpResp(www, tag);
            }
        }

        private IEnumerator CoroHttpDownload(string url, string savePath, bool md5chk, float timeout)
        {
            float time = Time.realtimeSinceStartup + timeout;
            long progress = 0;
            var httpReq = Get();
            httpReq.Download(url, savePath, md5chk ? this : null);
            for (; ; ) {
                yield return null;
                var isDone = httpReq.isDone;
                if (httpReq.error == null) {
                    float realtimeSinceStartup = Time.realtimeSinceStartup;
                    if (!isDone && httpReq.current == progress) {
                        if (time < realtimeSinceStartup) {
                            httpReq.error = HttpStatusCode.RequestTimeout.ToString();
                            httpReq.Stop();
                        } else {
                            continue;
                        }
                    } else {
                        time = realtimeSinceStartup + timeout;
                    }
                }

                if (onHttpDL != null) {
                    onHttpDL.Invoke(url, isDone, httpReq);
                }
                if (isDone) break;
                progress = httpReq.current;
            }
            Release(httpReq);
        }

        public void StartGet(string tag, string url, string param, float timeout)
        {
            StartCoroutine(CoroHttpGet(tag, url, param, timeout));
        }

        public void StartPost(string tag, string url, byte[] postData, Dictionary<string, string> headers, float timeout)
        {
            StartCoroutine(CoroHttpPost(tag, url, postData, headers, timeout));
        }

        public void StartDownload(string url, string savePath, bool md5chk, float timeout)
        {
            StartCoroutine(CoroHttpDownload(url, savePath, md5chk, timeout));
        }
    }
}
