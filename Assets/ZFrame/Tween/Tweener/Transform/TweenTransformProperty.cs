using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZFrame.Tween
{
    public abstract class TweenTransformProperty : TweenVector3<Transform>
    {
        [SerializeField] protected Space m_Space = Space.Self;

#if UNITY_EDITOR        
        internal abstract class TweenTransformEditor : TweenValueEditor
        {
            protected override void OnPropertiesGUI()
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Space"));
            }
        }
#endif
    }
}
