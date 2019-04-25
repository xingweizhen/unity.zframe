using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

namespace ZFrame.UGUI
{
    public class UISliderExt : Slider
    {
        [SerializeField]
        private Graphic m_PrevLayer;

        [SerializeField]
        private RectTransform m_FollowRect;
        private Image m_FollowImage;

        public int totalLayer = 1;

        [SerializeField]
        private Color[] m_LayerColors = new Color[] {
            Color.white,
        };

        [SerializeField]
        private UILabel lbLayer;
        
        private float m_OriginValue
        {
            get { return ((Slider)this).value; }
            set { ((Slider)this).value = value; }
        }

        private int m_Layer;

        private Graphic m_CurrLayer;
        private float m_PrevValue, m_CurrValue;
        private float m_Tween = -1f;

        private int axis { get { return (direction == Direction.LeftToRight || direction == Direction.RightToLeft) ? 0 : 1; } }
        bool reverseValue { get { return direction == Direction.RightToLeft || direction == Direction.TopToBottom; } }

        new public float value
        {
            get { return m_CurrValue; }
            set
            {
                if (m_FollowRect) {
                    SetVisualValue(value);
                } else {
                    if (m_Tween > 0f) {
                        m_PrevValue = Mathf.Lerp(m_PrevValue, m_CurrValue, m_Tween);
                    }
                    m_Tween = 0f;
                }
                m_CurrValue = value;
            }
        }

        private Color GetColorForLayer(int layer)
        {
            return m_LayerColors[layer % m_LayerColors.Length];
        }

        private int CalcVisualValue(ref float value)
        {
            var val = value - minValue;
            var rngValue = maxValue - minValue;
            float layerVal = rngValue / totalLayer;
            int layer = Mathf.FloorToInt(val / layerVal);

            value = minValue + (val - layer * layerVal) / layerVal * rngValue;
            //LogMgr.D(string.Format("{0} << n:{1}, v:{2}", value, layer, val));

            return layer;
        }

        private void SetVisualValue(float value)
        {
            var val = value;
            m_Layer = CalcVisualValue(ref val);
            if (m_PrevLayer) {
                m_PrevLayer.color = m_Layer > 0 ? GetColorForLayer(m_Layer - 1) : Color.clear;
            }
            if (m_CurrLayer) {
                m_CurrLayer.color = m_Layer < totalLayer ? GetColorForLayer(m_Layer) : GetColorForLayer(totalLayer - 1);
            }
            m_OriginValue = val;

            if (lbLayer) {
				if (value > minValue) {
					lbLayer.SetFormatArgs(Mathf.Min(m_Layer + 1, totalLayer));
				} else {
                    lbLayer.SetFormatArgs(0);
				}
			}
        }

        private void SetFollowValue(float value)
        {
            if (m_FollowRect) {
                Vector2 anchorMin = Vector2.zero;
                Vector2 anchorMax = Vector2.one;
                
                CalcVisualValue(ref value);
                var t = Mathf.InverseLerp(minValue, maxValue, value);

                if (m_FollowImage && m_FollowImage.type == Image.Type.Filled) {
                    m_FollowImage.fillAmount = t;
                } else {
                    if (reverseValue)
                        anchorMin[axis] = 1 - t;
                    else
                        anchorMax[axis] = t;
                }

                m_FollowRect.anchorMin = anchorMin;
                m_FollowRect.anchorMax = anchorMax;
            } else {
                SetVisualValue(value);
            }
            
        }

        private void OnValueChangedExt(float val)
        {
            if (m_FollowRect) {
                if (m_CurrValue < m_PrevValue) {
                    if (m_Tween >= 0f) {
                        m_PrevValue = Mathf.Lerp(m_PrevValue, m_CurrValue, m_Tween);
                    }
                    m_Tween = 0f;
                } else {
                    m_PrevValue = m_CurrValue;
                    m_Tween = -1f;
                    SetFollowValue(m_CurrValue);
                }
            } else {

            }
        }

        protected override void Awake()
        {
            base.Awake();
            onValueChanged.AddListener(OnValueChangedExt);
            m_PrevValue = m_OriginValue;
            m_CurrValue = m_PrevValue;
        }

        protected override void Start()
        {
            base.Start();
            if (fillRect) {
                m_CurrLayer = fillRect.GetComponent<Graphic>();
            }
            if (m_FollowRect) {
                m_FollowImage = m_FollowRect.GetComponent<Image>();
            }                        
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetVisualValue(m_CurrValue);
        }

        private void Update()
        {
            if (m_Tween < 0f) return;

            m_Tween += Time.deltaTime * 2f;
            if (m_Tween > 1f) m_Tween = 1f;
            SetFollowValue(Mathf.Lerp(m_PrevValue, m_CurrValue, m_Tween));
            if (m_Tween == 1f) {
                m_PrevValue = m_CurrValue;
                m_Tween = -1f;
            }            
        }

		public void InitValue(float value)
		{
			SetVisualValue(value);
			m_CurrValue = value;
		}
    }
}
