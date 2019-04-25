using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
//using NoToLua = XLua.BlackListAttribute;

namespace ZFrame.UGUI
{
    using Tween;

    public class UISlider : Slider, IStateTransTarget, IEventSender, ITweenable
    {
        public float minLmt = 0, maxLmt = 1;
        public bool antiProgress;

        [SerializeField]
        protected RectTransform m_RadialHandle;

        private Image m_FillImg;

        #region 事件通知
        [SerializeField]
        private EventData m_ValueChanged = new EventData(TriggerType.None, UIEvent.Send);

        IEnumerator<EventData> IEnumerable<EventData>.GetEnumerator()
        {
            yield return m_ValueChanged;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return m_ValueChanged;
        }

        private UIWindow __Wnd;
        protected UIWindow Wnd {
            get {
                if (__Wnd == null) {
                    __Wnd = GetComponentInParent(typeof(UIWindow)) as UIWindow;
                }
                return __Wnd;
            }
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            __Wnd = null;
        }
        
        //[NoToLua]
        public void SetEvent(TriggerType id, UIEvent eventName, string param)
        {
            if (id == TriggerType.None) {
                m_ValueChanged.name = eventName;
                m_ValueChanged.param = param;
            }
        }
        
        #endregion

        protected override void Set(float input, bool sendCallback)
        {
            var oldValue = value;
            base.Set(input, sendCallback);

#if UNITY_EDITOR
            if (!Application.isPlaying || oldValue != value)
#else
            if (oldValue != value)
#endif
            {
                UpdateRadialHandle(value);
            }
        }

        public override float value {
            get {
                return base.value;
            }
            set {
                if (antiProgress) {
                    value = maxValue + minValue - value;
                }
                base.value = value;
            }
        }

        public void SetProgress(float progress)
        {
            var range = maxValue - minValue;
            value = (minLmt + progress * (maxLmt - minLmt)) * range;
        }

        private void Setter(float value)
        {
            Set(value, true);
        }

        private float Getter()
        {
            return value;
        }

        public ZTweener Tween(object from, object to, float duration)
        {
            ZTweener tw = null;

            if (to is float) {
                tw = ZTween.Tween(this, Getter, Setter, (float)to, duration);
                if (from != null) {
                    value = (float)from;
                    tw.StartFrom(value);
                }
            }

            if (tw != null) tw.SetTag(this);
            return tw;
        }

        private void UpdateRadialHandle(float value)
        {
            if (m_FillImg && m_RadialHandle) {
                float maxDegree = 0f;
                switch (m_FillImg.fillMethod) {
                    case Image.FillMethod.Radial90: maxDegree = 90; break;
                    case Image.FillMethod.Radial180: maxDegree = 180; break;
                    case Image.FillMethod.Radial360: maxDegree = 360; break;
                    case Image.FillMethod.Horizontal:
                    case Image.FillMethod.Vertical:
                    default:
                        return;
                }
                float currDegree = maxDegree * normalizedValue;
                if (m_FillImg.fillClockwise) {
                    currDegree = -currDegree;
                }
                m_RadialHandle.localEulerAngles = new Vector3(0, 0, currDegree);
            }
        }

        private void DoValueChanged(float value)
        {
            m_ValueChanged.Send(this, Wnd, NoBoxingFloat.Apply(value));
        }

        protected override void Awake()
        {
            base.Awake();
            onValueChanged.AddListener(DoValueChanged);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_FillImg = fillRect ? fillRect.GetComponent<Image>() : null;
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            this.TryStateTransition((SelectingState)state, instant);
        }
    }
}
