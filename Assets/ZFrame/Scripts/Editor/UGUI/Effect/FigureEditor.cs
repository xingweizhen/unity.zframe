using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;

namespace ZFrame.UGUI
{
    [CustomEditor(typeof(Figure), true), CanEditMultipleObjects]    
    public class FigureEditor : SelfControllerEditor
    {
        protected virtual void OnEnable()
        {

        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            base.OnInspectorGUI();
        }
    }
}
