using UnityEngine;
using System.Collections;

namespace ZFrame.UGUI
{
    public enum SelectingState
    {
        Normal = 0,
        Highlighted = 1,
        Pressed = 2,
        Disabled = 3
    }
    
    public interface IStateTransTarget
    {
        UnityEngine.UI.Graphic targetGraphic { get; }
    }

    public interface IStateTransition
    {
        void OnStateTransition(SelectingState state, bool instant);
    }

    public static class StateTransitionextends
    {
        public static void TryStateTransition(this Component self, SelectingState state, bool instant)
        {
            var list = ListPool<Component>.Get();
            self.GetComponents(typeof(IStateTransition), list);
            foreach (IStateTransition st in list) {
                st.OnStateTransition(state, instant);
            }
            ListPool<Component>.Release(list);
        }
    }
}
