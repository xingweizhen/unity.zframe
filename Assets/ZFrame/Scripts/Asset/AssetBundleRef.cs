using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Asset
{
    internal class AssetBundleRef : AbstractAssetBundleRef
    {
        private AssetBundle m_Assetbundle;
        private Object[] m_Assets;
        private List<Object> m_CachedAssets = new List<Object>();

        public void Init(string assetbundleName, AssetBundle ab, Object[] assets, LoadMethod method)
        {
            if (method == LoadMethod.Forever) {
                // 对常驻的资源，卸载文件镜像以节省内存占用。
                ab.Unload(false);
            }

            this.name = assetbundleName;
            this.method = method;
            this.m_Assetbundle = ab;
            this.m_Assets = assets;
        }

        public void CacheAssets(Object[] assets)
        {
            m_Assets = assets;
        }

        public override bool IsEmpty()
        {
            return m_Assets == null;
        }

        public override bool hasAsset {
            get { return m_Assetbundle == null || !m_Assetbundle.isStreamedSceneAssetBundle; }
        }

        protected override Object LoadFromBundle(string assetName, System.Type type)
        {
            Object obj = null;
            if (m_Assetbundle && !m_Assetbundle.isStreamedSceneAssetBundle) {
                // 从AssetBundle加载
                var realTime = Time.realtimeSinceStartup;

                if (string.IsNullOrEmpty(assetName)) {
                    // 新AssetBundle下该值无效
                    obj = m_Assetbundle.mainAsset;
                } else {
                    obj = type == null ? m_Assetbundle.LoadAsset(assetName) : m_Assetbundle.LoadAsset(assetName, type);
                }

                if (obj != null) {
                    // 放入缓存，下次加载会更快
                    m_CachedAssets.Add(obj);
                }

                var costTime = Time.realtimeSinceStartup - realTime;
                if (costTime > 0.1f) {
                    LogMgr.W("加载[{0}/{1}]={2}。消耗：{3}秒。", name, assetName, obj, costTime);
                }
            }

            return obj;
        }

        public override Object LoadFromCache(string assetName, System.Type type)
        {
            // 默认资源
            if (string.IsNullOrEmpty(assetName)) {
                if (m_Assets.Length > 0) {
                    if (type == null) return m_Assets[0];

                    foreach (var asset in m_Assets) {
                        if (type.IsInstanceOfType(asset)) return asset;
                    }
                }

                if (m_CachedAssets.Count > 0) {
                    if (type == null) return m_CachedAssets[0];

                    foreach (var asset in m_CachedAssets) {
                        if (type.IsInstanceOfType(asset)) return asset;
                    }
                }
            }

            // 从数组加载
            for (int i = 0; i < m_Assets.Length; ++i) {
                var asset = m_Assets[i];
                if (asset.name == assetName && (type == null || type.IsAssignableFrom(asset.GetType()))) {
                    return asset;
                }
            }


            // 从缓存加载
            for (int i = 0; i < m_CachedAssets.Count; ++i) {
                var asset = m_CachedAssets[i];
                if (asset.name == assetName && (type == null || type.IsAssignableFrom(asset.GetType()))) {
                    return asset;
                }
            }

            return null;
        }

        public override IEnumerator LoadAsync(AsyncLoadingTask task)
        {
            // 先尝试从缓存加载
            task.asset = LoadFromCache(task.assetName, task.assetType);
            if (task.asset == null && m_Assetbundle && !m_Assetbundle.isStreamedSceneAssetBundle) {
                if (string.IsNullOrEmpty(task.assetName)) {
                    task.asset = m_Assetbundle.mainAsset;
                } else {
                    var req = task.assetType != null
                        ? m_Assetbundle.LoadAssetAsync(task.assetName, task.assetType)
                        : m_Assetbundle.LoadAssetAsync(task.assetName);
                    yield return req;
                    task.asset = req.asset;
                }

                var uObj = task.asset as Object;
                if (uObj != null) {
                    // 放入缓存，下次加载会更快
                    m_CachedAssets.Add(uObj);
                }
            }
        }

        protected override void UnloadAssets(bool markAsLoaded = false)
        {
            if (m_Assetbundle) {
                // 场景依赖也属于场景，不能完全卸载。
                var isScene = name.OrdinalStartsWith("scenes");
                m_Assetbundle.Unload(!isScene);
            }

            m_CachedAssets.Clear();
            m_Assets = markAsLoaded ? AssetBundleLoader.I.empty : null;

        }
    }
}
