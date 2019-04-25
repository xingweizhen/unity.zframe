using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ZFrame.UGUI
{
    [RequireComponent(typeof(Slider))]
    public class GradientBar : MonoBehaviour 
    {
        [SerializeField]
        private UnityEngine.Gradient m_Gradient;

        private Slider m_Sld;
        private Graphic m_Fill;

        private void Awake()
        {
            m_Sld = GetComponent(typeof(Slider)) as Slider;
            if (m_Sld) {
                m_Sld.onValueChanged.AddListener(OnSliderValueChanged);
                m_Fill = m_Sld.fillRect.GetComponent(typeof(Graphic)) as Graphic;
            }
        }

        private void OnSliderValueChanged(float value)
        {
            if (m_Fill) {
                if (m_Sld.wholeNumbers) {
                    value = (value - m_Sld.minValue) / (m_Sld.maxValue - m_Sld.minValue);
                }
                m_Fill.color = m_Gradient.Evaluate(value);
            }
        }
    }
}
