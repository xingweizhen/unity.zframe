using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ZFrame.UGUI
{
    public enum UIEvent
    {
        /// <summary>
        /// 自动确定事件参数
        /// </summary>
        Auto,
        /// <summary>
        /// 显示一个新窗口
        /// </summary>
        Show,
        /// <summary>
        /// 打开一个新窗口（栈管理）
        /// </summary>
        Open,
        /// <summary>
        /// 打开一个新的弹窗（栈管理）
        /// </summary>
        PopWindow,
        /// <summary>
        /// 关闭本窗口
        /// </summary>
        Close,
        /// <summary>
        /// 自定义事件参数
        /// </summary>
        Send,
    }

    /// <summary>
    /// 扩展 UnityEngine.EventSystems.EventTriggerType
    /// </summary>
    public enum TriggerType
    {
        None = -1,

        PointerEnter = EventTriggerType.PointerEnter,
        PointerExit = EventTriggerType.PointerExit,
        PointerDown = EventTriggerType.PointerDown,
        PointerUp = EventTriggerType.PointerUp,
        PointerClick = EventTriggerType.PointerClick,
        Drag = EventTriggerType.Drag,
        Drop = EventTriggerType.Drop,
        Scroll = EventTriggerType.Scroll,
        UpdateSelected = EventTriggerType.UpdateSelected,
        Select = EventTriggerType.Select,
        Deselect = EventTriggerType.Deselect,
        Move = EventTriggerType.Move,
        InitializePotentialDrag = EventTriggerType.InitializePotentialDrag,
        BeginDrag = EventTriggerType.BeginDrag,
        EndDrag = EventTriggerType.EndDrag,
        Submit = EventTriggerType.Submit,
        Cancel = EventTriggerType.Cancel,
        Longpress,
        DoubleClick,

        /// <summary>
        /// 同Deselect，不过会延迟到本帧结束才触发
        /// </summary>
        Unselect,
    }

    [System.Serializable]
    public class EventData
    {
        public TriggerType type;
        public UIEvent name;
        public string param;

        public EventData(TriggerType type, UIEvent name = UIEvent.Auto)
        {
            this.type = type;
            this.name = name;
        }

        public bool IsActive()
        {
            return name == UIEvent.Close || !string.IsNullOrEmpty(param);
        }

        public void Send(Component sender, IEventTransfer transfer, object data = null)
        {
            if (IsActive()) transfer.SendEvent(sender, name, param, data);
        }
    }

    public interface IEventSender : IEnumerable<EventData>
    {
        void SetEvent(TriggerType id, UIEvent eventName, string param);
    }

    public interface IEventTransfer
    {
        void SendEvent(Component sender, UIEvent eventName, string eventParam, object data);
    }
}
