using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Asset
{	
	/// <summary>
	/// 从文件获取的资源引用
	/// </summary>
	internal class FileAssetRef : AbstractAssetBundleRef
	{
		public override BundleType bundleType { get { return BundleType.FileAsset; } }

		private object m_Asset;
		private string m_Text;
        
		public void Init(string bundleName, object asset)
		{
			name = bundleName;
			m_Asset = asset;
		}
        
		protected override void UnloadAssets(bool markAsLoaded = false)
		{
			Object.Destroy(m_Asset as Object);
			m_Asset = null;
		}

		protected override Object LoadFromBundle(string assetName, System.Type type)
		{
			return m_Asset as Object;
		}

		public override Object LoadFromCache(string assetName, System.Type type)
		{
			return m_Asset as Object;
		}

        public override bool Contains(object asset)
        {
            return m_Asset == asset;
        }

        public override IEnumerator LoadAsync(AsyncLoadingTask task)
		{
			yield return null;
			task.asset = m_Asset;
		}

		public override bool IsEmpty()
		{
			return m_Asset == null;
		}
	}
}
