using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Asset
{
	public class AsyncMultitasking : Poolable<AsyncMultitasking>
	{
		public static AsyncMultitasking Get(DelegateObjectLoaded onLoaded, object param, IAssetProgress progress)
		{
			var mt = Get();
			mt.m_Loaded = onLoaded;
			mt.m_Param = param;
			mt.m_Progress = progress;
			return mt;
		}

		public AsyncMultitasking()
		{
            m_OnBundleLoading = (task) => {
                if (m_Progress != null) {
                    m_Progress.SetProgress((m_Count - m_Tasks.Count + task.loadingProgress) / m_Count);
                }
            };

			m_OnBundleLoaded = (name, ab) => {
				if (m_Progress != null) {
					m_Progress.OnBundleLoaded(name, ab);
				}
			};
			
			m_OnAssetLoaded = (a, o, p) => {
                if (m_Tasks.Count > 0) {
                    m_Tasks.Remove(a);
                    if (m_Progress != null) {
                        m_Progress.SetProgress((m_Count - m_Tasks.Count) / (float)m_Count);
                    }

                    if (m_Tasks.Count == 0) {
                        Release(this);
                    }
                }
			};
			
			m_OnTaskCancel = task => {
				if (m_Loaded != null) {
					// 被取消，不会执行回调
					m_Loaded = null;
					Release(this);
				}
			};
		}

        private readonly System.Action<AsyncLoadingTask> m_OnBundleLoading;
        private readonly DelegateAssetBundleLoaded m_OnBundleLoaded;
		private readonly DelegateObjectLoaded m_OnAssetLoaded;        
        private readonly System.Action<AsyncLoadingTask> m_OnTaskCancel;
		
		private HashSet<string> m_Tasks = new HashSet<string>();
		private DelegateObjectLoaded m_Loaded;
		private object m_Param;
		private IAssetProgress m_Progress;
		private int m_Count;
		
		public LoadType loadType { get; private set; }

		protected override void OnRelease()
		{
			if (m_Loaded != null) {
				var cb = AssetLoadedCallback.New(m_Loaded, string.Empty, null, m_Param);
				AsyncLoadingTask.LoadedCallbacks.Enqueue(cb);
			}
			m_Tasks.Clear();
			m_Loaded = null;
			m_Param = null;
            m_Progress = null;
			loadType = LoadType.IDLE;
		}

		public void AddTask(AsyncLoadingTask task)
		{
			if (string.IsNullOrEmpty(task.assetPath)) {
				Log.Format(LogLevel.W, "{0}: NullOrEmpty asset path", task);
				return;
			}

			if (!m_Tasks.Add(task.assetPath)) {
				return;
			}

            task.bundleLoading += m_OnBundleLoading;
			task.bundleLoaded += m_OnBundleLoaded;
			task.assetLoaded += m_OnAssetLoaded;
			task.loadingCanceled += m_OnTaskCancel;
			loadType |= task.loadType;
		}

		public void ConfirmTask()
		{
			m_Count = m_Tasks.Count;
			if (m_Count == 0) {
				Release(this);
			}
		}

        public bool IsProcessing()
        {
            return m_Tasks.Count > 0;
        }

		public bool HasDownload()
		{
			return (loadType & LoadType.Download) != 0;
		}
	}
}

