using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ZFrame.Lua
{
    [CustomEditor(typeof(LuaScriptMgr))]
    public class LuaScriptMgrEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var self = target as LuaScriptMgr;
            if (self.IsLua) {
                EditorGUILayout.LabelField(string.Format("栈：{0}", self.L.GetTop()));
                //EditorGUILayout.LabelField(string.Format("udata：{0}", self.Env.GetTranslator().objects.Count));
            }
        }
    }
}
