using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZFrame;
using ZFrame.Asset;

public class UIFitSafeScreen : MonoBehaviour
{
    private static Rect safeArea;

    [Tooltip("自动配适屏幕安全区")]
    public bool m_bAutofit = true;
    [Tooltip("全屏展示的背景图(不受安全区影响)")]
    public RectTransform m_rtBackGround;

    private RectTransform rectTrans = null;
    private System.Action<DeviceOrientation> onHandleScreenOrientationChanged;

    private void ApplySafeArea()
    {
        //if (rectTrans)
        //{
        //    Vector2 anchorMin = safeArea.position;
        //    Vector2 anchorMax = safeArea.position + safeArea.size;
        //    anchorMin.x /= Screen.width;
        //    anchorMin.y = 0;
        //    anchorMax.x /= Screen.width;
        //    anchorMax.y = 1;

        //    rectTrans.anchorMin = anchorMin;
        //    rectTrans.anchorMax = anchorMax;

        //    if (m_rtBackGround)
        //    {
        //        float orgX = 1 / (anchorMax.x - anchorMin.x);
        //        float deltaLeft = -orgX * anchorMin.x;
        //        float deltaRight = -orgX * (1 - anchorMax.x);

        //        m_rtBackGround.anchorMin = new Vector2(deltaLeft, 0);
        //        m_rtBackGround.anchorMax = new Vector2(1 - deltaRight, 1);
        //    }
        //}
    }

    private void Awake()
    {
        if (m_bAutofit)
        {
            rectTrans = transform as RectTransform;
            onHandleScreenOrientationChanged = delegate { ApplySafeArea(); };
            ApplySafeArea();
        }
    }

    public void Start()
    {
        if (m_bAutofit)
            UIManager.Instance.onScreenOrientationChanged += onHandleScreenOrientationChanged;
    }

    public void OnRecycle()
    {
        if (m_bAutofit)
            UIManager.Instance.onScreenOrientationChanged -= onHandleScreenOrientationChanged;

    }

    public static void SetSafeArea(Rect _safeArea)
    {
        safeArea = _safeArea;
    }
}
