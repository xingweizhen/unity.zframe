using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.UGUI
{
    [DisallowMultipleComponent]
    public class UIWindow : MonoBehaviour, IPoolable, IEventTransfer
    {
        protected static Dictionary<string, UIWindow> s_OpenWindows = new Dictionary<string, UIWindow>();
        public string cachedName { get; private set; }

        public static UIWindow FindByName(string wndName)
        {
            UIWindow wnd;
            s_OpenWindows.TryGetValue(wndName, out wnd);
            if (wnd == null) {
                s_OpenWindows.Remove(wndName);
                return null;
            }

            return wnd;
        }

        [SerializeField, HideInInspector, AssetRef(bundleOnly = true)]
        private string[] m_PreloadAssets;

        private List<Canvas> m_SubCanvas = new List<Canvas>();

        public string[] preloadAssets { get { return m_PreloadAssets; } }

        private int m_Depth;
        public int depth {
            get { return m_Depth; }
            set {                
                if (value == 0 || m_Depth == 0 || m_Depth != value) {
                    m_Depth = value;
                    ResetCanvasSorting();
                }
            }
        }

        protected virtual void Awake()
        {
            gameObject.NeedComponent(typeof(Canvas));
            gameObject.NeedComponent(typeof(CanvasGroup));

            gameObject.GetComponentsInChildren(m_SubCanvas);
        }

        protected virtual void Start()
        {
            //ResetCanvasSorting();

            if (string.IsNullOrEmpty(cachedName)) cachedName = name;
            if (FindByName(cachedName) == null) {
                s_OpenWindows.Add(cachedName, this);
            }
        }

        private void OnTransformParentChanged()
        {
            ResetCanvasSorting();
        }

        public void ResetCanvasSorting()
        {
            var canvas = (Canvas)gameObject.GetComponent(typeof(Canvas));
            if (canvas.sortingLayerID == 0) {
                canvas.overrideSorting = true;

                int siblingIdx = transform.GetSiblingIndex();
                var sortingOrder = siblingIdx;
                for (var i = siblingIdx - 1; i >= 0; --i) {
                    var preCanvas = transform.parent.GetChild(i).GetComponent(typeof(Canvas)) as Canvas;
                    if (preCanvas && preCanvas.sortingLayerID == 0) {
                        sortingOrder = preCanvas.sortingOrder + 1;
                        break;
                    }
                }
                canvas.sortingOrder = Mathf.Max(depth, sortingOrder);
            }
        }
        
        public virtual void OnRecycle()
        {
            for (int i = 0; i < m_SubCanvas.Count; ++i) {
                m_SubCanvas[i].enabled = false;
            }
            gameObject.SetEnable(typeof(CanvasGroup), false);

            if (!string.IsNullOrEmpty(cachedName))
                s_OpenWindows.Remove(cachedName);
        }

        public virtual void SendEvent(Component sender, UIEvent eventName, string eventParam, object data) { }

        private void OnDestroy()
        {
            if (cachedName != null && FindByName(cachedName)) {
                OnRecycle();
            }
        }

        void IPoolable.OnRestart()
        {
            this.enabled = true;
            gameObject.SetEnable(typeof(CanvasGroup), true);
            for (int i = 0; i < m_SubCanvas.Count; ++i) {
                m_SubCanvas[i].enabled = true;
            }

            Start();            
        }

        void IPoolable.OnRecycle()
        {
            OnRecycle();
            this.enabled = false;

            transform.SetAsLastSibling();
        }
    }
}
