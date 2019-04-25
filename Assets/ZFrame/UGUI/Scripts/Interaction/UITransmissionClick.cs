using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
//using NoToLua = XLua.BlackListAttribute;

namespace ZFrame.UGUI
{
    public class UITransmissionClick : UIButton
    {
        private static List<RaycastResult> _RaycastList = new List<RaycastResult>();
        
        //[NoToLua]
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (IsActive() && IsInteractable()) {
                base.OnPointerClick(eventData);
                this.OnEventTrigger(eventData);
                if (onButtonClick != null)
                    onButtonClick.Invoke(gameObject);

                EventSystem.current.RaycastAll(eventData, _RaycastList);

                foreach (var rst in _RaycastList) {
                    if (rst.gameObject.Equals(gameObject))
                        continue;

                    ExecuteEvents.ExecuteHierarchy(rst.gameObject, 
                        new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
                    break;
                }
                _RaycastList.Clear();

                // 点击音效
                UGUITools.PlayUISfx(clickSfx, defaultSfx);
            }
        }
    }
}
