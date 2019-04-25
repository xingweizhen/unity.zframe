using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Attaching this script to an object will make it visibly follow another object, even if the two are using different cameras to draw them.
/// 改自NGUI的脚本，以适配Unity5中的官方UI系统使用
/// </summary>
namespace ZFrame.UGUI
{
    /// <summary>
    /// UI对象跟随游戏对象
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIFollowTarget : MonoBehaviour, ILateTick
    {
        /// <summary>
        /// 3D target that this object will be positioned above.
        /// </summary>

        public Transform target;
        public Vector3 targetPos;

		[SerializeField]
		private Vector3 m_TargetOffset;

        /// <summary>
        /// Game camera to use.
        /// </summary>

        public Camera gameCamera;

        /// <summary>
        /// UI camera to use.
        /// </summary>

        public Camera uiCamera;

        /// <summary>
        /// Whether the children will be disabled when this object is no longer visible.
        /// </summary>

        public bool disableIfInvisible = false;

        [System.Serializable]
        public class FollowTargetEvent : UnityEvent<bool> { public FollowTargetEvent() { } }        
        public FollowTargetEvent onLeaveScreen = new FollowTargetEvent();

        private RectTransform m_Rect;
        private RectTransform rectTransform { get { if (!m_Rect) m_Rect = GetComponent<RectTransform>(); return m_Rect; } }

        private RectTransform cvRect;

        bool mInsideScreen = false;
        bool mIsVisible = false;

        public virtual Vector3 GetTargetPos()
        {
			var pos = target ? target.position : targetPos;
			return pos + m_TargetOffset;
        }
        
        /// <summary>
        /// Find both the UI camera and the game camera so they can be used for the position calculations
        /// </summary>

        private void Start()
        {
            Init();
        }

        private void SetInside(bool val)
        {
            mInsideScreen = val;
            if (onLeaveScreen != null) {
                onLeaveScreen.Invoke(!val);
            }
        }

        /// <summary>
        /// Enable or disable child objects.
        /// </summary>

        private void SetVisible(bool val)
        {
            mIsVisible = val;
            rectTransform.gameObject.SetActive(val);
        }

        bool ITickBase.ignoreTimeScale { get { return true; } }

        public void LateTick(float delta)
        {
            if (gameCamera == null) return;
            
            if (uiCamera) {
                Vector3 pos = gameCamera.WorldToScreenPoint(GetTargetPos());
                
                // Determine the visibility and the target alpha
                bool insideScreen =  pos.z > 0 && gameCamera.pixelRect.Contains(pos);
                bool isVisible = !disableIfInvisible || insideScreen;

                if (mInsideScreen != insideScreen) SetInside(insideScreen);
                // Update the visibility flag
                if (mIsVisible != isVisible) SetVisible(isVisible);

                // If visible, update the position
                if (isVisible) {
                    if (pos.z < 0) pos *= -1;
                    Vector2 anchoredPos;
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(cvRect, pos, uiCamera, out anchoredPos)) {
                        rectTransform.anchoredPosition = anchoredPos;
                    }
                }
                OnUpdate(isVisible);
            } else {
                Vector3 pos = gameCamera.WorldToScreenPoint(GetTargetPos());

                bool isVisible = true;
                if (mIsVisible != isVisible) SetVisible(isVisible);

                if (isVisible) {
                    rectTransform.position = pos.SetZ(0);
                }
                OnUpdate(isVisible);
            }
        }

        private void OnEnable()
		{
		    LateTick(0);
		    TickManager.Add(this);
		}

        private void OnDisable()
        {
            TickManager.Remove(this);
        }

        /// <summary>
        /// Custom update function.
        /// </summary>

        protected virtual void OnUpdate(bool isVisible) { }
        
        private void Init()
        {
            if (target && gameCamera == null) {
                gameCamera = target.gameObject.FindCameraForLayer();
            }

            if (gameCamera) {
                var canvas = rectTransform.GetComponentInParent<Canvas>();
                if (canvas) {
                    uiCamera = canvas.worldCamera;
                    if (!uiCamera) uiCamera = gameObject.FindCameraForLayer();
                    cvRect = rectTransform.parent as RectTransform;

                    enabled = true;
                    LateTick(0);
                    return;
                }
            }

            enabled = false;
        }

        public void SetFollow(Vector3 pos, Camera cam)
        {
            target = null;
            targetPos = pos;
            gameCamera = cam;
            Init();
        }

        public void SetFollow(Transform target, Camera cam)
        {
            this.target = target;
            if (cam != null) {
                gameCamera = cam;
            }
            Init();
        }

        public static UIFollowTarget Follow(GameObject self, Transform target, Camera cam = null)
        {
            var follow = self.NeedComponent<UIFollowTarget>();
            follow.SetFollow(target, cam);
            return follow;
        }

        public static UIFollowTarget Follow(GameObject self, Vector3 pos, Camera cam)
        {
            var follow = self.NeedComponent<UIFollowTarget>();
            follow.SetFollow(pos, cam);
            return follow;
        }
    }
}