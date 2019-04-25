using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Asset
{
    public delegate void DelegateObjectLoaded(string assetPath, object loadedObj, object param);

    public delegate void DelegateAssetBundleLoaded(string bundleName, AbstractAssetBundleRef ab);

    public delegate void TaskInitializer(AsyncLoadingTask task, string path, LoadMethod method, DelegateAssetBundleLoaded onLoaded, System.Type type, DelegateObjectLoaded onObjectLoaded, object param);

    public enum BundleType
    {
        None,
        AssetBundle,
        FileAsset,
    }

    [System.Flags]
    public enum AssetOp
    {
        Cache = 1,
        Keep = 1 << 1,
    }

    public enum LoadMethod
    {
        /// <summary>
        ///  不读取资源，可以被卸载
        /// </summary>
        Default = 0,

        /// <summary>
        /// 读取并缓存资源，可以被卸载
        /// </summary>
        Cache = AssetOp.Cache,

        /// <summary>
        /// 读取并缓存资源，不可被卸载
        /// </summary>        
        Forever = AssetOp.Keep | AssetOp.Cache,

        /// <summary>
        /// 仅加载，不可被卸载
        /// </summary>
        Always = AssetOp.Keep,
    }

    public interface IAssetProgress
    {
        void SetProgress(float progress);
        void OnBundleLoaded(string bundleName, AbstractAssetBundleRef bundle);
    }
}