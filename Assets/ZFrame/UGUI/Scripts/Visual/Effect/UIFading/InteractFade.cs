using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ZFrame.UGUI
{
    using Tween;

    public enum FadeGroup { In, Out, Custom, }

    public enum AutoFade
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

    public abstract class InteractFade : UISelectable,
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
        private int m_Fadein = 0;
        [SerializeField, HideInInspector]
        private int m_Fadeout = 0;
        public int fadein { get { return m_Fadein; } }
        public int fadeout { get { return m_Fadeout; } }

        public virtual GameObject target { get { return gameObject; } }
        public virtual Ease easeType { get { return Ease.Linear; } }
        public virtual float delay { get { return 0; } }
        public virtual int loops { get { return 0; } }
        public virtual bool ignoreTimescale { get { return true; } }
        public virtual LoopType loopType { get { return LoopType.Restart; }  }
        public abstract float duration { get; }

        protected ZTweener m_Tweener;
        public ZTweener tweener { get { return m_Tweener; } }

        public float lifetime {
            get {
                if (loops < 0) return -1f;
                return duration * (loops == 0 ? 1 : loops) + delay;
            }
        }

        protected abstract void Restart();
        protected abstract void SetRestart(bool forward);
        protected abstract ZTweener AnimateFade(bool forward);

        public bool DOFade(bool reset, bool forward)
        {
            OnDisable();
            m_Tweener = AnimateFade(forward);
            m_Tweener.EaseBy(easeType).DelayFor(delay);

            if (m_Tweener != null) {
                m_Tweener.SetTag(gameObject).SetUpdate(UpdateType.Normal, ignoreTimescale);
                if (loops != 0) {
                    m_Tweener.LoopFor(loops, loopType);
                }
                if (reset) {
                    if (delay == 0) {
                        this.Restart();
                        this.SetRestart(forward);
                    }
                }
            }
            return m_Tweener != null;
        }

        public void Play(bool forward)
        {
            DOFade(true, forward);
        }

        protected override void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (!DOAutoFade(AutoFade.OnEnable)) {
                if (delay == 0) {
                    Restart();
                }
            }
        }

        protected override void OnDisable()
        {
            if (m_Tweener != null && m_Tweener.IsTweening()) {
                m_Tweener.CompleteWith(null);
                m_Tweener.Stop(true);
            }
        }

        private bool DOAutoFade(AutoFade fade)
        {
            var iFade = (int)fade;
            if ((fadein & iFade) != 0) {
                DOFade(true, true);
            } else if ((fadeout & iFade) != 0) {
                DOFade(false, false);
            } else return false;

            return true;
        }

        #region Event Handler
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (IsInteractable()) DOAutoFade(AutoFade.PointerEnter);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.pointerEnterHandler);
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (IsInteractable()) DOAutoFade(AutoFade.PointerExit);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.pointerExitHandler);
            }
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (IsInteractable()) DOAutoFade(AutoFade.PointerDown);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.pointerDownHandler);
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (IsInteractable()) DOAutoFade(AutoFade.PointerUp);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.pointerUpHandler);
            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (IsInteractable()) DOAutoFade(AutoFade.PointerClick);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.pointerClickHandler);
            }
        }

        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            if (IsInteractable()) DOAutoFade(AutoFade.Drop);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.dragHandler);
            }
        }

        void IUpdateSelectedHandler.OnUpdateSelected(BaseEventData eventData)
        {
            if (IsInteractable()) DOAutoFade(AutoFade.UpdateSelected);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.updateSelectedHandler);
            }
        }

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            if (IsInteractable()) DOAutoFade(AutoFade.Select);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.selectHandler);
            }
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            if (IsInteractable()) DOAutoFade(AutoFade.Deselect);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.deselectHandler);
            }
        }

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (IsInteractable()) DOAutoFade(AutoFade.InitializePotentialDrag);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.initializePotentialDrag);
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (IsInteractable()) DOAutoFade(AutoFade.BeginDrag);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.beginDragHandler);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (IsInteractable()) DOAutoFade(AutoFade.EndDrag);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.endDragHandler);
            }
        }

        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {
            if (IsInteractable()) DOAutoFade(AutoFade.Submit);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.submitHandler);
            }
        }

        void ICancelHandler.OnCancel(BaseEventData eventData)
        {
            if (IsInteractable()) DOAutoFade(AutoFade.Cancle);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.cancelHandler);
            }
        }
        #endregion
    }
}
