using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[CanEditMultipleObjects]
[CustomEditor(typeof(MonoBehavior), true)]
public class MonoBehaviorEditor : Editor
{
	private const BindingFlags bflag = 
		BindingFlags.Instance | 
		BindingFlags.Public | 
		BindingFlags.NonPublic |
		BindingFlags.GetField |
		BindingFlags.GetProperty;

	private static bool m_Show;
	private List<MemberInfo> m_Members = new List<MemberInfo>();

	protected virtual void OnEnable()
	{
		m_Members.Clear();
		var type = target.GetType();

		var fields = type.GetFields(bflag);
		foreach (var f in fields) {
			var attr = System.Attribute.GetCustomAttribute(f, typeof(DescriptionAttribute));
			if (attr != null) m_Members.Add(f);
		}

		var properties = type.GetProperties(bflag);
		foreach (var p in properties) {
			var attr = System.Attribute.GetCustomAttribute(p, typeof(DescriptionAttribute));
			if (attr != null) m_Members.Add(p);
		}
	}

    private void inspectObject(string fName, System.Object obj)
    {        
        if (obj is UnityEngine.Object) {
            var unityObj = obj as UnityEngine.Object;
            EditorGUILayout.ObjectField(fName, unityObj, unityObj.GetType(), true);
        } else {
            EditorGUILayout.TextField(fName, obj == null ? "NULL" : obj.ToString());
        }
    }

    protected void ShowClassTip()
    {
        var attr = System.Attribute.GetCustomAttribute(target.GetType(), typeof(DescriptionAttribute)) as DescriptionAttribute;
        if (attr != null) {
            EditorGUILayout.LabelField(string.Format("<color=yellow>{0}</color>", attr.description), CustomEditorStyles.titleStyle);
        }
    }

    protected void ShowList(string desc, IEnumerable enumerable)
    {
	    var count = 0;
	    foreach (var obj in enumerable) count++;
        EditorGUILayout.LabelField(desc, string.Format("数量={0}", count));
		++EditorGUI.indentLevel;
        foreach (var obj in enumerable) {
            inspectObject("", obj);
        }
		--EditorGUI.indentLevel;
	}

    protected void ShowDictionary(string desc, IDictionary dict)
    {
        EditorGUILayout.LabelField(desc, string.Format("数量={0}", dict.Count));
        ++EditorGUI.indentLevel;
        foreach (DictionaryEntry kv in dict) {
            inspectObject(kv.Key.ToString(), kv.Value);
        }
        --EditorGUI.indentLevel;
    }

    protected void ShowObject(string desc, object obj)
    {
        if (obj is IDictionary) {
            ShowDictionary(desc, (IDictionary)obj);
        } else if (obj is ICollection || typeof(ICollection<>).IsInstanceOfType(obj)) {
            ShowList(desc, obj as IEnumerable);
        } else {
            inspectObject(desc, obj);
        }
    }

	protected void ShowDescFields()
	{
		m_Show = GUILayout.Toggle(m_Show, string.Format("Show In Inspector[{0}]", m_Members.Count));
		if (m_Show) {
			++EditorGUI.indentLevel;
			EditorGUI.BeginDisabledGroup(true);
			for (int i = 0; i < m_Members.Count; ++i) {
				var member = m_Members[i];
				var attr = System.Attribute.GetCustomAttribute(member, typeof(DescriptionAttribute)) as DescriptionAttribute;
				string desc = attr.description == null ? member.Name : attr.description;

				if (member is FieldInfo) {
                    ShowObject(desc, (member as FieldInfo).GetValue(target));
				} else if (member is PropertyInfo) {
                    ShowObject(desc, (member as PropertyInfo).GetValue(target, null));
				}
			}
			EditorGUI.EndDisabledGroup();
			--EditorGUI.indentLevel;
		}
	}

    protected void DefaultInspector()
    {
        ShowClassTip();
        base.OnInspectorGUI();
    }

	public override void OnInspectorGUI ()
	{
        DefaultInspector();
		EditorGUILayout.Separator();
		ShowDescFields();
	}
}
