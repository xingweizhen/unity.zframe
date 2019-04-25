//#define DEST_MATH
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
#if DEST_MATH
using Dest.Math;
#endif

namespace ZFrame.UGUI
{
    public class UIIndicatTarget : MonoBehaviour, ILateTick
    {
        public Camera gameCamera;
        public Transform target;

        [SerializeField] private GameObject m_Root;
        [SerializeField] private RectTransform m_Arrow;

        bool ITickBase.ignoreTimeScale { get { return true; } }
        
#if DEST_MATH
        private Camera m_UICam;
        private AAB2 screenBox;
        private Vector2 screenCtr;
        private CanvasGroup cvGroup;

        private RectTransform m_Rect;
        public RectTransform rectTransform { get { if (!m_Rect) m_Rect = GetComponent<RectTransform>(); return m_Rect; } }
                
        //public float distance { get { return Vector2.Distance(screenCtr, rectHUD.anchoredPosition); } }
        //public bool visible { get { return screenBox.Contains(rectHUD.anchoredPosition); } }

        public Vector3 GetAnchoredPos()
        {
            if (gameCamera && m_UICam && target) {
                var screenPos = gameCamera.WorldToScreenPoint(target.position);
                if (screenPos.z < 0) screenPos *= -1;

                Vector2 anchoredPos;
                var cvRect = rectTransform.parent as RectTransform;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(cvRect, screenPos, m_UICam, out anchoredPos)) {
                    return anchoredPos;
                }
            }

            return Vector3.zero;
        }

        protected virtual void Start()
        {
            m_UICam = gameObject.FindCameraForLayer();

            var cvRect = rectTransform.parent as RectTransform;
            screenBox = new AAB2(cvRect.rect.min, cvRect.rect.max);
            screenCtr = cvRect.rect.center;

            var anchoredPos = GetAnchoredPos();
            cvGroup = GetComponent<CanvasGroup>();
            cvGroup.alpha = screenBox.Contains(anchoredPos) ? 0 : 1;

            m_Root.SetActive(false);
        }

        protected virtual void OnEnable()
        {
            TickManager.Add(this);
        }

        protected virtual void OnDisable()
        {
            TickManager.Remove(this);
        }

        public void Stop()
        {
            this.enabled = false;
            m_Root.SetActive(false);
        }

        public void Resume()
        {
            this.enabled = true;
        }
        
        void ILateTick.LateTick(float deltaTime)
        {
            var anchoredPos = GetAnchoredPos();
            if (!screenBox.Contains(anchoredPos)) {
                if (!m_Root.activeSelf) m_Root.SetActive(true);

                var ray = new Ray2(screenCtr, anchoredPos);
                Ray2AAB2Intr intr;
                if (Intersection.FindRay2AAB2(ref ray, ref screenBox, out intr)) {
                    var nextPos = intr.Point1.Round(10f);

                    var offsetX = rectTransform.sizeDelta.x / 2;
                    var offsetY = rectTransform.sizeDelta.y / 2;
                    nextPos.x = Mathf.Clamp(nextPos.x, screenBox.Min.x + offsetX, screenBox.Max.x - offsetX);
                    nextPos.y = Mathf.Clamp(nextPos.y, screenBox.Min.y + offsetY, screenBox.Max.y - offsetY);

                    rectTransform.anchoredPosition = nextPos;
                    m_Arrow.up = ray.Direction;
                }
                if (cvGroup.alpha < 1) {
                    cvGroup.alpha = Mathf.Clamp01(cvGroup.alpha + deltaTime * 3f);
                }
            } else {
                if (cvGroup.alpha > 0) {
                    cvGroup.alpha = Mathf.Clamp01(cvGroup.alpha - deltaTime * 3f);
                    if (cvGroup.alpha == 0) {
                        m_Root.SetActive(false);
                    }
                }
            }
        }
#else
        void ILateTick.LateTick(float deltaTime) { }
#endif
    }
}
