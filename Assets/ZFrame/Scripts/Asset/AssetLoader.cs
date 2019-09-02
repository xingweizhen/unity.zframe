//#define ASSETLOADED_CBF_TIME
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TinyJSON;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZFrame.Asset
{
    public abstract class AssetLoader : MonoSingleton<AssetLoader>
    {
        public const int VER = 0x3F7A0;

        protected AsyncMultitasking m_Multitasking;

        public System.Action<string, float> Loading;
        protected void OnLoading(string bundle, float f)
        {
            if (Loading != null) Loading(bundle, f);
        }

        protected abstract BundleStat GetBundleStat(string bundleName, out string md5);

        public BundleStat GetBundleStat(string bundleName)
        {
            string md5;
            return GetBundleStat(bundleName, out md5);
        }

        protected BundleStat GetBundleStat(AsyncLoadingTask task)
        {
            if (task.bundleType == BundleType.FileAsset) return BundleStat.Local;

            string md5;
            var stat = GetBundleStat(task.bundleName, out md5);
            task.bundleId = md5;
            return stat;
        }

        /// <summary>
        /// 资源从磁盘载入（同步）
        /// </summary>
        protected abstract AbstractAssetBundleRef PerformTask(System.Type type, string bundleName, string assetName, LoadMethod method);

        /// <summary>
        /// 资源从磁盘载入（异步）
        /// </summary>
		protected abstract IEnumerator PerformTask(AsyncLoadingTask task);
        
        /// <summary>
        /// 加载文件形式的资源
        /// </summary>
        private IEnumerator PerformFileTask(AsyncLoadingTask task)
        {
            using (WWW www = new WWW(task.bundleName)) {
                while (!www.isDone) {
                    yield return null;
                    OnLoading(www.url, Mathf.Min(0.999f, www.progress));
                }
                OnLoading(www.url, 1f);

                if (www.error == null) {
                    object obj = null;
                    if (typeof(Texture).IsAssignableFrom(task.assetType)) {
                        obj = www.texture;
                    } else if (typeof(AudioClip) == task.assetType) {
                        obj = www.GetAudioClip();
                    } else if (typeof(TextAsset) == task.assetType) {
                        obj = www.text;
                    }
                    
                    var bundle = m_FileAssetPool.Get();
                    bundle.Init(task.bundleName, obj);
                    task.bundle = bundle;
                }
            }
        }

        /// <summary>
        /// 同时加载多个资源
        /// </summary>
        //protected abstract IEnumerator PerformMultiTasks(IList<AsyncLoadingTask> tasks, IAssetProgress prog);

        /// <summary>
        /// 场景加载方法
        /// </summary>
        public abstract AsyncOperation LoadLevelAsync(string path, LoadSceneMode mode = LoadSceneMode.Single);

        [Conditional(Log.DEVELOPMENT_BUILD), Conditional(Log.UNITY_EDITOR), Conditional(Log.UNITY_STANDALONE)]
        public static void Info(string fmt, params object[] Args)
        {
            Instance.LogFormat(LogLevel.I, "[Asset] " + fmt, Args);
        }
        
        public static BundleType GetAssetpath(string path, out string assetbundleName, out string assetName)
        {
            var assetType = BundleType.None;
            if (!string.IsNullOrEmpty(path)) {
                if (path.StartsWith("file://", System.StringComparison.OrdinalIgnoreCase)) {
                    assetType = BundleType.FileAsset;
                    path = path.Substring(7);
                    if (path.Contains("file://")) {
                        assetbundleName = path;
                    } else {
                        assetbundleName = "file://" + path;
                    }
                    assetName = Path.GetFileNameWithoutExtension(path);
                } else {
                    assetType = BundleType.AssetBundle;
                    assetbundleName = SystemTools.GetDirPath(path);
                    assetbundleName = assetbundleName.ToLower();
                    try {
                        assetName = Path.GetFileName(path);
                    } catch (System.Exception e) {
                        Instance.LogFormat(LogLevel.W, "GetAssetpath<{0}> error: {1}", path, e.Message);
                        assetName = null;
                        assetbundleName = null;
                        return assetType;
                    }
                }
            } else {
                assetName = null;
                assetbundleName = null;
            }
            return assetType;
        }

        #region 资源管理


        /// <summary>
        /// 已加载的AssetBundles
        /// </summary>
        protected Dictionary<string, AbstractAssetBundleRef> m_LoadedAssetBundles = new Dictionary<string, AbstractAssetBundleRef>();
        /// <summary>
        /// 已加载的文件资源
        /// </summary>
        protected Dictionary<string, AbstractAssetBundleRef> m_LoadedFileAssets = new Dictionary<string, AbstractAssetBundleRef>();
        
        protected abstract void ReleaseAbr(AbstractAssetBundleRef ab);

        private Pool<FileAssetRef> m_FileAssetPool = new Pool<FileAssetRef>(null, null);
        
        protected Dictionary<string, AbstractAssetBundleRef> GetLoadedAssets(string path)
        {
            return path.Contains("file://") ? m_LoadedFileAssets : m_LoadedAssetBundles;
        }

        /// <summary>
        /// 某个资源包是否存在内存中
        /// </summary>
		public bool TryGetAssetBundle(string assetbundleName, out AbstractAssetBundleRef abRef)
        {
            abRef = null;

            if (string.IsNullOrEmpty(assetbundleName)) return false;

            var loadedAssets = GetLoadedAssets(assetbundleName);
            if (loadedAssets.TryGetValue(assetbundleName, out abRef)) {
                abRef.lastLoaded = Time.realtimeSinceStartup;
                return true;
            }
            return false;
        }

        public bool TryGetAssetBundle(Object asset, out AbstractAssetBundleRef abRef)
        {
            abRef = null;
            foreach (var kv in m_LoadedAssetBundles) {
                if (kv.Value.Contains(asset)) {
                    abRef = kv.Value;
                    return true;
                }
            }

            foreach (var kv in m_LoadedFileAssets) {
                if (kv.Value.Contains(asset)) {
                    abRef = kv.Value;
                    return true;
                }
            }

            return false;
        }

        #endregion

        protected void FinishLoadindBundle(string abName, AbstractAssetBundleRef bundle)
        {
            if (bundle != null) {
                bundle.lastLoaded = Time.realtimeSinceStartup;
                Info("Ready: {0}", bundle);
                var loadedAssets = GetLoadedAssets(abName);
                if (!loadedAssets.ContainsKey(abName)) {
                    loadedAssets.Add(abName, bundle);
                }
            } else {
                Info("加载失败：{0}", abName);
            }
        }

#if ASSETLOADED_CBF_TIME
        public Dictionary<string, long> LoadTime = new Dictionary<string, long>();
        public Stopwatch InstantiateWatch = Stopwatch.StartNew();
#endif
        Stopwatch m_Stopwatch = Stopwatch.StartNew();        

        /// <summary>
        /// 每帧允许资源加载回调函数最大耗时
        /// </summary>
        private const long MaxLoadTime = 10; //TimeSpan

        protected override void Awaking()
        {
            CollectGarbage += () => {
                Resources.UnloadUnusedAssets();
                System.GC.Collect();
            };
        }

        protected virtual void Start()
        {
            StartCoroutine(LoadAssetBundleOnebyone());
        }

        private void Update()
        {
            m_Stopwatch.Reset();
            m_Stopwatch.Start();
#if ASSETLOADED_CBF_TIME
            int count = 0;
#endif
            while (AsyncLoadingTask.LoadedCallbacks.Count > 0) {
                var c = AsyncLoadingTask.LoadedCallbacks.Peek();

#if ASSETLOADED_CBF_TIME
                long loadTime = 0;
                if (!string.IsNullOrEmpty(c.path) && LoadTime.TryGetValue(c.path, out loadTime)) {
                    if (maxLoadTime < loadTime && count > 0) break;
                }
#endif

                AsyncLoadingTask.LoadedCallbacks.Dequeue();
#if ASSETLOADED_CBF_TIME
                InstantiateWatch.Reset();
                InstantiateWatch.Start();
#endif
                c.ExecOnLoaded();

#if ASSETLOADED_CBF_TIME
                count++;
                if (!string.IsNullOrEmpty(c.path))
                    LoadTime[c.path] = (InstantiateWatch.ElapsedMilliseconds + loadTime) / 2;
#endif

                var elapsedMilliseconds = m_Stopwatch.ElapsedMilliseconds;
                if (elapsedMilliseconds >= MaxLoadTime) {
                    Info("ElapsedMilliseconds: {0}, Break Executing Loaded Callback. Remaining {1}", 
                        elapsedMilliseconds, AsyncLoadingTask.LoadedCallbacks.Count);
                    break;
                }
            }
        }

#if ASSETLOADED_CBF_TIME
        private void OnDisable()
        {
            var str = JSON.Dump(LoadTime);
            PlayerPrefs.SetString("AssetLoader.LoadTimeMap", str);
        }

        private void OnEnable()
        {
            var str = PlayerPrefs.GetString("AssetLoader.LoadTimeMap");
            if (!string.IsNullOrEmpty(str)) {
                JSON.MakeInto(JSON.Load(str), out LoadTime);
                Debug.LogFormat("加载 [AssetLoader.LoadTimeMap] Success. {0}", str);
            }
        }
#endif

        protected override void Destroying()
        {
            UnloadAll(true);
            base.Destroying();
        }

        protected bool LoadAssetFromCache(AsyncLoadingTask task)
        {
            if (task.needsAsset) {
                var bundle = task.bundle;
                if (bundle != null) {
                    if (bundle.hasAsset) {
                        task.asset = bundle.LoadFromCache(task.assetName, task.assetType);
                        if (task.asset == null) return false;
                    }
                } else {
                    this.LogFormat(LogLevel.W, "[{0}]未加载。<{1}>", task.bundleName, task.assetPath);
                }
            }

            task.OnAssetLoaded();
            return true;
        }

        protected IEnumerator LoadAssetFromBundle(AsyncLoadingTask task, bool releaseOnLoaded)
        {
            if (task.needsAsset) {
                yield return task.bundle.LoadAsync(task);
                if (task.asset == null) {
                    this.LogFormat(LogLevel.W, "[{0}]中不存在{1}", task.bundleName, task.assetName);
                }
            }

            task.OnAssetLoaded();
            task.OnBundleLoaded();
            if (releaseOnLoaded) AsyncLoadingTask.Release(task);
        }

#region 资源加载/释放

        private IEnumerator LoadAssetAsync(AsyncLoadingTask task, LoadedBundle loaded)
        {
            while (m_Tasking != null) yield return null;

            m_Tasking = task;
            var abName = task.bundleName;
            if (!string.IsNullOrEmpty(abName) && !TryGetAssetBundle(abName, out task.bundle)) {
                if (task.bundleType == BundleType.AssetBundle) {
                    yield return PerformTask(task);
                } else if (task.bundleType == BundleType.FileAsset) {
                    yield return PerformFileTask(task);
                }
                FinishLoadindBundle(task.bundleName, task.bundle);                
            }

            if (!LoadAssetFromCache(task)) {
                yield return LoadAssetFromBundle(task, false);
            } else {
                task.OnBundleLoaded();
            }

            if (loaded != null) {
                loaded.bundle = task.bundle;
                loaded.asset = task.asset;                
            }
            
            AsyncLoadingTask.Release(task);
            m_Tasking = null;
        }

        /// <summary>
        /// 执行加载任务
        /// </summary>
		private IEnumerator LoadAssetBundleOnebyone()
        {
            for (; ; ) {
                while (m_OBOTasks.Count == 0) yield return null;
                var task = m_OBOTasks[0];
                m_OBOTasks.RemoveAt(0);

                yield return LoadAssetAsync(task, null);
            }
        }
        
        /// <summary>
        /// 单独加载某个资源
        /// </summary>
        public IEnumerator LoadingAsset(AsyncLoadingTask task, LoadedBundle loaded)
        {
            var stat = GetBundleStat(task);
            if (stat == BundleStat.Local) {
                task.loadType = LoadType.Load;
                yield return LoadAssetAsync(task, loaded);
            } else if ((stat & BundleStat.Remote) != 0) {
                if (AssetDownload.Instance) {
                    task.loadType = LoadType.Download;
                    AssetDownload.Instance.Download(task);
                    while (task.loadType != LoadType.Load) yield return null;
                    yield return LoadAssetAsync(task, loaded);
                } else {
                    this.LogFormat(LogLevel.W, "Bundle [{0}] CANNOT download)", task.bundleName);
                    AsyncLoadingTask.Release(task);
                }
            } else {
                this.LogFormat(LogLevel.W, "Bundle [{0}] NOT exist", task.bundleName);
                AsyncLoadingTask.Release(task);
            }
        }

        /// <summary>
        /// 单独加载某个资源
        /// </summary>
        public IEnumerator LoadingAsset(System.Type type, string bundleName, string assetName, LoadedBundle loaded = null, LoadMethod method = LoadMethod.Default)
        {
            if (loaded != null) {
                loaded.bundle = null;
                loaded.asset = null;
            }
            var task = NewTask(BundleType.AssetBundle, bundleName, assetName, method, null, type);
            yield return LoadingAsset(task, loaded);
        }

        /// <summary>
        /// 单独加载某个资源
        /// </summary>
        public IEnumerator LoadingAsset(System.Type type, string assetPath, LoadedBundle loaded = null, LoadMethod method = LoadMethod.Default)
        {
            if (loaded != null) {
                loaded.bundle = null;
                loaded.asset = null;
            }
            var task = NewTask(assetPath, method, null, type);
            yield return LoadingAsset(task, loaded);
        }

        public bool IsBundleLoaded(string bundleName)
        {
            AbstractAssetBundleRef abRef;
            if (TryGetAssetBundle(bundleName, out abRef)) {
                return true;
            }

            return false;
        }

        public bool IsLoaded(string path)
        {
            string bundleName, assetName;
            GetAssetpath(path, out bundleName, out assetName);
            return IsBundleLoaded(bundleName);
        }

        /// <summary>
        /// 根据包名和资产名加载资源
        /// </summary>  
        public Object Load(System.Type type, string bundleName, string assetName, bool warnIfMissing = true)
        {
            AbstractAssetBundleRef abRef;
            if (TryGetAssetBundle(bundleName, out abRef)) {
                var asset = abRef.Load(assetName, type);
                if (asset) return asset;

                if (warnIfMissing) {
                    this.LogFormat(LogLevel.W, "{0}/{1}<{1}>不存在。[{2} {3}]", bundleName, assetName, type, bundleName, assetName);
                } else {
                    Info("{0}<{1}> not exist.[{2} {3}]", bundleName, assetName, type, bundleName, assetName);
                }
            } else {
                if (warnIfMissing) {
                    this.LogFormat(LogLevel.W, "[{0}]未加载。[{1}]", bundleName, assetName);
                } else {
                    Info("[{0}] isn't loaded. [{1}]", bundleName, assetName);
                }
            }

            return null;
        }

        /// <summary>
        /// 从某个位置加载某个类型的资源
        /// </summary>
        public Object Load(System.Type type, string path, bool warnIfMissing = true)
        {
            string bundleName, assetName;
            GetAssetpath(path, out bundleName, out assetName);

            return Load(type, bundleName, assetName, warnIfMissing);
        }

        public bool TryLoad(System.Type type, string bundleName, string assetName, out Object asset)
        {
            if (!string.IsNullOrEmpty(assetName)) {
                asset = Load(type, bundleName, assetName, false);
                return asset != null;
            } else {
                asset = null;
                return IsBundleLoaded(bundleName);
            }
        }

        public bool TryLoad(System.Type type, string path, out Object asset)
        {
            string bundleName, assetName;
            GetAssetpath(path, out bundleName, out assetName);
            return TryLoad(type, bundleName, assetName, out asset);
        }

        public object LoadSync(BundleType bundleType, System.Type type, string bundleName, string assetName, LoadMethod method)
        {
            AbstractAssetBundleRef bundle = null;
            if (!TryGetAssetBundle(bundleName, out bundle)) {
                if (bundleType == BundleType.AssetBundle) {
                    bundle = PerformTask(type, bundleName, assetName, method);
                    FinishLoadindBundle(bundleName, bundle);
                } else {

                }
            }

            return bundle != null && !string.IsNullOrEmpty(assetName) ? bundle.Load(assetName, type) : null;
        }

        public object LoadSync(System.Type type, string assetPath, LoadMethod method)
        {
            string bundleName, assetName;
            var bundleType = GetAssetpath(assetPath, out bundleName, out assetName);

            return LoadSync(bundleType, type, bundleName, assetName, method);
        }

        /// <summary>
        /// 根据包名和资产名加载资源（异步）
        /// </summary>  
        public AsyncLoadingTask LoadAsync(System.Type type, string bundleName, string assetName, LoadMethod method, 
            DelegateObjectLoaded onObjectLoaded = null, object param = null)
        {
            if (string.IsNullOrEmpty(bundleName)) {
                var task = AsyncLoadingTask.Get();
                task.SetBundle(method, null).SetAsset(type, onObjectLoaded, param);
                return ScheduleTask(task);
            } else {
                var task = NewTask(BundleType.AssetBundle, bundleName, assetName, method, null, type, onObjectLoaded, param);
                return ScheduleTask(task);
            }
        }

        /// <summary>
        /// 从某个位置加载某个类型的资源（异步）
        /// </summary>
        public AsyncLoadingTask LoadAsync(System.Type type, string path, LoadMethod method, 
            DelegateObjectLoaded onObjectLoaded = null, object param = null)
        {
            if (string.IsNullOrEmpty(path)) {
                var task = AsyncLoadingTask.Get();
                task.SetBundle(method, null).SetAsset(type, onObjectLoaded, param);
                return ScheduleTask(task);
            } else {
                var task = NewTask(path, method, null, type, onObjectLoaded, param);
                return ScheduleTask(task);
            }
        }

        public void Unload(AbstractAssetBundleRef ab, bool forced)
        {
            ab.Unload(forced);
            if (ab.IsEmpty()) {
                if (ab.bundleType == BundleType.AssetBundle) {
                    m_LoadedAssetBundles.Remove(ab.name);
                    ReleaseAbr(ab);
                } else if (ab.bundleType == BundleType.FileAsset) {
                    var ret = m_LoadedFileAssets.Remove(ab.name);
                    Debug.LogFormat("remove {0}={1} : {2}", ab.name, ab, ret);
                    m_FileAssetPool.Release(ab as FileAssetRef);
                }
            }
        }

        /// <summary>
        /// 释放某个资源包
        /// </summary>
        public void Unload(string path, bool forced)
        {
            AbstractAssetBundleRef abRef;
            string assetbundleName, assetName;
            GetAssetpath(path, out assetbundleName, out assetName);
            if (TryGetAssetBundle(assetbundleName, out abRef)) {
                Unload(abRef, forced);
            }
        }

        private void UnloadAssets(Dictionary<string, AbstractAssetBundleRef> dict, bool forced)
        {
            m_ABList.AddRange(dict.Values);
            foreach (var abRef in m_ABList) {
                if (m_PreloadAssetBundles.ContainsKey(abRef.name)) {
                    Info("Keep " + abRef.ToString());
                } else {
                    Unload(abRef, forced);
                }
            }
            m_ABList.Clear();
        }

        /// <summary>
        /// 释放所有资源包
        /// </summary>
        public void UnloadAll(bool forced = false)
        {
            Info("UnloadAll: {0}", forced);
            if (forced) ClearPreload();

            UnloadAssets(m_LoadedAssetBundles, forced);
            UnloadAssets(m_LoadedFileAssets, forced);

            GC();
        }

        /// <summary>
        /// 清空任务队列，停止加载资源包
        /// </summary>
        public void StopLoading(string assetPath = null, bool keepPreload = true)
        {
            if (assetPath == null) {
                // 停止加载队列中所有资源包
                for (int i = 0; i < m_OBOTasks.Count; ++i) {
                    var task = m_OBOTasks[i];
                    if (!task.allowUnload) continue;
                    if (keepPreload && m_PreloadAssetBundles.ContainsKey(task.bundleName))
                        continue;
                    
                    AsyncLoadingTask.Cancel(m_OBOTasks[i]);
                    m_OBOTasks[i] = null;
                }
                
            } else {
                if (assetPath[assetPath.Length - 1] == '/') {
                    // 停止加载队列中指定名字的资源包
                    string assetBundleName, assetName;
                    GetAssetpath(assetPath, out assetBundleName, out assetName);
                    for (int i = 0; i < m_OBOTasks.Count; ++i) {
                        var task = m_OBOTasks[i];
                        if (!task.allowUnload) continue;
                        if (keepPreload && m_PreloadAssetBundles.ContainsKey(task.bundleName))
                            continue;
                        
                        if (task.bundleName == assetBundleName) {
                            AsyncLoadingTask.Cancel(m_OBOTasks[i]);
                            m_OBOTasks[i] = null;
                        }
                    }
                } else {
                    // 停止加载队列中属于指定组的资源包
                    assetPath = assetPath.ToLower();
                    for (int i = 0; i < m_OBOTasks.Count; ++i) {
                        var task = m_OBOTasks[i];
                        if (!task.allowUnload) continue;
                        if (keepPreload && m_PreloadAssetBundles.ContainsKey(task.bundleName))
                            continue;
                        
                        var group = Path.GetFileName(SystemTools.GetDirPath(task.bundleName));
                        if (group == assetPath) {
                            AsyncLoadingTask.Cancel(m_OBOTasks[i]);
                            m_OBOTasks[i] = null;
                        }
                    }
                }
            }
            
            m_OBOTasks.RemoveNull();

            if (AssetDownload.Instance) AssetDownload.Instance.StopLoading(assetPath, keepPreload);
        }

        private List<AbstractAssetBundleRef> m_ABList = new List<AbstractAssetBundleRef>();
        /// <summary>
        /// 限制某一组的资源包的最大数量，多出来的比较早使用的会被释放。
        /// </summary>
		public int LimitAssetBundle(string group, int limit)
        {
            int nUnload = 0;
            group = group.ToLower();

            using (var itor = m_LoadedAssetBundles.Values.GetEnumerator()) {
                while (itor.MoveNext()) {
                    var abf = itor.Current;
                    if (abf.allowUnload && abf.group == group) {
                        m_ABList.Add(abf);
                    }
                }
            }

            if (m_ABList.Count > limit) {
                m_ABList.Sort();
                nUnload = m_ABList.Count - limit;
#if UNITY_EDITOR || UNITY_STANDALONE
                var strbld = new System.Text.StringBuilder();
                for (int i = 0; i < m_ABList.Count; ++i) {
                    strbld.AppendLine(m_ABList[i].ToString());
                }
                Instance.LogFormat(LogLevel.D, "LAB:\n{0}", strbld.ToString());
#endif
                for (int i = 0; i < nUnload; ++i) {
                    Instance.LogFormat(LogLevel.D, "LAB: [{0}]:{1}/{2}, -{3}", group, m_ABList.Count, limit, m_ABList[i]);
                    Unload(m_ABList[i], false);
                }
            }
            m_ABList.Clear();
            return nUnload;
        }
#endregion

#region 管理加载任务队列        
        private List<AsyncLoadingTask> m_OBOTasks = new List<AsyncLoadingTask>();
        protected AsyncLoadingTask m_Tasking;
        public event TaskInitializer InitNewTask;

        public AsyncLoadingTask NewTask(BundleType bundleType, string bundleName, string assetName, LoadMethod method = LoadMethod.Default,
            DelegateAssetBundleLoaded onLoaded = null, System.Type type = null,
            DelegateObjectLoaded onObjectLoaded = null, object param = null)
        {
            UnityEngine.Assertions.Assert.IsNotNull(bundleName);

            var task = AsyncLoadingTask.Get();

            if (InitNewTask != null) {
                InitNewTask.Invoke(task, bundleName, assetName, method, onLoaded, type, onObjectLoaded, param);
            }

            if (task.bundleType == BundleType.None) {
                task.SetInfo(bundleType, bundleName, assetName).SetBundle(method, onLoaded).SetAsset(type, onObjectLoaded, param);
            }

#if UNITY_EDITOR
            if (GetBundleStat(task) == BundleStat.NotExist) {
                Info("{0}资源不存在。", task);
            }
#endif
            return task;
        }

        public AsyncLoadingTask NewTask(string path, LoadMethod method = LoadMethod.Default,
            DelegateAssetBundleLoaded onLoaded = null, System.Type type = null, 
            DelegateObjectLoaded onObjectLoaded = null, object param = null)
        {
            string bundleName, assetName;
            var bundleType = GetAssetpath(path, out bundleName, out assetName);
            return NewTask(bundleType, bundleName, assetName, method, onLoaded, type, onObjectLoaded, param);
        }

        /// <summary>
        /// 添加一个资源包加载任务
        /// </summary>
        public virtual AsyncLoadingTask ScheduleTask(AsyncLoadingTask task)
        {
            if (TryGetAssetBundle(task.bundleName, out task.bundle)) {
                Info("Loaded: {0}", task);
                if (task.needsAsset) {
                    task.asset = task.bundle.LoadFromCache(task.assetName, task.assetType);
                    if (task.asset != null) {
                        task.OnAssetLoaded();
                    } else {
                        StartCoroutine(LoadAssetFromBundle(task, true));
                        return null;
                    }
                } else {
                    task.OnAssetLoaded();
                }

                task.OnBundleLoaded();
                AsyncLoadingTask.Release(task);
                return null;
            }

            var stat = GetBundleStat(task);
            if (stat == BundleStat.Local) {
                task.loadType = LoadType.Load;
                Info("Enqueue: {0}", task);
                m_OBOTasks.Add(task);
                if (m_Multitasking != null) m_Multitasking.AddTask(task);
            } else if ((stat & BundleStat.Remote) != 0) {
                if (AssetDownload.Instance) {
                    task.loadType = LoadType.DownloadAndLoad;
                    Info("Download: {0}", task);
                    AssetDownload.Instance.Download(task);
                    if (m_Multitasking != null) m_Multitasking.AddTask(task);
                } else {
                    this.LogFormat(LogLevel.W, "Bundle [{0}] CANNOT download", task.bundleName);
                    AsyncLoadingTask.Release(task);
                    task = null;
                }
            } else {
                this.LogFormat(LogLevel.W, "Bundle [{0}] NOT exist", task.bundleName);
                AsyncLoadingTask.Release(task);
                task = null;
            }

            return task;
        }

        public virtual bool DownloadTask(AsyncLoadingTask task)
        {
            var stat = GetBundleStat(task);
            if ((stat & BundleStat.Remote) != 0) {
                if (AssetDownload.Instance) {
                    task.loadType = LoadType.Load;
                    return AssetDownload.Instance.Download(task);
                } else {
                    this.LogFormat(LogLevel.W, "Bundle [{0}] CANNOT download", task.bundleName);
                }
            }
            AsyncLoadingTask.Release(task);
            return false;
        }

        /// <summary>
        /// 把一个资源包名称加入加载任务列表。
        /// </summary>
        public AsyncLoadingTask BundleTask(string assetPath, LoadMethod method, DelegateAssetBundleLoaded onLoaded = null)
        {
            return ScheduleTask(NewTask(assetPath, method, onLoaded));
        }

        public void BeginMultitasking(DelegateObjectLoaded onLoaded, object param = null, IAssetProgress progress = null)
        {
            m_Multitasking = AsyncMultitasking.Get(onLoaded, param, progress);
        }

        public AsyncMultitasking EndMultitasking()
        {
            AsyncMultitasking ret = m_Multitasking;
            if (ret != null) {
                ret.ConfirmTask();
                m_Multitasking = null;
            }
            return ret;
        }

#endregion

#region 管理载入场景前需要预先加载的资源
        private Dictionary<string, PreloadAsset> m_PreloadAssetBundles = new Dictionary<string, PreloadAsset>();

        protected virtual void AddBundleToPreload(string bundleName, string assetPath, LoadMethod method)
        {
            if (!m_PreloadAssetBundles.ContainsKey(bundleName)) {
                m_PreloadAssetBundles.Add(bundleName, new PreloadAsset(assetPath, method));
            }
        }

        /// <summary>
        /// 缓存需要预加载的资源包路径
        /// </summary>
		public void CachedPreload(string assetPath, LoadMethod method)
        {
            string assetBundleName, assetName;
            GetAssetpath(assetPath, out assetBundleName, out assetName);
            AddBundleToPreload(assetBundleName, assetPath, method);
        }

        /// <summary>
        /// 清除预载缓存
        /// </summary>
		public void ClearPreload()
        {
            m_PreloadAssetBundles.Clear();
        }

        /// <summary>
        /// 需要预加载的资源数量
        /// </summary>
        /// <returns></returns>
        public int CountPreLoad()
        {
            return m_PreloadAssetBundles.Count;
        }

        public bool IsPreload(string bundleName)
        {
            return m_PreloadAssetBundles.ContainsKey(bundleName);
        }

        /// <summary>
        /// 执行预加载
        /// </summary>
        public AsyncMultitasking ExecutePreload(DelegateObjectLoaded onLoadedAll = null, IAssetProgress progress = null)
        {
            BeginMultitasking(onLoadedAll, progress: progress);
            foreach (var preload in m_PreloadAssetBundles.Values) {
                BundleTask(preload.path, preload.method);
            }

            m_PreloadAssetBundles.Clear();
            return EndMultitasking();
        }

        public IEnumerator PreloadingBundles(IAssetProgress progress)
        {
            var tasking = ExecutePreload(progress: progress);
            while (tasking.IsProcessing()) yield return null;
        }

#endregion

        public event System.Action CollectGarbage;
        public void GC()
        {
            CollectGarbage();
        }

        public bool isQuiting { get; private set; }
        public event System.Action AppQuit;
        private void OnApplicationQuit()
        {
            isQuiting = true;
            //Bundle.Unpacker.isQuiting = true;
            if (AppQuit != null) AppQuit.Invoke();
        }

#if UNITY_EDITOR
        public static Object EditorLoadAsset(BundleType bundleType, System.Type type, string bundleName, string assetName)
        {
            if (bundleType != BundleType.AssetBundle) return null;

            bundleName = bundleName.ToLower();
            var paths = string.IsNullOrEmpty(assetName)
                ? UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(bundleName)
                : UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName, assetName);

            if (paths.Length <= 0) return null;
            return type != null
                ? UnityEditor.AssetDatabase.LoadAssetAtPath(paths[0], type)
                : UnityEditor.AssetDatabase.LoadMainAssetAtPath(paths[0]);
        }

        public static Object EditorLoadAsset(System.Type type, string path)
        {
            string bundleName, assetName;
            var bundleType = GetAssetpath(path, out bundleName, out assetName);          

            return EditorLoadAsset(bundleType, type, bundleName, assetName);
        }
#endif
        [Conditional("UNITY_EDITOR")]
        public static void AssignEditorShader(Material mat)
        {
#if !UNITY_STANDALONE
            if (AssetBundleLoader.I) {
                if (mat && mat.shader) mat.shader = Shader.Find(mat.shader.name);
            }
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void AssignEditorShaders(GameObject go)
        {
#if !UNITY_STANDALONE
            if (AssetBundleLoader.I) {
                var list = ListPool<Component>.Get();
                go.GetComponentsInChildren(typeof(Renderer), list, true);
                foreach (var rdr in list) {
                    foreach (var m in ((Renderer)rdr).sharedMaterials) {
                        if (m && m.shader) m.shader = Shader.Find(m.shader.name);
                    }
                }
                list.Clear();
                go.GetComponentsInChildren(typeof(Projector), list, true);
                foreach (var com in list) {
                    var m = ((Projector)com).material;
                    if (m && m.shader) m.shader = Shader.Find(m.shader.name);
                }
                ListPool<Component>.Release(list);
            }
#endif
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(AssetLoader), true)]
        public class AssetLoaderEditor : Editor
        {
            private void InspectObject(string fName, object obj)
            {
                if (obj is Object) {
                    var unityObj = obj as Object;
                    EditorGUILayout.ObjectField(fName, unityObj, unityObj.GetType(), true);
                } else {
                    EditorGUILayout.TextField(fName, obj == null ? "NULL" : obj.ToString());
                }
            }

            protected void ShowDictionary(string desc, IDictionary dict)
            {
                EditorGUILayout.LabelField(desc, string.Format("数量={0}", dict.Count));
                
                ++EditorGUI.indentLevel;
                foreach (DictionaryEntry kv in dict) {
                    var key = kv.Key.ToString();
                    if (!string.IsNullOrEmpty(m_SearchText) && key.IndexOf(m_SearchText, System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                    InspectObject(key, kv.Value);
                }

                --EditorGUI.indentLevel;
            }

            private string m_SearchText;

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                m_SearchText = EditorAPI.SearchField(m_SearchText);

                var self = target as AssetLoader;
                ShowDictionary("已加载的资源", self.m_LoadedAssetBundles);
                ShowDictionary("已加载的资源", self.m_LoadedFileAssets);
            }
        }
#endif
    }
}
