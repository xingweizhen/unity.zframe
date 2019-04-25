using System.Collections;
using System.Collections.Generic;

namespace ZFrame.HFSM
{
    public enum TransType {
        /// <summary>
        /// 状态转换时，新状态替代老状态
        /// </summary>
        SET,

        /// <summary>
        /// 状态转换时，老状态入栈
        /// </summary>
        PUSH,

        /// <summary>
        /// 状态转换时，老状态出栈，再已新状态代替栈顶状态
        /// </summary>
        POP,
    }
    
    public delegate void StateTransfer(IFSMContext context, BaseState src, BaseState dst);

    public interface IFSMContext
    {
        HFSM fsm { get; }
        bool OnEvent(int eventId);    
    }

    public abstract class BaseState
    {
        public abstract int id { get; }

        public virtual void Enter(IFSMContext context) { }
        public virtual void Exit(IFSMContext context) { }
        public virtual bool Update(IFSMContext context) { return true; }
    }
}
