using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffScreenAutoHide : MonoBehaviour {

    [SerializeField] private RectTransform m_ScreenBorder;
    [SerializeField] private Vector2 m_vHideOffset;
    [SerializeField] private RectTransform m_Ctrl;
    //[SerializeField] private Canvas m_Renderer;

    Vector2 m_vMaxBorderSize = Vector2.zero;
    Vector2 m_vMinBorderSize = Vector2.zero;

    private CanvasGroup m_CvGroup;

    private void Start()
    {
        Vector2 rect = m_ScreenBorder.rect.size;
        m_vMaxBorderSize = (rect / 2f) + m_vHideOffset;
        m_vMinBorderSize = -m_vMaxBorderSize;

        m_CvGroup = gameObject.NeedComponent(typeof(CanvasGroup)) as CanvasGroup;
    }

    void SetVisible(bool isVisible)
    {
        m_CvGroup.alpha = isVisible ? 1 : 0;
    }

    void Update()
    {
        Vector3 pos = m_Ctrl.localPosition;
        if (pos.x > m_vMaxBorderSize.x || pos.y > m_vMaxBorderSize.y ||
            pos.x < m_vMinBorderSize.x || pos.y < m_vMinBorderSize.y)
        {
            SetVisible(false);
        }
        else
        {
            SetVisible(true);
        }
    }
}
