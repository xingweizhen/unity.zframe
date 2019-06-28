using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ZFrame.Tween
{
    public class AutoTween : UIBehaviour,
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
        public enum Event
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

        [SerializeField] private int m_Forward = 0;
        [SerializeField] private int m_Backward = 0;

        [SerializeField]
        protected Selectable m_Selectable;

        private bool m_GroupsAllowInteraction;

        private bool m_Interactable = true;
        protected bool IsInteractable()
        {
            return m_Selectable ? m_Selectable.IsInteractable() : m_GroupsAllowInteraction && m_Interactable;
        }

        protected bool IsForwardable()
        {
            return m_Selectable && !m_Selectable.gameObject.Equals(gameObject)
                && m_Selectable.IsInteractable();
        }

        public bool interactable {
            get { return IsInteractable(); }
            set {
                if (IsInteractable() == value) return;

                m_Interactable = value;
                if (m_Selectable) {
                    m_Selectable.interactable = value;
                }
            }
        }

        protected override void OnCanvasGroupChanged()
        {
            // Figure out if parent groups allow interaction
            // If no interaction is alowed... then we need
            // to not do that :)
            var groupAllowInteraction = true;
            Transform t = transform;
            var cvGrps = ListPool<Component>.Get();
            while (t != null) {
                t.GetComponents(typeof(CanvasGroup), cvGrps);
                bool shouldBreak = false;
                for (var i = 0; i < cvGrps.Count; i++) {
                    // if the parent group does not allow interaction
                    // we need to break
                    var cvGrp = (CanvasGroup)cvGrps[i];
                    if (!cvGrp.interactable) {
                        groupAllowInteraction = false;
                        shouldBreak = true;
                    }
                    // if this is a 'fresh' group, then break
                    // as we should not consider parents
                    if (cvGrp.ignoreParentGroups)
                        shouldBreak = true;
                }
                if (shouldBreak)
                    break;

                t = t.parent;
            }
            ListPool<Component>.Release(cvGrps);

            if (groupAllowInteraction != m_GroupsAllowInteraction) {
                m_GroupsAllowInteraction = groupAllowInteraction;
            }
        }

        protected override void Start()
        {
            if (!m_Selectable) m_Selectable = GetComponent<Selectable>();
        }

        private void TweenAll(bool reset, bool forward)
        {
            var list = ListPool<Component>.Get();
            GetComponents(typeof(TweenObject), list);
            for (var i = 0; i < list.Count; ++i) {
                ((TweenObject)list[i]).DoTween(reset, forward);
            }
            ListPool<Component>.Release(list);
        }

        private bool DoTween(Event evt)
        {
            var mask = (int)evt;
            if ((m_Forward & mask) != 0) {
                TweenAll(true, true);
            } else if ((m_Backward & mask) != 0) {
                TweenAll(false, false);
            } else return false;

            return true;
        }

        #region Event Handler
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (IsInteractable()) DoTween(Event.PointerEnter);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.pointerEnterHandler);
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (IsInteractable()) DoTween(Event.PointerExit);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.pointerExitHandler);
            }
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (IsInteractable()) DoTween(Event.PointerDown);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.pointerDownHandler);
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (IsInteractable()) DoTween(Event.PointerUp);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.pointerUpHandler);
            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (IsInteractable()) DoTween(Event.PointerClick);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.pointerClickHandler);
            }
        }

        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            if (IsInteractable()) DoTween(Event.Drop);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.dragHandler);
            }
        }

        void IUpdateSelectedHandler.OnUpdateSelected(BaseEventData eventData)
        {
            if (IsInteractable()) DoTween(Event.UpdateSelected);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.updateSelectedHandler);
            }
        }

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            if (IsInteractable()) DoTween(Event.Select);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.selectHandler);
            }
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            if (IsInteractable()) DoTween(Event.Deselect);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.deselectHandler);
            }
        }

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (IsInteractable()) DoTween(Event.InitializePotentialDrag);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.initializePotentialDrag);
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (IsInteractable()) DoTween(Event.BeginDrag);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.beginDragHandler);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (IsInteractable()) DoTween(Event.EndDrag);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.endDragHandler);
            }
        }

        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {
            if (IsInteractable()) DoTween(Event.Submit);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.submitHandler);
            }
        }

        void ICancelHandler.OnCancel(BaseEventData eventData)
        {
            if (IsInteractable()) DoTween(Event.Cancle);
            if (IsForwardable()) {
                ExecuteEvents.Execute(m_Selectable.gameObject, eventData, ExecuteEvents.cancelHandler);
            }
        }
        #endregion
    }
}
