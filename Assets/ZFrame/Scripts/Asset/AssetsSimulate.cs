using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
namespace ZFrame.Asset
{
    using UnityEditor;

    public sealed class AssetsSimulate : AssetLoader
    {
        private class SimAssetBundle : AbstractAssetBundleRef
        {
            private class SimAsset
            {
                public string name { get; private set; }
                public string path { get; private set; }
                private Dictionary<System.Type, Object> m_ObjRefs;
                private Object m_MainAsset;
                public Object mainAsset {
                    get {
                        if (m_MainAsset == null) {
                            m_MainAsset = AssetDatabase.LoadMainAssetAtPath(path);
                            if (m_MainAsset) {
                                var type = m_MainAsset.GetType();
                                if (m_ObjRefs.ContainsKey(type)) {
                                    m_ObjRefs[type] = m_MainAsset;
                                } else {
                                    m_ObjRefs.Add(type, m_MainAsset);
                                }
                            }
                        }
                        return m_MainAsset;
                    }
                }
                public SimAsset(string path)
                {
                    this.path = path;
                    this.name = Path.GetFileNameWithoutExtension(path);
                }
                public Object Load(System.Type type)
                {
                    if (m_ObjRefs == null) {
                        m_ObjRefs = new Dictionary<System.Type, Object>();
                    }
                    Object objRef;
                    if (type == null) {
                        objRef = mainAsset;
                    } else {
                        if (!m_ObjRefs.TryGetValue(type, out objRef)) {
                            objRef = AssetDatabase.LoadAssetAtPath(path, type);
                            if (objRef) m_ObjRefs.Add(type, objRef);
                        }
                    }
                    return objRef;
                }

                public void Unload()
                {
                    if (m_ObjRefs != null) {
                        m_ObjRefs.Clear();
                    }
                }

                public override string ToString()
                {
                    return string.Format("[({0}), ({1})]", name, path);
                }
            }

            private bool m_Loaded = false;
            public bool loaded {
                get { return m_Loaded; }
                set {
                    m_Loaded = value;
                    if (value) lastLoaded = Time.realtimeSinceStartup;
                }
            }

            private List<SimAsset> __AllAssets;
            private List<SimAsset> m_AllAssets {
                get {
                    if (__AllAssets == null) {
                        __AllAssets = new List<SimAsset>();
                        var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(name);
                        if (assetPaths != null) {
                            for (int i = 0; i < assetPaths.Length; ++i) {
                                var asset = new SimAsset(assetPaths[i]);
                                __AllAssets.Add(asset);
                            }
                        }
                    }
                    return __AllAssets;
                }
            }

            public SimAssetBundle(string assetbundleName)
            {
                this.name = assetbundleName;
            }

            public override bool IsEmpty()
            {
                return !loaded;
            }

            protected override Object LoadFromBundle(string assetName, System.Type type)
            {
                SimAsset asset = null;
                if (string.IsNullOrEmpty(assetName)) {
                    if (m_AllAssets.Count > 0) {
                        if (type == null) {
                            asset = m_AllAssets[0];
                        } else {
                            for (int i = 0; i < m_AllAssets.Count; ++i) {
                                var obj = m_AllAssets[i].Load(type);
                                if (obj) return obj;
                            }
                        }
                    }
                }

                for (int i = 0; i < m_AllAssets.Count; ++i) {
                    if (m_AllAssets[i].name == assetName) {
                        asset = m_AllAssets[i];
                        break;
                    }
                }

                return asset != null ? asset.Load(type) : null;
            }

            public override Object LoadFromCache(string assetName, System.Type type)
            {
                return LoadFromBundle(assetName, type);
            }

            public override bool Contains(object asset)
            {
                var uObj = asset as Object;
                if (__AllAssets != null && uObj) {
                    for (int i = 0; i < __AllAssets.Count; ++i) {
                        if (m_AllAssets[i].mainAsset == uObj) {
                            asset = m_AllAssets[i];
                            break;
                        }
                    }
                }

                return false;
            }

            public override IEnumerator LoadAsync(AsyncLoadingTask task)
            {
                task.asset = LoadFromCache(task.assetName, task.assetType);
                if (task.asset == null) {
                    yield return null;
                    task.asset = LoadFromBundle(task.assetName, task.assetType);
                }
            }

            protected override void UnloadAssets(bool markAsLoaded = false)
            {
                if (__AllAssets != null) {
                    foreach (var asset in __AllAssets) {
                        asset.Unload();
                    }
                }
                loaded = markAsLoaded;
            }

            public string GetAssetPath(string assetName)
            {
                for (int i = 0; i < m_AllAssets.Count; ++i) {
                    var asset = m_AllAssets[i];
                    if (asset.name == assetName) {
                        return asset.path;
                    }
                }
                return null;
            }

            public void SetMethod(LoadMethod method)
            {
                this.method = method;
            }

            public string ToDetailString()
            {
                var strbld = new System.Text.StringBuilder(string.Format("[SimAB: {0}]\n", name));
                foreach (var asset in m_AllAssets) {
                    strbld.AppendLine(asset.ToString());
                }
                strbld.AppendLine();
                return strbld.ToString();
            }
        }

        private Dictionary<string, SimAssetBundle> m_AllAssetBundles;

        protected override void Awaking()
        {
            base.Awaking();

            m_AllAssetBundles = new Dictionary<string, SimAssetBundle>();
            // 分析资源结构，做虚拟加载使用
            var assetNames = AssetDatabase.GetAllAssetBundleNames();
            foreach (var ab in assetNames) {
                m_AllAssetBundles.Add(ab, new SimAssetBundle(ab));
            }
        }

        protected override void ReleaseAbr(AbstractAssetBundleRef ab)
        {

        }

        protected override BundleStat GetBundleStat(string bundleName, out string md5)
        {
            md5 = null;
            if (string.IsNullOrEmpty(bundleName)) return BundleStat.Local;
            return m_AllAssetBundles.ContainsKey(bundleName) ? BundleStat.Local : BundleStat.NotExist;
        }

        protected override AbstractAssetBundleRef PerformTask(System.Type type, string bundleName, string assetName, LoadMethod method)
        {
            SimAssetBundle bundle = null;
            if (m_AllAssetBundles.TryGetValue(bundleName, out bundle)) {
                if (!bundle.loaded) {
                    this.LogFormat(LogLevel.W, "[Asset] [SYNC] 加载资源：{0}|{1}", bundleName, (AssetOp)method);
                    bundle.loaded = true;
                }

                bundle.SetMethod(method);
            } else {
                this.LogFormat(LogLevel.E, "资源不存在：{0}", bundleName);
            }
            return bundle;
        }

        protected override IEnumerator PerformTask(AsyncLoadingTask task)
        {
            OnLoading(task.bundleName, 0);
            SimAssetBundle bundle;
            if (m_AllAssetBundles.TryGetValue(task.bundleName, out bundle)) {
                if (!bundle.loaded) {
                    yield return null;
                    bundle.loaded = true;
                }

                bundle.SetMethod(task.method);
                task.bundle = bundle;
            } else {
                this.LogFormat(LogLevel.E, "资源不存在：{0}", task.bundleName);
            }
            OnLoading(task.bundleName, 1);
        }

#if false
        protected override IEnumerator PerformMultiTasks(IList<AsyncLoadingTask> tasks, IAssetProgress prog)
        {
            var time = Time.realtimeSinceStartup;
            var strbld = new System.Text.StringBuilder();

            var progress = 0f;
            var p = 1f / tasks.Count;
            while (tasks.Count > 0) {
                var task = tasks[0];
                SimAssetBundle bundle;
                if (m_AllAssetBundles.TryGetValue(task.bundleName, out bundle)) {
                    if (!bundle.loaded) {
                        strbld.AppendLine(task.ToString());
                        yield return null;
                        bundle.loaded = true;
                        bundle.SetMethod(task.method);
                        task.bundle = bundle;
                        FinishLoadindBundle(task);
                        if (!LoadAssetFromCache(task)) {
                            yield return LoadAssetFromBundle(task);
                        }
                    }
                    task.OnBundleLoaded();
                    if (prog != null) {
                        prog.OnBundleLoaded(task.bundleName, task.bundle);
                    }
                }
                progress += p;
                if (prog != null) prog.SetProgress(progress);

                AsyncLoadingTask.Release(task);
                tasks.RemoveAt(0);
            }

            if (progress < 1) {
                progress = 1;
                if (prog != null) prog.SetProgress(progress);
            }

            LogMgr.Log(string.Format("加载资源耗时{0}秒：\n{1}", Time.realtimeSinceStartup - time, strbld.ToString()));
        }
#endif

        public override AsyncOperation LoadLevelAsync(string path, LoadSceneMode mode = LoadSceneMode.Single)
        {
            // 解析出资源包和资源对象
            string assetbundleName, assetName;
            GetAssetpath(path, out assetbundleName, out assetName);

            if (assetbundleName == string.Empty) {
                return SceneManager.LoadSceneAsync(path, mode);
            }

            SimAssetBundle assetbundle;
            if (m_AllAssetBundles.TryGetValue(assetbundleName, out assetbundle)) {
                var assetPath = assetbundle.GetAssetPath(assetName);
                if (assetPath != null) {
#if UNITY_2018_4_OR_NEWER
                    return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(assetPath, new LoadSceneParameters(mode));
#else
                    if (mode == LoadSceneMode.Single) {
                        return EditorApplication.LoadLevelAsyncInPlayMode(assetPath);
                    } else if (mode == LoadSceneMode.Additive) {
                        return EditorApplication.LoadLevelAdditiveAsyncInPlayMode(assetPath);
                    } else {
                        LogMgr.E("错误的场景加载模式：{0}", mode);
                    }
#endif
                }

            }
            this.LogFormat(LogLevel.E, string.Format("场景未标志为<AssetBundle>：{0}({1}, {2})", path, assetbundleName, assetName));
            return null;
        }
    }
}
#endif