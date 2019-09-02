using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using UnityEngine;

namespace ZFrame.NetEngine
{
    public class HttpRequester : Poolable<HttpRequester>
    {
        public static int BYTE_LEN = 1024;

        public delegate void ProcessDelegate(HttpRequester httpReq, long current, long total);

        public delegate void RespDelegate(HttpRequester httpReq, string resp, System.Exception e);
                      
        public string reqUri { get; private set; }
        public string reqMethod { get; private set; }
        public string reqPara { get; private set; }
        public string rspFile { get; private set; }
        public long current { get; private set; }
        public long total { get; private set; }

        /// <summary>
        /// 下载完成的文件保存路径
        /// </summary>
        private string m_SavePath;
        public string savePath { get { return m_SavePath; } }

        private long m_Cache;
        private float m_StartTime;

        /// <summary>
        /// 检查超时和下载数据变化情况
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns>小于0：超时：timeout：数据有变化；>0等待中</returns>
        public float CheckTimeout(float timeout)
        {
            if (current == total) return 0;

            if (m_Cache != current) {
                m_Cache = current;
                m_StartTime = Time.realtimeSinceStartup;
                return timeout;
            }

            return m_StartTime + timeout - Time.realtimeSinceStartup;
        }

        /// <summary>
        /// 已完成或者空闲状态
        /// </summary>
        public bool isDone { get; private set; }

        public string error;
        public HttpStatusCode statusCode { get; private set; }

        long storageSiz = 0;

        private readonly object m_FileLock;
        public FileStream file { get; private set; }        
        public object param { get; private set; }        
        public HttpWebRequest wrq;
        public ProcessDelegate onProcess;
        public RespDelegate onResponse;
        public string md5 { get; private set; }

        public System.IAsyncResult result { get; private set; }

        public HttpRequester()
        {
            isDone = true;
            m_FileLock = this;
        }

        public void Start(string uri, string param, string method, string savePath = null,
            ProcessDelegate process = null, RespDelegate resp = null)
        {
            reqUri = uri;
            reqPara = param;
            reqMethod = method;
            rspFile = savePath;
            onProcess = process;
            onResponse = resp;
            storageSiz = 1024 * 1024 * 100;

            current = 0;
            total = 1;
            error = null;
            statusCode = 0;

            Log.Format(LogLevel.I, "{0} {1}?{2}", reqMethod, reqUri, reqPara);
            switch (reqMethod) {
                case "GET":
                    wrq = (HttpWebRequest)WebRequest.Create(reqUri + "?" + reqPara);
                    break;
                case "POST": {
                        wrq = (HttpWebRequest)WebRequest.Create(reqUri);
                        wrq.Method = "POST";
                        wrq.ContentType = "application/x-www-form-urlencoded";

                        if (reqPara != null) {
                            byte[] SomeBytes = Encoding.UTF8.GetBytes(reqPara);
                            Stream newStream = wrq.GetRequestStream();
                            newStream.Write(SomeBytes, 0, SomeBytes.Length);
                            newStream.Close();
                            wrq.ContentLength = reqPara.Length;
                        } else {
                            wrq.ContentLength = 0;
                        }

                        break;
                    }
                case "GETF": {
                        CloseFile();
                        if (wrq != null) wrq.Abort();

                        if (!File.Exists(rspFile)) {
                            SystemTools.NeedDirectory(Path.GetDirectoryName(rspFile));
                        }
                        try {
                            file = new FileStream(rspFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                            file.Seek(0, SeekOrigin.End);
                        } catch {
                            return;
                        }

                        wrq = (HttpWebRequest)WebRequest.Create(reqUri);
                        wrq.AddRange((int)file.Length);
                        total = file.Length;
                        break;
                    }

                default:
                    return;
            }

            isDone = false;
            result = wrq.BeginGetResponse(f_processHttpResponseAsync, wrq);
            m_StartTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// 开始下载
        /// </summary>
        /// <param name="url">要下载的文件的url</param>
        /// <param name="tmpPath">下载时缓存的地址</param>
        /// <param name="param">外部参数，值不为空时，会对文件进行MD5检查</param>
        /// <param name="savePath">下载完成后保存地址</param>
        public void Download(string url, string tmpPath, object param = null, string savePath = null)
        {
            this.param = param;
            this.m_SavePath = savePath;
            Start(url, null, "GETF", tmpPath, onProcess, onResponse);
        }

        public object TakeParam()
        {
            var ret = param;
            param = null;
            return ret;
        }

        public void Stop()
        {
            CloseFile();

            if (wrq != null) {
                wrq.Abort();
                wrq = null;
            }

            isDone = true;
        }

        public void Reset()
        {
            Stop();

            result = null;
            reqUri = null;
            reqPara = null;
            reqMethod = null;
            rspFile = null;
            onProcess = null;
            onResponse = null;
            param = null;
            current = 0;
            total = 0;
        }

        protected override void OnRelease()
        {
            Reset();
        }

        private bool CloseFile()
        {
            lock (m_FileLock) {
                if (file != null) {
                    Log.Format(LogLevel.I, "Close: {0}", file.Name);
                    file.Close();
                    file = null;
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 异步调用函数
        /// </summary>
        /// <param name="iar"></param>
        private void f_processHttpResponseAsync(System.IAsyncResult iar)
        {
            StringBuilder rsb = new StringBuilder();
            HttpWebRequest req = iar.AsyncState as HttpWebRequest;

            byte[] buffer = new byte[BYTE_LEN];
            System.Exception ex = null;
            try {
                HttpWebResponse response = req.EndGetResponse(iar) as HttpWebResponse;
                Stream responseStream = response.GetResponseStream();
                if (response.ContentLength / 1000 / 1000 > storageSiz)
                    throw new IOException(string.Format("Disk Full ({0} / {1})", response.ContentLength / 1000 / 1000,
                        storageSiz));

                total = response.ContentLength;
                lock (m_FileLock) if (file != null) total += file.Length;

                for (; current < total && req == wrq;) {
                    int count = responseStream.Read(buffer, 0, buffer.Length);
                    if (count > 0) {
                        if (wrq == null) {
                            throw new WebException("Request Canceled", WebExceptionStatus.RequestCanceled);
                        }
                        
                        lock (m_FileLock) {
                            if (file == null) {
                                string str = Encoding.UTF8.GetString(buffer);
                                rsb.Append(str);
                                current = rsb.Length;
                                if (onProcess != null) onProcess(this, rsb.Length, total);
                            } else {
                                file.Write(buffer, 0, count);
                                current = file.Length;
                                if (onProcess != null) onProcess(this, file.Length, total);
                            }
                        }
                    } else {
                        break;
                    }
                }

                lock (m_FileLock) {
                    if (file != null) {
                        var length = 0L;
                        length = file.Length;

                        if (length != total) {
                            throw new WebException("Request Unfinished", WebExceptionStatus.RequestCanceled);
                        }
                    }
                }

                if (responseStream != null) {
                    responseStream.Dispose();
                }

                response.Close();
            } catch (WebException e) {
                var resp = e.Response as HttpWebResponse;
                if (resp != null) {
                    statusCode = resp.StatusCode;
                    if (statusCode == HttpStatusCode.RequestedRangeNotSatisfiable) {
                        Log.Format(LogLevel.I, "File {0} is done.", rspFile);
                        rsb.Append(rspFile);
                        current = total;
                        return;
                    }
                    error = statusCode.ToString();
                } else {
                    statusCode = HttpStatusCode.NotFound;
                    error = e.Status.ToString();
                }

                ex = e;
            } catch (IOException e) {
                ex = e;
                error = "IOException: " + e.Message;
            } catch (System.Exception e) {
                ex = e;
                req.Abort();
                error = e.Message;
            } finally {
                lock (m_FileLock) {
                    if (ex == null && file != null && param != null) {
                        file.Seek(0, SeekOrigin.Begin);
                        md5 = StreamToMD5(file);
                    }
                }

                if (CloseFile() && ex == null && !string.IsNullOrEmpty(m_SavePath)) {
                    if (File.Exists(m_SavePath)) File.Delete(m_SavePath);
                    File.Move(rspFile, m_SavePath);
                }

                // GetResponse Success
                if (rsb.Length == 0) {
                    rsb.Append(rspFile);
                }

                isDone = true;

                if (onResponse != null) {
                    if (ex == null) {
                        onResponse(this, rsb.ToString(), null);
                    } else {
                        onResponse(this, null, ex);
                    }
                }
            }
        }

        private static string StreamToMD5(Stream stream)
        {
            using (var alg = System.Security.Cryptography.MD5.Create()) {
                var hashBytes = alg.ComputeHash(stream);
                var strbld = new StringBuilder(hashBytes.Length * 2);
                for (var i = 0; i < hashBytes.Length; ++i) {
                    strbld.Append(hashBytes[i].ToString("x2"));
                }
                return strbld.ToString();
            }
        }
    }
}