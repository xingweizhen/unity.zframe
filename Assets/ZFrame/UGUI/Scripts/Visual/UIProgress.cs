using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
//using NoToLua = XLua.BlackListAttribute;

namespace ZFrame.UGUI
{
    using Tween;
    using Direction = Slider.Direction;
    using SliderEvent = Slider.SliderEvent;

    public class UIProgress : UIBehaviour, IEventSender, ITweenable, ITweenable<float>, IEventSystemHandler
    {
        public readonly float minValue = 0f;
        public readonly float maxValue = 1f;

        public int maxLayer = 1;

        [SerializeField]
        private Color[] m_LayerColors = {
            Color.white,
        };

        public void SetLayerColor(int layer, Color color)
        {
            if (layer < m_LayerColors.Length) {
                m_LayerColors[layer] = color;
            }
        }

        [SerializeField]
        public Image m_PrevBar, m_FadeBar, m_CurrBar;
        public RectTransform m_Thumb;

        [SerializeField]
        private Direction m_Direction = Direction.LeftToRight;
        public Direction direction {
            get { return m_Direction; }
            set {
                if (SetPropertyUtility.SetStruct(ref m_Direction, value)) {
                    UpdateHandle();
                    UpdateVisuals();
                }
            }
        }

        [SerializeField] private UILabel m_LayerLabel;

        [SerializeField] private string m_LayerFmt = "{0}";

        [SerializeField]
        private SliderEvent m_OnValueChanged = new SliderEvent();
        public SliderEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

        private int m_Layer;
        private float m_Tween = -1f;

        protected float m_VisualValue;
        protected float m_FadeValue;
        protected float m_CurrValue;
        public virtual float value {
            get { return m_CurrValue; }
            set {
                Set(value, true);
            }
        }

        protected int axis { get { return (m_Direction == Direction.LeftToRight || m_Direction == Direction.RightToLeft) ? 0 : 1; } }
        protected bool reverseValue { get { return m_Direction == Direction.RightToLeft || m_Direction == Direction.TopToBottom; } }

        protected RectTransform m_Rect;
        public RectTransform rectTransform { get { if (!m_Rect) m_Rect = GetComponent<RectTransform>(); return m_Rect; } }

        private Color GetColorForLayer(int layer)
        {
            return m_LayerColors[layer % m_LayerColors.Length];
        }

        private void UpdateHandle()
        {
            if (m_Thumb) {
                switch (m_Direction) {
                    case Direction.BottomToTop:
                        m_Thumb.anchorMin = new Vector2(0.5f, 0f);
                        m_Thumb.anchorMax = new Vector2(0.5f, 0f);
                        break;
                    case Direction.LeftToRight:
                        m_Thumb.anchorMin = new Vector2(0f, 0.5f);
                        m_Thumb.anchorMax = new Vector2(0f, 0.5f);
                        break;
                    case Direction.RightToLeft:
                        m_Thumb.anchorMin = new Vector2(1f, 0.5f);
                        m_Thumb.anchorMax = new Vector2(1f, 0.5f);
                        break;
                    case Direction.TopToBottom:
                        m_Thumb.anchorMin = new Vector2(0.5f, 1f);
                        m_Thumb.anchorMax = new Vector2(0.5f, 1f);
                        break;
                    default:
                        break;
                }
            }
        }

        protected float CalcVisualValue(float realValue)
        {
            var visualValue = 1f;
            if (realValue < maxValue) {
                var val = realValue - minValue;
                var rngValue = maxValue - minValue;
                float layerVal = rngValue / maxLayer;
                m_Layer = Mathf.FloorToInt(val / layerVal);

                visualValue = minValue + (val - m_Layer * layerVal) / layerVal * rngValue;
            } else {
                m_Layer = maxLayer;
            }
            return visualValue;
        }

        protected void SetVisualImage(Image image, float visualValue)
        {
            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.one;

            if (image != null && image.type == Image.Type.Filled) {
                image.fillAmount = visualValue;
            } else {
                if (reverseValue)
                    anchorMin[axis] = 1 - visualValue;
                else
                    anchorMax[axis] = visualValue;
            }

            if (image) {
                var fillRect = image.rectTransform;
                fillRect.anchorMin = anchorMin;
                fillRect.anchorMax = anchorMax;
            }
        }

        protected virtual void UpdateVisuals()
        {
            var visualValue = CalcVisualValue(m_CurrValue);

            SetVisualImage(m_CurrBar, visualValue);

            if (m_Thumb) {
                var anchoredPos = Vector2.zero;
                var sizeDelta = rectTransform.sizeDelta;
                if (axis == 0) {
                    if (reverseValue) {
                        anchoredPos = new Vector2(-visualValue * sizeDelta.x, 0f);
                    } else {
                        anchoredPos = new Vector2(visualValue * sizeDelta.x, 0f);
                    }
                } else {
                    if (reverseValue) {
                        anchoredPos = new Vector2(0f, -visualValue * sizeDelta.y);
                    } else {
                        anchoredPos = new Vector2(0f, visualValue * sizeDelta.y);
                    }
                }
                m_Thumb.anchoredPosition = anchoredPos;
            }

            if (m_PrevBar) {
                m_PrevBar.color = m_Layer > 0 ? GetColorForLayer(m_Layer - 1) : Color.clear;
            }
            if (m_CurrBar) {
                m_CurrBar.color = m_Layer < maxLayer ? GetColorForLayer(m_Layer) : GetColorForLayer(maxLayer - 1);
            }

            if (m_LayerLabel) {
                if (m_CurrValue > minValue) {
                    m_LayerLabel.text = string.Format(m_LayerFmt, Mathf.Min(m_Layer + 1, maxLayer));
                } else {
                    m_LayerLabel.text = string.Format(m_LayerFmt, 0);
                }
            }
        }

        protected virtual void Set(float input, bool sendCallback)
        {
            input = Mathf.Clamp(input, minValue, maxValue);
            if (input == m_CurrValue) return;

            var cachedValue = m_CurrValue;
            m_CurrValue = input;
            UpdateVisuals();

            if (sendCallback) {
                m_OnValueChanged.Invoke(m_CurrValue);
                m_ValueChanged.Send(this, Wnd, NoBoxingValue<float>.Apply(value));
            }

            if (m_FadeBar) {
                if (m_Tween > 0f) {
                    m_FadeValue = Mathf.Lerp(m_FadeValue, cachedValue, m_Tween);
                }
                if (input > m_FadeValue) {
                    m_FadeValue = input;
                } else {
                    var val = m_FadeValue - minValue;
                    var rngValue = maxValue - minValue;
                    float layerVal = rngValue / maxLayer;
                    var fadeLayer = Mathf.FloorToInt(val / layerVal);
                    if (fadeLayer > m_Layer) m_FadeValue = (m_Layer + 1) * layerVal;
                }
                m_Tween = 0f;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            UpdateHandle();
            UpdateVisuals();
        }
#endif

        protected override void OnEnable()
        {
            UpdateHandle();
        }

        public void InitValue(float input)
        {
            m_CurrValue = input;
            m_FadeValue = input;
            SetVisualImage(m_FadeBar, input);
            UpdateVisuals();
            m_Tween = -1;
        }


        private void Update()
        {
            if (m_Tween < 0f) return;

            m_Tween += Time.deltaTime * 2f;
            if (m_Tween > 1f) m_Tween = 1f;
            var visual = CalcVisualValue(Mathf.Lerp(m_FadeValue, m_CurrValue, m_Tween));
            SetVisualImage(m_FadeBar, visual);
            if (m_Tween == 1f) {
                m_FadeValue = m_CurrValue;
                m_Tween = -1f;
            }

        }

        #region 事件通知
        [SerializeField, HideInInspector]
        private EventData m_ValueChanged = new EventData(TriggerType.None, UIEvent.Send);

        IEnumerator<EventData> IEnumerable<EventData>.GetEnumerator()
        {
            yield return m_ValueChanged;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return m_ValueChanged;
        }

        private IEventTransfer __Wnd;
        protected IEventTransfer Wnd {
            get {
                if (__Wnd == null) {
                    __Wnd = GetComponentInParent(typeof(IEventTransfer)) as IEventTransfer;
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

        private void Setter(float value)
        {
            Set(value, true);
        }

        private float Getter()
        {
            return value;
        }

        public object Tween(object from, object to, float duration)
        {
            object tw = null;

            if (to is float) {
                tw = this.Tween(Getter, Setter, (float)to, duration);
                if (from != null) {
                    value = (float)from;
                    tw.StartFrom(value);
                }
            }

            if (tw != null) tw.SetTag(this);
            return tw;
        }

        public object Tween(float to, float duration)
        {
            return this.Tween(Getter, Setter, to, duration).SetTag(this);
        }

        public object Tween(float from, float to, float duration)
        {
            object tw = this.Tween(Getter, Setter, to, duration);
            value = (float)from;
            return tw.StartFrom(value).SetTag(this);
        }
    }
}