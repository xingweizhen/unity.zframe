using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ZFrame.Editors
{
    using Settings;
    [CustomEditor(typeof(AssetProcessSettings), true)]
    public class AssetProcessSettingsEditor : ZFrameSettings4FolderEditor
    {
        protected override void DrawSettingsHeader()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_IgnorePattern"));
            EditorGUILayout.Separator();
            base.DrawSettingsHeader();
        }

        protected override void DrawSettings()
        {
            var self = (AssetProcessSettings)target;
            var settings = m_SelSettings;
            var enumType = self.props.GetType();
            var flags = settings.FindPropertyRelative("flags");
            self.props = (System.Enum)System.Enum.ToObject(enumType, flags.intValue);

#if UNITY_2017_3_OR_NEWER
            flags.intValue = System.Convert.ToInt32(EditorGUILayout.EnumFlagsField("属性管理", self.props));
#else
            flags.intValue = System.Convert.ToInt32(EditorGUILayout.EnumMaskPopup("属性管理", self.props));
#endif
            EditorGUI.indentLevel++;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(flags);
            EditorGUI.EndDisabledGroup();
            foreach (var enVal in System.Enum.GetValues(enumType)) {
                var value = System.Convert.ToInt32(enVal);
                if ((flags.intValue & value) != 0) {
                    var enumName = System.Enum.GetName(enumType, enVal);
                    EditorGUILayout.PropertyField(settings.FindPropertyRelative(enumName));
                }
            }
            EditorGUI.indentLevel--;
        }
    }
}

