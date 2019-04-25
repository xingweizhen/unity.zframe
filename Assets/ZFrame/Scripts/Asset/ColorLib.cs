using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

[CreateAssetMenu(menuName = "资源库/颜色", fileName = "New ColorLib.asset")]
public class ColorLib : ScriptableObject
{
    [System.Serializable]
    public struct NamedColor
    {
        public string name;
        public Color color;
    }

    [System.Serializable]
    public struct TextColor
    {
        public string name;
        public Color outline;
        public Color gradient1, gradient2;

        public static readonly TextColor Empty = new TextColor() {
            outline = Color.black, gradient1 = Color.white, gradient2 = Color.black,
        };
    }

    [SerializeField, HideInInspector]
    private NamedColor[] m_NamedColors;

    [SerializeField, HideInInspector]
    [FormerlySerializedAs("m_FontColors")]
    private TextColor[] m_TextColors;

    public Color GetColor(string name, bool warnIfMissing = true)
    {
        for (int i = 0; i < m_NamedColors.Length; ++i) {
            if (m_NamedColors[i].name == name) {
                return m_NamedColors[i].color;
            }
        }

        if (warnIfMissing)
            LogMgr.W("库中不存在名称为'{0}'的颜色", name);
        
        return Color.clear;
    }

    public TextColor GetTextColor(string name, bool warnIfMissing = true)
    {
        for (int i = 0; i < m_TextColors.Length; ++i) {
            if (m_TextColors[i].name == name) {
                return m_TextColors[i];
            }
        }

        if (warnIfMissing)
            LogMgr.W("库中不存在名称为'{0}'的字体颜色", name);
        
        return TextColor.Empty;
    }
}