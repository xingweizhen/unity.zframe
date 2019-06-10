using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame.UGUI
{
    /// <summary>
    /// 该组件允许把点击事件转发到下一个Handler
    /// </summary>
    public class UIForwardPointer : UIBehaviour, IPointerClickHandler
    {
        private static List<RaycastResult> _RaycastList = new List<RaycastResult>();

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isActiveAndEnabled) return;

            var selectable = GetComponent(typeof(Selectable)) as Selectable;
            if (selectable && !selectable.IsInteractable()) return;

            EventSystem.current.RaycastAll(eventData, _RaycastList);

            foreach (var rst in _RaycastList) {
                if (rst.gameObject == null || rst.gameObject.Equals(gameObject))
                    continue;

                eventData.pointerCurrentRaycast = rst;
                ExecuteEvents.Execute(rst.gameObject, eventData, ExecuteEvents.pointerClickHandler);
                break;
            }
            _RaycastList.Clear();
        }
    }
}
