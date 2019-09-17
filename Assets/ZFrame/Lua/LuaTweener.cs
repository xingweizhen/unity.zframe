using UnityEngine;
using System.Collections;
using ZFrame;
using ZFrame.UGUI;
using ZFrame.Tween;

namespace ZFrame.Lua
{
    public class LuaTweener : MonoBehaviour, IFading
    {
        [SerializeField] private string m_TweenFunc;
        [SerializeField] private FadeGroup m_Group;
        [SerializeField] private GameObject m_Root;

        private float m_Lifetime;

        public float lifetime {
            get { return m_Lifetime; }
        }

        public FadeGroup group {
            get { return m_Group; }
        }

        public object tweener { get; private set; }

        public bool DOFade(bool reset, bool forward)
        {
            tweener = null;
            m_Lifetime = 0;
            var lua = LuaScriptMgr.Instance.L;
            lua.GetGlobal("TWEEN", m_TweenFunc);
            if (lua.IsFunction(-1)) {
                var b = lua.BeginPCall();
                lua.PushLightUserData(m_Root ? m_Root : gameObject);
                lua.PushBoolean(reset);
                lua.PushBoolean(forward);
                lua.ExecPCall(3, 2, b);
                tweener = lua.ToAnyObject(-2) as ZTweener;
                m_Lifetime = lua.OptSingle(-1, 0f);
                lua.Pop(2);
            } else {
                lua.Pop(1);
                LogMgr.W("Call TWEEN:{0}() failure!", m_TweenFunc);
            }

            return tweener != null;
        }
    }
}
