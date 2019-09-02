using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using UnityEngine;

namespace ZFrame.Asset
{
    using NetEngine;

    public class AssetDownload : MonoSingleton<AssetDownload>
    {
        public event System.Action<string> onDownloadStart;
        public event System.Action<string, string> onDownloadError;
        public event System.Action<string, string> onVerifyFailed;
        public event System.Action<string, long> onDownloaded;        
        public event System.Action<string, long, long> onDownloading;
        public bool downloadTracking;

        [SerializeField] private int m_DownloaderCount = 3; 
        [SerializeField] private float m_Timeout = 10;
        [SerializeField] private float m_RetryInterval = 0.5f;

        [System.NonSerialized, Description("下载地址")]
        public string baseUrl;
        [System.NonSerialized, Description("保存地址")]
        public string savePath;

        private HttpRequester[] m_Downloaders;
        private readonly List<AsyncLoadingTask> m_Tasks = new List<AsyncLoadingTask>();

#if UNITY_EDITOR
        public IEnumerator<KeyValuePair<string, Vector2Int>> ForEachDownload()
        {
            if (m_Downloaders == null) yield break;
            
            foreach (var dl in m_Downloaders) {
                if (!dl.isDone) {
                    yield return new KeyValuePair<string, Vector2Int>(
                        dl.param.ToString(), new Vector2Int((int)dl.current, (int)dl.total));
                }
            }
        }
#endif

        protected override void Awaking()
        {
            base.Awaking();
            m_Downloaders = new HttpRequester[m_DownloaderCount];
            for (int i = 0; i < m_Downloaders.Length; i++) {
                m_Downloaders[i] = new HttpRequester();
            }
        }

        protected override void Destroying()
        {
            base.Destroying();

            for (var i = 0; i < m_Downloaders.Length; ++i) {
                m_Downloaders[i].Reset();
            }

        }

        private HttpRequester FindDownloader(string bundleName)
        {
            for (var i = 0; i < m_Downloaders.Length; ++i) {
                var task = m_Downloaders[i].param as AsyncLoadingTask;
                if (task != null && string.CompareOrdinal(task.bundleName, bundleName) == 0) {
                    return m_Downloaders[i];
                }
            }

            return null;
        }

        private AsyncLoadingTask FindTasking(string bundleName)
        {
            for (var i = 0; i < m_Tasks.Count; ++i) {
                var task = m_Tasks[i];
                if (task != null && string.CompareOrdinal(task.bundleName, bundleName) == 0) {
                    return task;
                }
            }

            return null;
        }

        private void CancelTasks(string bundleName)
        {
            for (var i = 0; i < m_Tasks.Count; ++i) {
                if (m_Tasks[i].bundleName == bundleName) {
                    AsyncLoadingTask.Cancel(m_Tasks[i]);
                    m_Tasks[i] = null;
                }
            }
            m_Tasks.RemoveNull();
        }

        private bool DownloadNextAsset(HttpRequester dl)
        {
            AsyncLoadingTask task = null;
            for (var i= 0; i < m_Tasks.Count; ++i) {
                var bundleName = m_Tasks[i].bundleName;                
                if (FindDownloader(bundleName) != null) continue;

                if (m_Tasks[i].hangUntilTime < Time.realtimeSinceStartup) {
                    task = m_Tasks[i];
                    m_Tasks.RemoveAt(i);
                    break;
                }
            }

            if (task != null) {
                UnityEngine.Profiling.Profiler.BeginSample("StartDownload");
                dl.Download(string.Format("{0}{1}?md5={2}", baseUrl, task.bundleName, task.bundleId), 
                    savePath + task.bundleName + ".tmp", task, savePath + task.bundleName);
                UnityEngine.Profiling.Profiler.EndSample();

                Log2File(LogLevel.D, "资源下载开始: {0}", task);
                if (onDownloadStart != null) onDownloadStart.Invoke(task.bundleName);
                return true;
            }

            return false;
        }

        private void OnTaskDownloaded(AsyncLoadingTask task)
        {
            if (task.loadType == LoadType.DownloadAndLoad) {
                AssetLoader.Instance.ScheduleTask(task);
            } else if (task.loadType == LoadType.Download) {
                task.loadType = LoadType.Load;
            } else {
                AsyncLoadingTask.Release(task);
            }
        }

        private void Update()
        {
            for (int i = 0; i < m_Downloaders.Length; ++i) {
                var dl = m_Downloaders[i];
                var task = dl.param as AsyncLoadingTask;
                if (task == null) {
                    UnityEngine.Profiling.Profiler.BeginSample("PickingTask");
                    dl.Reset(); DownloadNextAsset(dl);
                    UnityEngine.Profiling.Profiler.EndSample();
                    continue;
                }

                if (dl.isDone) {
                    dl.TakeParam();
                    var bundleName = task.bundleName;                    
                    if (dl.error == null) {
                        task.downloadProgress = 1f;
                        if (AssetBundleLoader.I.AddBundle(bundleName, dl.md5)) {
                            UnityEngine.Profiling.Profiler.BeginSample("OnAssetDownloaded");
                            if (onDownloaded != null) onDownloaded.Invoke(bundleName, dl.total);
                            UnityEngine.Profiling.Profiler.EndSample();

                            UnityEngine.Profiling.Profiler.BeginSample("OnTaskDownloaded");
                            OnTaskDownloaded(task);
                            UnityEngine.Profiling.Profiler.EndSample();

                            for (var n = 0; n < m_Tasks.Count; ++n) {
                                if (string.CompareOrdinal(m_Tasks[n].bundleName, bundleName) == 0) {
                                    OnTaskDownloaded(m_Tasks[n]);
                                    m_Tasks[n] = null;
                                }
                            }
                            m_Tasks.RemoveNull();
                        } else {
                            // 文件校验异常，删除重新下
                            File.Delete(dl.savePath);
                            task.hangUntilTime = Time.realtimeSinceStartup + m_RetryInterval;
                            m_Tasks.Insert(0, task);
                            if (onVerifyFailed != null) onVerifyFailed.Invoke(bundleName, dl.md5);
                        }
                    } else {
                        // 下载出错，任务放回继续下？
                        if (dl.statusCode != System.Net.HttpStatusCode.NotFound) {
                            task.hangUntilTime = Time.realtimeSinceStartup + m_RetryInterval;
                            m_Tasks.Insert(0, task);
                        }
                        Log2File(LogLevel.W, "资源下载失败: {0} {1}", dl.reqUri, dl.statusCode);
                        if (onDownloadError != null) onDownloadError.Invoke(bundleName, dl.error);
                    }
                } else {
                    var timeout = dl.CheckTimeout(m_Timeout);
                    if (timeout < 0) {
                        Log2File(LogLevel.W, "资源下载超时: {0}", dl.reqUri);
                        dl.TakeParam();
                        // 超时了把任务重新排队
                        m_Tasks.Insert(0, task);
                        if (onDownloadError != null) onDownloadError.Invoke(task.bundleName, "timeout>" + m_Timeout);
                    } else if (timeout == m_Timeout) {
                        // 正在正常下载中...
                        task.downloadProgress = (float)dl.current / dl.total;
                        if (downloadTracking && onDownloading != null) {
                            onDownloading.Invoke(task.bundleName, dl.current, dl.total);
                        }
                    }
                }
            }
        }

        public float GetProgress(string bundleName)
        {
            var dl = FindDownloader(bundleName);
            if (dl != null) return ((AsyncLoadingTask)dl.param).downloadProgress;

            return -1;            
        }

        public bool Download(AsyncLoadingTask task)
        {
            if (task.loadType == LoadType.Load) {
                if (FindDownloader(task.bundleName) != null || FindTasking(task.bundleName) != null) {
                    AsyncLoadingTask.Release(task);
                    return false;
                }
                Log2File(LogLevel.D, "资源预下载：{0}", task);
            }
            m_Tasks.Add(task);
            return true;
        }

        public void StopLoading(string assetPath = null, bool keepPreload = true)
        {
            if (assetPath == null) {
                for (int i = 0; i < m_Downloaders.Length; ++i) {
                    var task = m_Downloaders[i].param as AsyncLoadingTask;
                    if (task == null) continue;
                    if (keepPreload && AssetLoader.Instance.IsPreload(task.bundleName))
                        continue;

                    Log2File(LogLevel.D, "资源下载被取消：{0}", task);
                    m_Downloaders[i].Reset();
                    AsyncLoadingTask.Cancel(task);
                }

                for (var i = 0; i < m_Tasks.Count; ++i) {
                    AsyncLoadingTask.Cancel(m_Tasks[i]);
                }
                m_Tasks.Clear();
            } else {
                if (assetPath[assetPath.Length - 1] == '/') {
                    // 停止加载队列中指定名字的资源包
                    string assetBundleName, assetName;
                    AssetLoader.GetAssetpath(assetPath, out assetBundleName, out assetName);
                    for (int i = 0; i < m_Downloaders.Length; ++i) {
                        var task = m_Downloaders[i].param as AsyncLoadingTask;
                        if (task == null) continue;
                        if (keepPreload && AssetLoader.Instance.IsPreload(task.bundleName))
                            continue;

                        if (task.bundleName == assetBundleName) {
                            Log2File(LogLevel.D, "资源下载被取消：{0}", task);
                            m_Downloaders[i].Reset();
                            AsyncLoadingTask.Cancel(task);
                            break;
                        }
                    }

                    CancelTasks(assetBundleName);
                } else {
                    // 停止加载队列中属于指定组的资源包
                    assetPath = assetPath.ToLower();
                    for (int i = 0; i < m_Downloaders.Length; ++i) {
                        var task = m_Downloaders[i].param as AsyncLoadingTask;
                        if (task == null) continue;
                        if (keepPreload && AssetLoader.Instance.IsPreload(task.bundleName))
                            continue;

                        var group = Path.GetFileName(SystemTools.GetDirPath(task.bundleName));
                        if (group == assetPath) {
                            Log2File(LogLevel.D, "资源下载被取消：{0}", task);
                            m_Downloaders[i].Reset();
                            AsyncLoadingTask.Cancel(task);
                        }
                    }
                    for (var i = 0; i < m_Tasks.Count; ++i) {
                        var task = m_Tasks[i];
                        if (task == null) continue;
                        if (keepPreload && AssetLoader.Instance.IsPreload(task.bundleName))
                            continue;

                        var group = Path.GetFileName(SystemTools.GetDirPath(task.bundleName));
                        if (group == assetPath) {
                            Log2File(LogLevel.D, "资源下载被取消：{0}", task);
                            AsyncLoadingTask.Cancel(task);
                            m_Tasks[i] = null;
                        }
                    }
                    m_Tasks.RemoveNull();
                }
            }
        }

        public static void MarkAsDownloaded(string bundleName, long size)
        {
            if (Instance && Instance.onDownloaded != null) {
                Instance.onDownloaded(bundleName, size);
            }
        }

        [Conditional(Log.UNITY_EDITOR), Conditional(Log.DEVELOPMENT_BUILD)]
        public static void Log2File(LogLevel level, string fmt, params object[] args)
        {
            Log.Format(level, fmt, args);
#if !UNITY_EDITOR
            var time = Mathf.RoundToInt(Time.realtimeSinceStartup * 1000) + "|";
            var logSavePath = AssetBundleLoader.CombinePath(AssetBundleLoader.persistentDataPath, "asset_download.log");
            File.AppendAllText(logSavePath, string.Format(time + fmt, args) + "\n");
#endif
        }

    }
}