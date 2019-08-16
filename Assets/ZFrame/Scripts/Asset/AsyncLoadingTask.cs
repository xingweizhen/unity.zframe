using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Asset
{

    /// <summary>
    /// 资源包/资源加载任务
    /// </summary>
    public class AsyncLoadingTask : Poolable<AsyncLoadingTask>
    {        
        public static void Cancel(AsyncLoadingTask task)
        {   
            if (task.loadingCanceled != null) {
                task.loadingCanceled(task);
            }
            Release(task);
        }
        
        public AsyncOperation async;

        public AsyncLoadingTask SetInfo(BundleType bundleType, string bundleName, string assetName)
        {
            assetPath = string.Concat(bundleName, "/", assetName);
            if (bundleType == BundleType.AssetBundle && !string.IsNullOrEmpty(bundleName)) {
                bundleName = bundleName.ToLower();
            }

            this.bundleType = bundleType;
            this.bundleName = bundleName;           
            this.assetName = assetName;

            return this;
        }

        public BundleType bundleType { get; private set; }
        public string assetPath { get; private set; }
        public AsyncLoadingTask SetPath(string assetPath)
        {
            this.assetPath = assetPath;

            string bundle, asset;
            bundleType = AssetLoader.GetAssetpath(assetPath, out bundle, out asset);
            bundleName = bundle;
            assetName = asset;

#if UNITY_EDITOR
            TrackWillLoadAsset(bundleName);
#endif
            return this;
        }

        #region 资源包参数
        public string bundleName { get; private set; }
        public event DelegateAssetBundleLoaded bundleLoaded;
        public LoadMethod method { get; private set; }
        public AbstractAssetBundleRef bundle;
        public string bundleId;
        public AsyncLoadingTask SetBundle(LoadMethod method, DelegateAssetBundleLoaded onLoaded)
        {
            this.method = method;
            this.bundleLoaded = onLoaded;

            return this;
        }
        public void OnBundleLoaded()
        {
            if (bundleLoaded != null) {
                LoadedCallbacks.Enqueue(BundleLoadedCallback.New(bundleLoaded, bundleName, bundle));
            }
        }
        #endregion

        #region 资源参数
        public System.Type assetType;
        public string assetName { get; private set; }
        public event DelegateObjectLoaded assetLoaded;
        public object param { get; private set; }
        public object asset;
        public bool needsAsset { get { return assetType != null || !string.IsNullOrEmpty(assetName); } }
        public AsyncLoadingTask SetAsset(System.Type type, DelegateObjectLoaded onLoaded, object param)
        {
            this.assetType = type;
            this.assetLoaded = onLoaded;
            this.param = param;

            return this;
        }
        public void OnAssetLoaded()
        {
            if (assetLoaded != null) {
                LoadedCallbacks.Enqueue(AssetLoadedCallback.New(assetLoaded, assetPath, asset, param));
            }
        }

        public static Queue<ILoadedCallback> LoadedCallbacks = new Queue<ILoadedCallback>();
        #endregion

        /// <summary>
        /// 是否强制从原始资源加载
        /// </summary>
        public bool forcedStreaming;

        public bool allowCache { get { return method.HasOp(AssetOp.Cache); } }
        public bool allowUnload { get { return method.HasOp(AssetOp.Keep); } }

        public event System.Action<AsyncLoadingTask> loadingCanceled;

        protected override void OnRelease()
        {
            bundleType = BundleType.None;
            loadType = LoadType.IDLE;
            async = null; loadingCanceled = null;
            assetPath = null; bundleName = null; bundleLoaded = null; bundle = null;
            assetName = null; assetType = null; assetLoaded = null; asset = null;
            forcedStreaming = false;
            downloadProgress = 0;
            loadingProgress = 0;
            hangUntilTime = 0;
        }

        /// <summary>
        /// 在下载后，是否自动加载
        /// </summary>
        public LoadType loadType;

        public bool IsDownloading()
        {
            return (loadType & LoadType.Download) != 0;
        }

        public float downloadProgress;
        public float loadingProgress;
        public float hangUntilTime;

        public override string ToString()
        {
            return string.Format("[Task:{0}@{1}|{2})]", assetType, bundleName, method);
        }

        #region 追踪加载顺序
#if UNITY_EDITOR
        private static HashSet<string> m_TrackAssets = new HashSet<string>();
        private static void TrackWillLoadAsset(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName)) return;

            if (m_TrackAssets.Add(bundleName)) {
                System.IO.File.AppendAllText("Temp/trackasset.txt", 
                    string.Format("{0}ms,{1}\r\n",Mathf.Round(Time.realtimeSinceStartup * 1000), bundleName));
            }
        }
#endif
        #endregion
    }

    public class LoadedBundle : Poolable<LoadedBundle>
    {
        public AbstractAssetBundleRef bundle;
        public object asset;

        protected override void OnGet()
        {
            UnityEngine.Assertions.Assert.IsNull(bundle);
            UnityEngine.Assertions.Assert.IsNull(asset);
        }

        protected override void OnRelease()
        {
            bundle = null;
            asset = null;
        }
    }

    public interface ILoadedCallback
    {
        string path { get; }
        void ExecOnLoaded();
    }

    public class BundleLoadedCallback : ILoadedCallback
    {
        private DelegateAssetBundleLoaded m_Loaded;
        private string m_BundleName;
        private AbstractAssetBundleRef m_Bundle;

        string ILoadedCallback.path {
            get { return m_BundleName; }
        }
        
        void ILoadedCallback.ExecOnLoaded()
        {
            try {
                m_Loaded(m_BundleName, m_Bundle);
            } catch (System.Exception e) {
                LogMgr.E("Loaded {0}: {1}\n{2}", m_Bundle, e.Message, e.StackTrace);
            }
            
            Release(this);
        }
        
        private static Stack<BundleLoadedCallback> _pool = new Stack<BundleLoadedCallback>();

        public static BundleLoadedCallback New(DelegateAssetBundleLoaded loaded, string bundleName, AbstractAssetBundleRef bundle)
        {
            var elm = _pool.Count > 0 ? _pool.Pop() : new BundleLoadedCallback();
            elm.m_Loaded = loaded;
            elm.m_BundleName = bundleName;
            elm.m_Bundle = bundle;
            return elm;
        }

        public static void Release(BundleLoadedCallback item)
        {
            _pool.Push(item);
        }
    }
    
    public class AssetLoadedCallback : ILoadedCallback
    {
        private DelegateObjectLoaded m_Loaded;
        private string m_Path;
        private object m_Asset;
        private object m_Param;

        string ILoadedCallback.path {
            get { return m_Path; }
        }
        
        void ILoadedCallback.ExecOnLoaded()
        {
            try {
                m_Loaded(m_Path, m_Asset, m_Param);
            } catch (System.Exception e) {
                LogMgr.E("Loaded {0}: {1}\n{2}", m_Path, e.Message, e.StackTrace);
            }
            
            Release(this);
        }
        
        private static Stack<AssetLoadedCallback> _pool = new Stack<AssetLoadedCallback>();

        public static AssetLoadedCallback New(DelegateObjectLoaded loaded, string path, object asset, object param)
        {
            var elm = _pool.Count > 0 ? _pool.Pop() : new AssetLoadedCallback();
            elm.m_Loaded = loaded;
            elm.m_Path = path;
            elm.m_Asset = asset;
            elm.m_Param = param;
            return elm;
        }

        public static void Release(AssetLoadedCallback item)
        {
            _pool.Push(item);
        }
    }
}
