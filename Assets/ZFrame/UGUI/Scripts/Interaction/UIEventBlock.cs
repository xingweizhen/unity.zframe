using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ZFrame.UGUI
{
    public class UIEventBlock : UIBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler,
        IInitializePotentialDragHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler,
        IScrollHandler,
        IUpdateSelectedHandler,
        ISelectHandler,
        IDeselectHandler,
        IMoveHandler,
        ISubmitHandler,
        ICancelHandler
    {
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) { }

        void ICancelHandler.OnCancel(BaseEventData eventData) { }

        void IDeselectHandler.OnDeselect(BaseEventData eventData) { }

        void IDragHandler.OnDrag(PointerEventData eventData) { }

        void IDropHandler.OnDrop(PointerEventData eventData) { }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData) { }

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData) { }

        void IMoveHandler.OnMove(AxisEventData eventData) { }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData) { }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) { }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) { }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData) { }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) { }

        void IScrollHandler.OnScroll(PointerEventData eventData) { }

        void ISelectHandler.OnSelect(BaseEventData eventData) { }

        void ISubmitHandler.OnSubmit(BaseEventData eventData) { }

        void IUpdateSelectedHandler.OnUpdateSelected(BaseEventData eventData) { }
    }
}
