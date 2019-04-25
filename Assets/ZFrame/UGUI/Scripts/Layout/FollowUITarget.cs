using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ZFrame.UGUI
{
    public class FollowUITarget : MonoBehaviour, ILateTick
    {
        /// <summary>
        /// UI target that this object will be positioned above.
        /// </summary>

        public RectTransform target;
        public float depthOfView = 10;

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

		public bool alwaysSmooth = false;
		[SerializeField]
		private float m_SmoothTime = 0;
		private float m_Smooth = 0;
		private Vector3 m_TarPos = Vector3.zero;
		public float smoothTime { get { return m_SmoothTime; } set { m_SmoothTime = value; m_Smooth = 0f; } }

        private Transform m_Trans;
        private Transform cachedTransform { get { if (!m_Trans) m_Trans = transform; return m_Trans; } }

        private RectTransform cvRect;
        private CanvasScaler cvScaler;


        bool mIsVisible = false;

        /// <summary>
        /// Find both the UI camera and the game camera so they can be used for the position calculations
        /// </summary>

        private void Start()
        {
            if (gameCamera == null) {
                Init();
            }
        }

        /// <summary>
        /// Enable or disable child objects.
        /// </summary>

        private void SetVisible(bool val)
        {
            mIsVisible = val;

            for (int i = 0, imax = cachedTransform.childCount; i < imax; ++i) {
                cachedTransform.GetChild(i).gameObject.SetActive(val);
            }
        }

        bool ITickBase.ignoreTimeScale { get { return true; } }
        
        public void LateTick(float delta)
        {
            if (target == null || !target.gameObject.activeInHierarchy || gameCamera == null) return;

            Vector3 pos = uiCamera.WorldToViewportPoint(target.position);
            pos.z = depthOfView + target.anchoredPosition3D.z / cvScaler.referencePixelsPerUnit;

            // Determine the visibility and the target alpha
            bool insideScreen = pos.z > 0 && pos.x > 0f && pos.x < 1f && pos.y > 0f && pos.y < 1f;
            bool isVisible = !disableIfInvisible || insideScreen;

            // Update the visibility flag
            if (mIsVisible != isVisible) SetVisible(isVisible);

            // If visible, update the position
            if (isVisible) {
                var oriPos = cachedTransform.position;
                var tarPos = gameCamera.ViewportToWorldPoint(pos);
				if (smoothTime > 0) {
					bool isEq = oriPos == tarPos;
					if (isEq) {
						cachedTransform.position = tarPos;
						if (!alwaysSmooth) {
							smoothTime = 0f;
						} else {
							m_Smooth = 0f;
						}
					} else {
						if (tarPos != m_TarPos) {                            
							m_TarPos = tarPos;
						}
						cachedTransform.position = Vector3.Lerp(oriPos, tarPos, m_Smooth / smoothTime);
						m_Smooth += Time.deltaTime;
                    }
                } else {
                    cachedTransform.position = tarPos;
                }
            }
            OnUpdate(isVisible);
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

        public RectTransform followTarget { set { target = value; Init(); } get { return target; } }
        public void Init()
        {
            if (target != null) {
                uiCamera = target.gameObject.FindCameraForLayer();
                gameCamera = gameObject.FindCameraForLayer();
                cvScaler = target.GetComponentInParent<CanvasScaler>();
                m_Smooth = 0;
                enabled = true;
                LateTick(0);
            } else {
                enabled = false;
            }
        }
    }
}