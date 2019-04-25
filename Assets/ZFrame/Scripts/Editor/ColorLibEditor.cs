using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.IO;

[CustomEditor(typeof(ColorLib))]
public class ColorLibEditor : Editor
{
//#if !UNITY_5
//    [MenuItem("Assets/Create/资源库/颜色")]
//    private static void CreateAsset()
//    {
//        var asset = CreateInstance(typeof(ColorLib));
//        var o = Selection.activeObject;
//        var path = "Assets";
//        if (o != null) {
//            path = AssetDatabase.GetAssetPath(o);
//            if (!Directory.Exists(path)) {
//                path = Path.GetDirectoryName(path);
//            }
//        }
//        AssetDatabase.CreateAsset(asset, Path.Combine(path, "New ColorLib.asset"));
//    }
//#endif

    private SerializedProperty m_NamedColors;
    private SerializedProperty m_TextColors;

    private ReorderableList m_NamedColorList;
    private ReorderableList m_TextColorList;

    private void OnEnable()
    {
        m_NamedColors = serializedObject.FindProperty("m_NamedColors");
        m_TextColors = serializedObject.FindProperty("m_TextColors");

        m_NamedColorList = new ReorderableList(serializedObject, m_NamedColors, true, true, true, true);
        m_NamedColorList.drawHeaderCallback = DrawNamedColorHeader;
        m_NamedColorList.drawElementCallback = DrawNamedColorElement;

        m_TextColorList = new ReorderableList(serializedObject, m_TextColors, true, true, true, true);
        m_TextColorList.elementHeight = (EditorGUIUtility.singleLineHeight + 2) * 3 + 10;
        m_TextColorList.drawHeaderCallback = DrawTextColorHeader;
        m_TextColorList.drawElementCallback = DrawTextColorElement;
    }

    private void DrawNamedColorHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "命名颜色库");
    }

    private void DrawNamedColorElement(Rect rect, int index, bool selected, bool focused)
    {
        SerializedProperty element = m_NamedColorList.serializedProperty.GetArrayElementAtIndex(index);

        rect.y += 2;
        rect.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(rect, element);
    }

    private void DrawTextColorHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "文本颜色库");
    }

    private void DrawTextColorElement(Rect rect, int index, bool selected, bool focused)
    {
        SerializedProperty element = m_TextColorList.serializedProperty.GetArrayElementAtIndex(index);

        rect.y += 2;
        rect.height = m_TextColorList.elementHeight;
        using (new EditorGUI.PropertyScope(rect, null, element)) {
            var lbW = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 50;

            var name = element.FindPropertyRelative("name");
            var outline = element.FindPropertyRelative("outline");
            var gradient1 = element.FindPropertyRelative("gradient1");
            var gradient2 = element.FindPropertyRelative("gradient2");
            
            var pos = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            name.stringValue = EditorGUI.TextField(pos, "样式名称", name.stringValue);

            pos.x += 10;
            pos.width -= 10;
            pos.y += pos.height + 2;
            outline.colorValue = EditorGUI.ColorField(pos, outline.colorValue);

            var width = pos.width / 2;
            pos.y += pos.height + 2;
            pos.width = width - 4;
            gradient1.colorValue = EditorGUI.ColorField(pos, gradient1.colorValue);

            pos.x += width + 4;
            gradient2.colorValue = EditorGUI.ColorField(pos, gradient2.colorValue);

            EditorGUIUtility.labelWidth = lbW;
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        serializedObject.Update();
        m_NamedColorList.DoLayoutList();
        m_TextColorList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }
}
