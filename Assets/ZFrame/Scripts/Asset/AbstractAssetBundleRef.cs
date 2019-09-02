using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace ZFrame.Asset
{
    /// <summary>
    /// 抽象的AssetBundle引用，管理已加载的AssetBundle
    /// </summary>
    public abstract class AbstractAssetBundleRef : System.IComparable<AbstractAssetBundleRef>
    {
        public virtual BundleType bundleType { get { return BundleType.AssetBundle; } } 

        /// <summary>
        /// AssetBundle名，唯一
        /// </summary>
        private string m_Name;
        public string name {
            get { return m_Name; }
            protected set {
                m_Name = value;
                group = Path.GetFileName(SystemTools.GetDirPath(m_Name));
            }
        }

        /// <summary>
        /// AssetBundle组名
        /// </summary>
        public string group { get; private set; }

        /// <summary>
        /// 资源的保存方法
        /// </summary>
        public LoadMethod method { get; protected set; }
        public bool allowUnload { get { return ((AssetOp)method & AssetOp.Keep) == 0; } }

        /// <summary>
        /// 上一次加载的时间
        /// </summary>
        public float lastLoaded { get; set; }

        public virtual bool hasAsset { get { return true; } }
        protected abstract void UnloadAssets(bool markAsLoaded = false);
        public abstract bool IsEmpty();        
        protected abstract Object LoadFromBundle(string assetName, System.Type type);
        public abstract Object LoadFromCache(string assetName, System.Type type);
        public abstract bool Contains(object asset);

        public Object Load(string assetName, System.Type type)
        {
            var obj = LoadFromCache(assetName, type);
            if (obj == null) {
                obj = LoadFromBundle(assetName, type);
            }
            return obj;
        }
        public abstract IEnumerator LoadAsync(AsyncLoadingTask task);

        public virtual void LoadAllAssetsAsync() { }

        public void Release()
        {
            UnloadAssets(true);
        }

        public void Unload(bool forced = false)
        {
            if (allowUnload || forced) {
                AssetLoader.Info("{0}Unload {1}", forced ? "Forced " : "", this);
                lastLoaded = 0;
                UnloadAssets();
            } else {
                AssetLoader.Info("Skip {0}", this.ToString());
            }
        }

        public override string ToString()
        {
            return string.Format("[<{0}>{1}|[{2}] @{3:0.0000}]",
                group, Path.GetFileNameWithoutExtension(name), (AssetOp)method, lastLoaded);
        }

        public virtual int CompareTo(AbstractAssetBundleRef other)
        {
            return lastLoaded < other.lastLoaded ? -1 : 1;
        }
    }

}