using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TinyJSON;

namespace ZFrame.Asset
{
    using NetEngine;

    public sealed class AssetBundleLoader : AssetLoader
    {
        public const float ASYNC_LOAD_TIME = 1f;

        public static AssetBundleLoader I { get { return Instance as AssetBundleLoader; } }

        static string m_persistentDataPath;
        public static string persistentDataPath {
            get {
                if (m_persistentDataPath == null) {
#if UNITY_EDITOR || UNITY_STANDALONE
                    m_persistentDataPath = SystemTools.GetDirPath(Application.dataPath) + "/Issets/PersistentData";
#else
					m_persistentDataPath = Application.persistentDataPath;
#endif
                }
                return m_persistentDataPath;
            }
        }
        static string m_streamingAssetPath;
        public static string streamingAssetsPath {
            get {
                if (m_streamingAssetPath == null) {
#if UNITY_EDITOR
                    m_streamingAssetPath = SystemTools.GetDirPath(Application.dataPath) + "/Issets/StreamingAssets";
#else
					m_streamingAssetPath = Application.streamingAssetsPath;
#endif
                }
                return m_streamingAssetPath;
            }
        }

        public const string MD5 = "md5";
        public const string FILE_LIST = "filelist";

        public static string CombinePath(string path1, string path2)
        {
            return string.Concat(path1, '/', path2);
        }

        public const string ASSETBUNDLE_FOLDER = "AssetBundles";
        static string m_bundleRoot;
        public static string bundleRootPath {
            get {
                if (m_bundleRoot == null) {
                    m_bundleRoot = CombinePath(persistentDataPath, ASSETBUNDLE_FOLDER);
                }
                return m_bundleRoot;
            }
        }

        public const string DOWNLOAD_FOLDER = "Downloads";
        static string m_downloadRoot;
        public static string downloadRootPath {
            get {
                if (m_downloadRoot == null) {
                    m_downloadRoot = CombinePath(persistentDataPath, DOWNLOAD_FOLDER);
                }
                return m_downloadRoot;
            }
        }

        static string m_streamingRoot;
        public static string streamingRootPath {
            get {
                if (m_streamingRoot == null) {
                    m_streamingRoot = CombinePath(streamingAssetsPath, ASSETBUNDLE_FOLDER);
                }
                return m_streamingRoot;
            }
        }

        private AssetBundleManifest m_Manifest;
        private readonly HashSet<string> m_ExistBundles = new HashSet<string>();
        private readonly Dictionary<string, string> m_RemoteBundles = new Dictionary<string, string>();

        public bool HasBundle(string bundleName)
        {
            return m_ExistBundles.Contains(bundleName);
        }

        public bool AddBundle(string bundleName, string md5)
        {
            if (md5 != null) {
                string expectMD5;
                m_RemoteBundles.TryGetValue(bundleName, out expectMD5);
                if (expectMD5 == null) {
                    AssetDownload.Log2File(LogMgr.LogLevel.W, "资源校验异常： {0}的CRC期待值为0", bundleName);
                } else {
                    if (string.CompareOrdinal(md5, expectMD5) != 0) {
                        AssetDownload.Log2File(LogMgr.LogLevel.W,
                            "资源下载异常：{0}的MD5期望值={1}, 实际值={2}", bundleName, expectMD5, md5);
#if !UNITY_EDITOR
                        return false;
#endif
                    } else {
                        AssetDownload.Log2File(LogMgr.LogLevel.D, "资源下载完成：{0} MD5={1}", bundleName, expectMD5);
                    }
                }
            }

            m_ExistBundles.Add(bundleName);
            m_RemoteBundles.Remove(bundleName);
            return true;
        }

        public void MarkBundle(string bundleName, string md5)
        {
            if (m_RemoteBundles.ContainsKey(bundleName)) {
                m_RemoteBundles[bundleName] = md5;
            } else {
                m_RemoteBundles.Add(bundleName, md5);
            }
            // 不移除记录，表示该资源是待更新状态
            // m_ExistBundles.Remove(bundleName);
        }

        protected override void Awaking()
        {
            base.Awaking();

            var ab = AssetBundle.LoadFromFile(GetAssetBundlePath("AssetBundles", false));
            m_Manifest = ab.LoadAsset("AssetBundleManifest", typeof(AssetBundleManifest)) as AssetBundleManifest;
        }

        private void OnDestroy()
        {
            AssetBundle.UnloadAllAssetBundles(true);
        }

        private Pool<AssetBundleRef> m_ABPool = new Pool<AssetBundleRef>(null, null);
        protected override void ReleaseAbr(AbstractAssetBundleRef ab)
        {
            m_ABPool.Release(ab as AssetBundleRef);
        }

        private string GetStreamingPath(string fileName)
        {
            return CombinePath(streamingRootPath, fileName);
        }

        private string GetFilePath(string fileName)
        {
            var path = GetStreamingPath(fileName);
            if (!path.OrdinalStartsWith("file://")) path = "file://" + path;
            return path;
        }

        private string GetAssetBundlePath(string assetbundleName, bool forcedStreaming)
        {
            if (!forcedStreaming) {
                var suitPath = CombinePath(bundleRootPath, assetbundleName);
                if (File.Exists(suitPath)) return suitPath;

                suitPath = CombinePath(downloadRootPath, assetbundleName);
                if (File.Exists(suitPath)) return suitPath;
            }

            return CombinePath(streamingRootPath, assetbundleName);
        }

        private IEnumerable<string> GetDependencies(string bundleName)
        {
            var deps = m_Manifest.GetAllDependencies(bundleName);

#if UNITY_EDITOR
            if (deps.Length > 0) {
                var strbld = new System.Text.StringBuilder();
                foreach (var d in deps) {
                    AbstractAssetBundleRef abRef;
                    if (!TryGetAssetBundle(d, out abRef)) {
                        strbld.Append(d).Append("; ");
                    }
                }
                if (strbld.Length > 0) LogMgr.D("[Asset] [DEP] [{0}]需要加载的依赖项：{1}", bundleName, strbld.ToString());
            }
#endif
            return deps;
        }

//        public override AsyncLoadingTask ScheduleTask(AsyncLoadingTask task)
//        {
//#if true
//            foreach (var abName in GetDependencies(task.bundleName)) {
//                var dTask = AsyncLoadingTask.Get();
//                dTask.SetBundleAsset(abName, null);
//                base.ScheduleTask(dTask);
//            }
//#endif
//            return base.ScheduleTask(task);
//        }

        private bool InitBundleList(Variant joList)
        {
            var bundleListDirty = false;
            foreach (var kv in (ProxyObject)joList["Assets"]) {
                var assetPath = CombinePath(bundleRootPath, kv.Key);

                var joSiz = kv.Value["siz"];
                if (joSiz != null) {
                    if (joSiz == 0) {
                        m_ExistBundles.Add(kv.Key);
                    } else {
                        if (File.Exists(assetPath)) {
                            m_ExistBundles.Add(kv.Key);
                        } else {
                            kv.Value["siz"] = null;
                            bundleListDirty = true;
                            m_RemoteBundles.Add(kv.Key, kv.Value["md5"]);
                        }
                    }
                } else {
                    if (File.Exists(assetPath)) {
                        m_ExistBundles.Add(kv.Key);
                        kv.Value["siz"] = new ProxyNumber(new FileInfo(assetPath).Length);
                        bundleListDirty = true;
                    } else {
                        m_RemoteBundles.Add(kv.Key, kv.Value["md5"]);
                    }
                }
            }

            return bundleListDirty;
        }

        /// <summary>
        /// 合并内置和使用中的filelist
        /// </summary>
        /// <param name="joList">内置文件列表</param>
        /// <param name="joCached">使用中文件列表</param>
        /// <returns>内置文件是否发生了变化</returns>
        private bool MergeBundleList(Variant joList, Variant joCached)
        {
            var bundleListDirty = false;
            var dictCached = new Dictionary<string, string>();
            var joAssets = joCached["Assets"] as ProxyObject;
            if (joAssets != null) {
                foreach (var kv in joAssets) {
                    if (kv.Value["siz"] != null) {
                        dictCached.Add(kv.Key, kv.Value["md5"]);
                    }
                }
            }

            foreach (var kv in (ProxyObject)joList["Assets"]) {
                var assetPath = CombinePath(bundleRootPath, kv.Key);
                var assetExist = File.Exists(assetPath);
                if (kv.Value["siz"] != null) {
                    m_ExistBundles.Add(kv.Key);
                    // 移除内置资源的覆盖文件
                    if (assetExist) File.Delete(assetPath);
                } else {
                    string md5 = kv.Value["md5"];
                    string cacheMd5;
                    if (dictCached.TryGetValue(kv.Key, out cacheMd5) && cacheMd5 == md5 && assetExist) {
                        // 资源已同步
                        m_ExistBundles.Add(kv.Key);
                        kv.Value["siz"] = new ProxyNumber(new FileInfo(assetPath).Length);
                        bundleListDirty = true;
                    } else {
                        // 删除不匹配的资源， 并标志为远程资源
                        if (assetExist) File.Delete(assetPath);
                        m_RemoteBundles.Add(kv.Key, md5);
                    }
                }
            }

            return bundleListDirty;
        }

        protected override BundleStat GetBundleStat(string bundleName, out string md5)
        {
            md5 = null;

            if (string.IsNullOrEmpty(bundleName)) return BundleStat.Local;

            var stat = BundleStat.NotExist;

            if (m_RemoteBundles.TryGetValue(bundleName, out md5)) {
                stat |= BundleStat.Remote;
            }

            if (m_ExistBundles.Contains(bundleName)) {
                stat |= BundleStat.Local;
            }

            return stat;
        }

        protected override void AddBundleToPreload(string bundleName, string assetPath, LoadMethod method)
        {
            var deps = GetDependencies(bundleName);
            if (deps != null) {
                foreach (var abName in deps) {
                    base.AddBundleToPreload(abName, abName + "/", LoadMethod.Default);
                }
            }
            base.AddBundleToPreload(bundleName, assetPath, method);
        }

        private AbstractAssetBundleRef LoadBundleFromFile(string abName, LoadMethod method)
        {
            // 打断正在进行的同名资源的异步加载
            if (m_Tasking != null && m_Tasking.bundleName == abName) {
                var createReq = m_Tasking.async as AssetBundleCreateRequest;
                if (createReq != null && !createReq.isDone) {
                    LogMgr.W("[Asset] [SYNC] 打断进行中的异步加载。[{0}]", abName);
                    createReq.assetBundle.Unload(true);
                }
            }

            var suitPath = GetAssetBundlePath(abName, false);
            AbstractAssetBundleRef abRef;
            if (!TryGetAssetBundle(abName, out abRef)) {
                var startTime = Time.realtimeSinceStartup;                
                var ab = AssetBundle.LoadFromFile(suitPath);                
                if (ab != null) {
                    LogMgr.W("[Asset] [SYNC] Load [{0}]|{1} in {2}ms", 
                        abName, (AssetOp)method, (Time.realtimeSinceStartup - startTime) * 1000);
                    var bundle = m_ABPool.Get();
                    var allAssets = method.HasOp(AssetOp.Cache) ? ab.LoadAllAssets() : empty;
                    bundle.Init(abName, ab, allAssets, method);
                    FinishLoadindBundle(abName, bundle);
                    abRef = bundle;
                } else {
                    LogMgr.W("[Asset] [SYNC] Load [{0}] FAILURE!", abName);
                }
            }
            return abRef;
        }

        protected override AbstractAssetBundleRef PerformTask(System.Type type, string bundleName, string assetName, LoadMethod method)
        {
            // 先加载依赖
            var deps = GetDependencies(bundleName);
            if (deps != null) {
                foreach (var abName in deps) {
                    LoadBundleFromFile(abName, LoadMethod.Default);
                }
            }

            return LoadBundleFromFile(bundleName, method);
        }

        private IEnumerator LoadBundleFromFileAsync(AsyncLoadingTask task)
        {
            string abName = task.bundleName;
            string suitPath = GetAssetBundlePath(abName, task.forcedStreaming);

            var startTime = Time.realtimeSinceStartup;
            var req = AssetBundle.LoadFromFileAsync(suitPath);
            task.async = req;
            while (!req.isDone) {
                yield return null;
                task.loadingProgress = req.progress * 0.5f;
                OnLoading(abName, task.loadingProgress);
            }
            var costTime = Time.realtimeSinceStartup - startTime;
            if (costTime > ASYNC_LOAD_TIME) {
                LogMgr.W("[Asset] LoadFromFileAsync [{0}]|{1} in {2}ms", abName, (AssetOp)task.method, costTime * 1000);
            } else {
                Log("LoadFromFileAsync [{0}]|{1} in {2}ms", abName, (AssetOp)task.method, costTime * 1000);
            }

            AssetBundle ab = req.assetBundle;
            if (ab != null) {
                Object[] allAssets = empty;
                if (!ab.isStreamedSceneAssetBundle) {
                    if (task.allowCache) {
                        startTime = Time.realtimeSinceStartup;
                        // 这是一个普通的资源包，获取里面的所有Assets以缓存。
                        AssetBundleRequest abReq = ab.LoadAllAssetsAsync();
                        while (!abReq.isDone) {
                            yield return null;
                            task.loadingProgress = Mathf.Min(0.999f, 0.5f + abReq.progress * 0.5f);
                            OnLoading(abName, task.loadingProgress);
                        }
                        costTime = Time.realtimeSinceStartup - startTime;
                        if (costTime > ASYNC_LOAD_TIME) {
                            LogMgr.W("[Asset] LoadAllAssetsAsync [{0}]|{1} in {2}ms", abName, (AssetOp)task.method, costTime * 1000);
                        } else {
                            Log("LoadAllAssetsAsync [{0}]|{1} in {2}ms", abName, (AssetOp)task.method, costTime * 1000);
                        }

                        if (abReq.allAssets != null) allAssets = abReq.allAssets;
                    }
                } else {
                    // 这是一个场景资源包，不需要获取里面的Assets
                    // 需要保留文件镜像的内存直到场景被载入。
                }

                task.loadingProgress = 1f;
                OnLoading(abName, task.loadingProgress);

                var bundle = m_ABPool.Get();
                bundle.Init(abName, ab, allAssets, task.method);
                task.bundle = bundle;
            } else {
                LogMgr.W("{0}加载失败。", task);
            }
        }

        protected override IEnumerator PerformTask(AsyncLoadingTask task)
        {
            // 先加载依赖
            var deps = GetDependencies(task.bundleName);
            if (deps != null) {
                foreach (var abName in deps) {
                    AbstractAssetBundleRef abRef;
                    if (!TryGetAssetBundle(abName, out abRef)) {
                        var subTask = AsyncLoadingTask.Get();
                        subTask.SetInfo(BundleType.AssetBundle, abName, null);
                        m_Tasking = subTask;
                        yield return LoadBundleFromFileAsync(subTask);
                        FinishLoadindBundle(abName, subTask.bundle);
                        AsyncLoadingTask.Release(subTask);
                    }
                }
                m_Tasking = task;
            }

            yield return LoadBundleFromFileAsync(task);
        }

#if false
        protected override IEnumerator PerformMultiTasks(IList<AsyncLoadingTask> tasks, IAssetProgress prog)
        {
            var time = Time.realtimeSinceStartup;
            var strbld = new System.Text.StringBuilder();

            int maxTask = tasks.Count;
            int totalTask = maxTask + 1;
            AsyncOperation last = null;
            for (int i = 0, n = 0; i < tasks.Count; ++n) { 
            //for (int i = maxTask - 1; i >= 0; --i) {
                var task = tasks[i];
                if (!TryGetAssetBundle(task.bundleName, out task.bundle)) {
                    strbld.AppendLine(task.ToString());
                    string suitPath = GetAssetBundlePath(task.bundleName, false);
                    var req = AssetBundle.LoadFromFileAsync(suitPath);
                    // 读取文件一个一个来
                    while (!req.isDone) {
                        yield return null;
                        if (prog != null) {
                            var progress = n + (last == null ? req.progress : (req.progress + last.progress) / 2);
                            prog.SetProgress(progress / totalTask);
                        }
                    }

                    var ab = req.assetBundle;
                    if (ab) {
                        var bundle = m_ABPool.Get();
                        bundle.Init(task.bundleName, ab, empty, task.method);
                        task.bundle = bundle;

                        if (task.allowCache && !ab.isStreamedSceneAssetBundle) {
                            // 将继续实例化内部资源
                            task.async = ab.LoadAllAssetsAsync();
                            last = task.async;
                            ++i; continue;
                        }
                    }
                    last = null;
                    FinishLoadindBundle(task);
                }
                task.OnBundleLoaded();

                if (prog != null) {
                    prog.SetProgress((n + 1f) / totalTask);
                    prog.OnBundleLoaded(task.bundleName, task.bundle);
                }

                tasks.RemoveAt(i);
                AsyncLoadingTask.Release(task);
            }
            // 等待资源加载完成
            var max = 0f;
            while (tasks.Count > 0) {
                yield return null;
                for (int i = 0; i < tasks.Count;) {
                    //for (int i = tasks.Count - 1; i >= 0; --i) {
                    var task = tasks[i];
                    if (task.async.isDone) {
                        var allAssets = (task.async as AssetBundleRequest).allAssets;
                        (task.bundle as AssetBundleRef).CacheAssets(allAssets);
                        FinishLoadindBundle(task);
                        if (!LoadAssetFromCache(task)) {
                            yield return LoadAssetFromBundle(task);
                        }
                        task.OnBundleLoaded();
                        if (prog != null) {
                            prog.OnBundleLoaded(task.bundleName, task.bundle);
                        }

                        tasks.RemoveAt(i);
                        AsyncLoadingTask.Release(task);
                    } else {
                        if (max < task.async.progress) {
                            max = task.async.progress;
                        }
                        ++i;
                    }
                }

                if (prog != null) {
                    prog.SetProgress(Mathf.Min(0.999f, (maxTask + max) / totalTask));
                }
            }
            if (prog != null) {
                prog.SetProgress(1f);
            }

            LogMgr.Log(string.Format("加载资源耗时{0}秒：\n{1}", Time.realtimeSinceStartup - time, strbld.ToString()));
        }        
#endif

        /// <summary>
        /// 开始加载一个AssetBundle, 只会从streamingRootPath目录下加载
        /// </summary>
        /// <param name="assetPath">AssetBundle的路径</param>
        /// <param name="method">是否运行完全卸载</param>
        /// <param name="onLoaded">加载完成回调处理</param>
        /// <param name="needMD5">是否需要计算文件的MD5值</param>
        public void StreamingTask(string assetPath, LoadMethod method, DelegateAssetBundleLoaded onLoaded = null)
        {
            var task = NewTask(assetPath, method, onLoaded);
            task.forcedStreaming = true;
            ScheduleTask(task);
        }

        public override AsyncOperation LoadLevelAsync(string path, LoadSceneMode mode = LoadSceneMode.Single)
        {
            // 解析出资源包和资源对象
            string assetbundleName, assetName;
            GetAssetpath(path, out assetbundleName, out assetName);

            if (assetbundleName == string.Empty) {
                assetName = path;
            }

            return SceneManager.LoadSceneAsync(assetName, mode);
        }

        public string md5 { get; private set; }
        public IEnumerator LoadMD5()
        {
            var loaded = LoadedBundle.Get();
            yield return LoadingAsset(typeof(TextAsset), GetFilePath(MD5), loaded);
            md5 = loaded.asset as string;
            LoadedBundle.Release(loaded);
        }

        public IEnumerator LoadFileList(bool reset = false)
        {
            string filePath = CombinePath(bundleRootPath, FILE_LIST);
            var joCached = File.Exists(filePath) ? JSON.Load(File.ReadAllText(filePath)) : null;

            if (reset || joCached == null) {
                var loaded = LoadedBundle.Get();
#if UNITY_EDITOR
                var editorFileList = string.Format("file://{0}/{1}/{2}", Application.streamingAssetsPath, ASSETBUNDLE_FOLDER, FILE_LIST);
                yield return LoadingAsset(typeof(TextAsset), editorFileList, loaded);
#else
                yield return LoadingAsset(typeof(TextAsset), GetFilePath(FILE_LIST), loaded);
#endif

                var asset = loaded.asset as string;
                var joList = JSON.Load(asset);
                bool dirty = false;
                if (joCached != null) {
                    dirty = MergeBundleList(joList, joCached);
                } else {
                    dirty = InitBundleList(joList);
                }

                if (!dirty && m_RemoteBundles.Count == 0) {
                    // 如果不存在远程资源则清空所有缓存资源
                    if (Directory.Exists(bundleRootPath)) {
                        Directory.Delete(bundleRootPath, true);
                    }
                }

                LoadedBundle.Release(loaded);
                SystemTools.NeedDirectory(bundleRootPath);
                File.WriteAllText(filePath, dirty ? joList.ToJSONString() : asset);
                LogMgr.D("Save '{0}' at '{1}'", FILE_LIST, filePath);
            } else {
                if (InitBundleList(joCached)) {
                    File.WriteAllText(filePath, joCached.ToJSONString());
                }
            }
        }
    }
}
