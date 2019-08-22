using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Asset
{
    internal class AssetBundleRef : AbstractAssetBundleRef
    {
        private class AssetRef
        {
            public readonly Object asset;
            private readonly string m_Name;

            public AssetRef(Object asset)
            {
                m_Name = asset.name;
                this.asset = asset;
            }

            public bool IsName(string name)
            {
                return m_Name == name;
            }

            public bool IsType(System.Type type)
            {
                return type.IsAssignableFrom(asset.GetType());
            }
        }

        private AssetBundle m_Assetbundle;
        private List<AssetRef> m_CachedAssets = new List<AssetRef>();
        private bool m_Loaded;

        public void Init(string assetbundleName, AssetBundle ab, Object[] assets, LoadMethod method)
        {
            if (method.HasOp(AssetOp.Unload)) {
                // 对常驻的资源，卸载文件镜像以节省内存占用。
                ab.Unload(false);
            }

            this.name = assetbundleName;
            this.method = method;
            this.m_Assetbundle = ab;
            if (assets != null) {
                for (int i = 0; i < assets.Length; i++) {
                    if (assets[i] != null) {
                        m_CachedAssets.Add(new AssetRef(assets[i]));
                    }
                }
            }
            m_Loaded = true;
        }

        public override bool IsEmpty()
        {
            return !m_Loaded;
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
                    obj = null;
                } else {
                    obj = type == null ? m_Assetbundle.LoadAsset(assetName) : m_Assetbundle.LoadAsset(assetName, type);
                }

                if (obj != null) {
                    // 放入缓存，下次加载会更快
                    m_CachedAssets.Add(new AssetRef(obj));
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
                if (m_CachedAssets.Count > 0) {
                    if (type == null) return m_CachedAssets[0].asset;

                    foreach (var assetRef in m_CachedAssets) {
                        if (type.IsInstanceOfType(assetRef.asset)) return assetRef.asset;
                    }
                }
            } else {
                // 从缓存加载
                for (int i = 0; i < m_CachedAssets.Count; ++i) {
                    var assetRef = m_CachedAssets[i];
                    if (assetRef.IsName(assetName) && (type == null || assetRef.IsType(type))) {
                        return assetRef.asset;
                    }
                }
            }

            return null;
        }

        public override bool Contains(object asset)
        {
            var uObj = asset as Object;
            if (uObj == null) return false;
            
            for (int i = 0; i < m_CachedAssets.Count; ++i) {
                if (m_CachedAssets[i].asset == uObj) return true;
            }
            return false;
        }

        public override IEnumerator LoadAsync(AsyncLoadingTask task)
        {
            // 先尝试从缓存加载
            task.asset = LoadFromCache(task.assetName, task.assetType);
            if (task.asset == null && m_Assetbundle && !m_Assetbundle.isStreamedSceneAssetBundle) {
                if (string.IsNullOrEmpty(task.assetName)) {
                    task.asset = null;
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
                    m_CachedAssets.Add(new AssetRef(uObj));
                }
            }
        }

        protected override void UnloadAssets(bool markAsLoaded = false)
        {
            if (m_Assetbundle) {
                // 场景依赖也属于场景，不能完全卸载。
                // var isScene = name.StartsWith("scenes");                
                m_Assetbundle.Unload(!m_Assetbundle.isStreamedSceneAssetBundle);
            }

            m_CachedAssets.Clear();
            if (!markAsLoaded) m_Loaded = false;
        }
    }
}
