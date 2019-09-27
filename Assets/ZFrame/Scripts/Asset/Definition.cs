using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Asset
{
    public delegate void DelegateObjectLoaded(string assetPath, object loadedObj, object param);

    public delegate void DelegateAssetBundleLoaded(string bundleName, AbstractAssetBundleRef ab);

    public delegate void TaskInitializer(AsyncLoadingTask task, string bundleName, string assetName, 
        LoadMethod method, DelegateAssetBundleLoaded onLoaded, System.Type type, DelegateObjectLoaded onObjectLoaded, object param);

    public enum BundleType
    {
        None,
        AssetBundle,
        FileAsset,
    }

    [System.Flags]
    public enum AssetOp
    {
        None = 0,

        /// <summary>
        /// Call `LoadAllAssets` after AssetBundle is loaded. 
        /// </summary>
        Cache = 1,

        /// <summary>
        /// DO NOT Call `Unload(true)` while scene changing. 
        /// </summary>
        Keep = 1 << 1,

        /// <summary>
        /// If flags contains `Cache`，Call `Unload(false)` after AssetBundle is loaded.
        /// </summary>
        Unload = 1 << 2,

        /// <summary>
        /// The assetBundle should load first than others.
        /// </summary>
        FirstLoad = 1 << 3,
    }

    public enum LoadMethod
    {
        /// <summary>
        /// 不读取资源，可以被卸载
        /// </summary>
        Default = 0,

        /// <summary>
        /// 读取并缓存资源，可以被卸载
        /// </summary>
        Cache = AssetOp.Cache,

        /// <summary>
        /// 读取并缓存资源，然后释放内存镜像，资源不可被卸载
        /// </summary>        
        Forever = AssetOp.Keep | AssetOp.Cache | AssetOp.Unload,

        /// <summary>
        /// 仅加载，不可被卸载
        /// </summary>
        Always = AssetOp.Keep,
    }
    
    [System.Flags]
    public enum BundleStat
    {
        NotExist = 0,
        Local = 1,
        Remote = 2,
        NotUpdate = 3,
    }

    [System.Flags]
    public enum LoadType
    {
        IDLE = 0,
        Load = 1,
        Download = 2,
        DownloadAndLoad = 3,
    }

    public static class Extension
    {
        public static bool HasOp(this LoadMethod self, AssetOp op)
        {
            return ((int)self & (int)op) != 0;
        }
    }


    public interface IAssetProgress
    {
        void SetProgress(float progress);
        void OnBundleLoaded(string bundleName, AbstractAssetBundleRef bundle);
    }

    public static class Prefs
    {
#if UNITY_EDITOR
        public const string kPrintLuaLoading = "zframe.printLuaLoading";
        public const string kUseLuaAssetBundle = "zframe.useLuaAssetBundle";
        public const string kUseAssetBundleLoader = "zframe.useAssetBundleLoader";
#endif
    }
}