using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ZFrame;

public class FXShaderEditor : ShaderGUI
{
    private readonly List<MaterialProperty> m_DistortProps = new List<MaterialProperty>();
    private readonly List<MaterialProperty> m_TimeProps = new List<MaterialProperty>();
    private readonly List<MaterialProperty> m_NTimeProps = new List<MaterialProperty>();

    private void FindProperties(MaterialProperty[] props)
    {
        m_DistortProps.Clear();
        m_TimeProps.Clear();
        m_NTimeProps.Clear();
        foreach (var prop in props) {
            var propName = prop.name;
            if (propName.OrdinalStartsWith("_Distort")) {
                m_DistortProps.Add(prop);
            } else if (propName.OrdinalIgnoreCaseStartsWith("_Time")) {
                m_TimeProps.Add(prop);
            } else if (propName.OrdinalIgnoreCaseStartsWith("_NTime")) {
                m_NTimeProps.Add(prop);
            }
        }
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        FindProperties(properties);

        materialEditor.DefaultProperties(properties);
        
        EditorGUILayout.Separator();
        
        var targetMat = (Material)materialEditor.target;
        
        if (m_TimeProps.Count > 0 || m_NTimeProps.Count > 0) {
            var usingTime = System.Array.IndexOf(targetMat.shaderKeywords, "USING_TIME") != -1;
            if (EditorUtil.KeywordCheck(targetMat, "使用时间值", "USING_TIME", usingTime)) {
                EditorGUI.indentLevel++;
                foreach (var prop in m_TimeProps) materialEditor.DrawProperty(prop);
                EditorGUI.indentLevel--;
            } else {
                EditorGUI.indentLevel++;
                foreach (var prop in m_NTimeProps) materialEditor.DrawProperty(prop);
                EditorGUI.indentLevel--;
            }
        }
        
        EditorGUILayout.Separator();
        
        if (m_DistortProps.Count > 0) {
            var texDistort = System.Array.IndexOf(targetMat.shaderKeywords, "TEX_DISTORT") != -1;
            if (EditorUtil.KeywordCheck(targetMat, "使用贴图扭曲", "TEX_DISTORT", texDistort)) {
                EditorGUI.indentLevel++;
                foreach (var prop in m_DistortProps) materialEditor.DrawProperty(prop);
                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.Separator();
        
        materialEditor.RenderQueueField();
        materialEditor.EnableInstancingField();
        materialEditor.DoubleSidedGIField();

    }
}
