using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZFrame.Tween
{
    public enum TweenLaunchEvent
    {
        OnEnable = 1 << 0,
        PointerEnter = 1 << 1,
        PointerExit = 1 << 2,
        PointerDown = 1 << 3,
        PointerUp = 1 << 4,
        PointerClick = 1 << 5,
        Drop = 1 << 6,
        UpdateSelected = 1 << 7,
        Select = 1 << 8,
        Deselect = 1 << 9,
        InitializePotentialDrag = 1 << 10,
        BeginDrag = 1 << 11,
        EndDrag = 1 << 12,
        Submit = 1 << 13,
        Cancle = 1 << 14,
    }

    public class TweenLauncher : TweenGroup,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler,
        IDropHandler,
        IUpdateSelectedHandler,
        ISelectHandler,
        IDeselectHandler,
        IInitializePotentialDragHandler,
        IBeginDragHandler,
        IEndDragHandler,
        ISubmitHandler,
        ICancelHandler
    {
        [SerializeField, HideInInspector]
        private int m_EventMaskIn = 0;
        [SerializeField, HideInInspector]
        private int m_EventMaskOut = 0;
        public int eventIn { get { return m_EventMaskIn; } }
        public int eventOut { get { return m_EventMaskOut; } }

        private bool TryTweenByEvent(TweenLaunchEvent evt)
        {
            if (!tweenAutomatically) return false;

            var iEvent = (int)evt;
            if ((m_EventMaskIn & iEvent) != 0) {
                DoTween(true);
            } else if ((m_EventMaskOut & iEvent) != 0) {
                DoTween(false);
            } else return false;

            return true;
        }

        protected override void OnEnable()
        {
            TryTweenByEvent(TweenLaunchEvent.OnEnable);
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            TryTweenByEvent(TweenLaunchEvent.BeginDrag);
        }

        void ICancelHandler.OnCancel(BaseEventData eventData)
        {
            TryTweenByEvent(TweenLaunchEvent.Cancle);
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            TryTweenByEvent(TweenLaunchEvent.Deselect);
        }

        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            TryTweenByEvent(TweenLaunchEvent.Drop);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            TryTweenByEvent(TweenLaunchEvent.EndDrag);
        }

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            TryTweenByEvent(TweenLaunchEvent.InitializePotentialDrag);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            TryTweenByEvent(TweenLaunchEvent.PointerClick);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            TryTweenByEvent(TweenLaunchEvent.PointerDown);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            TryTweenByEvent(TweenLaunchEvent.PointerEnter);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            TryTweenByEvent(TweenLaunchEvent.PointerExit);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            TryTweenByEvent(TweenLaunchEvent.PointerUp);
        }

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            TryTweenByEvent(TweenLaunchEvent.Select);
        }

        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {
            TryTweenByEvent(TweenLaunchEvent.Submit);
        }

        void IUpdateSelectedHandler.OnUpdateSelected(BaseEventData eventData)
        {
            TryTweenByEvent(TweenLaunchEvent.UpdateSelected);
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(TweenLauncher), true)]
        new internal class SelfEditor : TweenGroup.SelfEditor
        {
            protected override void PropertiesGUI()
            {
                base.PropertiesGUI();

                var self = (TweenLauncher)target;
                if (self.tweenAutomatically) {
                    self.m_EventMaskIn = (int)(TweenLaunchEvent)EditorGUILayout.EnumFlagsField("Event Mask In", (TweenLaunchEvent)self.m_EventMaskIn);
                    self.m_EventMaskOut = (int)(TweenLaunchEvent)EditorGUILayout.EnumFlagsField("Event Mask Out", (TweenLaunchEvent)self.m_EventMaskOut);
                }
            }
        }
#endif
    }
}
