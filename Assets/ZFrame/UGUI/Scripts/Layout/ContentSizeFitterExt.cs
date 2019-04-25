using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using FitMode = UnityEngine.UI.ContentSizeFitter.FitMode;
using Axis = UnityEngine.RectTransform.Axis;
using System;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class ContentSizeFitterExt : UIBehaviour, ILayoutSelfController
{
    [SerializeField] protected FitMode m_HorizontalFit = FitMode.Unconstrained;
    [SerializeField] protected FitMode m_VerticalFit = FitMode.Unconstrained;
    public RectTransform TargetRTran;
    public LayoutElement TargetLayoutElement = null;//如果TargetRTran挂有LayoutElement组件，则仅控制LayoutElement的prefer宽高
    public Vector2 targetSizePlus = Vector2.zero;
    private RectTransform m_Rect;
    public bool MinLimit = false;
    public Vector2 MinSize = Vector2.zero;
    private RectTransform rectTransform
    {
        get
        {
            if (m_Rect == null)
                m_Rect = GetComponent<RectTransform>();
            return m_Rect;
        }
    }
    public FitMode horizontalFit
    {
        get { return m_HorizontalFit; }
        set
        {
            if(m_HorizontalFit != value)
            {
                SetDirty();
            }
            m_HorizontalFit = value;
        }
    }
    public FitMode verticalFit
    {
        get { return m_VerticalFit; }
        set
        {
            if (m_VerticalFit != value)
            {
                SetDirty();
            }
            m_VerticalFit = value;
        }
    }
    private DrivenRectTransformTracker m_Tracker;

    protected override void OnEnable()
    {
        base.OnEnable();
        SetDirty();
    }
    protected override void OnDisable()
    {
        m_Tracker.Clear();
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        base.OnDisable();
    }

    protected override void OnRectTransformDimensionsChange()//extend:UIBaviour
    {
        SetDirty();
    }

    public void SetLayoutHorizontal()//extend:ILayoutSelfController
    {
        m_Tracker.Clear();
        HandleSelfFittingAlongAxis(0);
    }

    public void SetLayoutVertical()
    {
        HandleSelfFittingAlongAxis(1);
    }

    private void HandleSelfFittingAlongAxis(int axis)
    {
        FitMode fitting = (axis == 0 ? horizontalFit : verticalFit);
        if (fitting == FitMode.Unconstrained)
            return;
        m_Tracker.Add(this, rectTransform, (axis == 0 ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY));

        //Set size to min or preferred size
        float size = 100f;
        if (fitting == FitMode.MinSize)
        {
            size = Mathf.CeilToInt(LayoutUtility.GetMinSize(m_Rect, axis));
            rectTransform.SetSizeWithCurrentAnchors((Axis)axis, size);
            
        }
        else
        {
            size = Mathf.CeilToInt(LayoutUtility.GetPreferredSize(m_Rect, axis));
            rectTransform.SetSizeWithCurrentAnchors((Axis)axis, size);
        }

        if (TargetRTran != null)
        {
            if (MinLimit)
            {
                size = Mathf.Max(size, axis == 0 ? MinSize.x : MinSize.y);
                if (TargetLayoutElement == null)
                {
                    TargetRTran.SetSizeWithCurrentAnchors((Axis)axis, size);
                }
                else
                {
                    if (axis == 0)
                    {
                        TargetLayoutElement.preferredWidth = size;
                    }
                    else
                    {
                        TargetLayoutElement.preferredHeight = size;
                    }
                }
            }
            else
            {
                if (TargetLayoutElement == null)
                {
                    TargetRTran.SetSizeWithCurrentAnchors((Axis)axis, size + (axis == 0 ? targetSizePlus.x : targetSizePlus.y));
                }
                else
                {
                    if (axis == 0)
                    {
                        TargetLayoutElement.preferredWidth = size + targetSizePlus.x;
                    }
                    else
                    {
                        TargetLayoutElement.preferredHeight = size + targetSizePlus.y;
                    }
                }
            }
        }
    }
    
    protected void SetDirty()
    {
        if (!IsActive())
            return;
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        SetDirty();
    }
#endif
}
