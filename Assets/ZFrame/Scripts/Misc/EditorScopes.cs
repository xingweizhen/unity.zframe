
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ZFrame.Editors
{
    public struct LabelWidthScope : System.IDisposable
    {
        float _orginal;

        public LabelWidthScope(float value)
        {
            _orginal = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = value;
        }

        void System.IDisposable.Dispose()
        {
            EditorGUIUtility.labelWidth = _orginal;
        }
    }
}

#endif
