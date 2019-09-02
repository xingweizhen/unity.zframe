using UnityEngine;
using System.Collections;
using ZFrame;

[CreateAssetMenu(menuName = "资源库/曲线", fileName = "New CurveLib.asset")]
public class CurveLib : ScriptableObject
{
    [System.Serializable]
    public struct NamedCurve
    {
        public string name;
        public AnimationCurve curve;

        private Keyframe[] m_Keys;
        public Keyframe[] Keys {
            get { if (m_Keys == null) m_Keys = curve.keys; return m_Keys; }
        }
    }
    [SerializeField]
    private NamedCurve[] m_Curves;
    
    public AnimationCurve GetCurve(string name)
    {
        for (int i = 0; i < m_Curves.Length; ++i) {
            if (m_Curves[i].name == name) {
                return m_Curves[i].curve;
            }
        }

        this.LogFormat(LogLevel.W, "库中不存在名称为'{0}'的曲线", name);
        return default(AnimationCurve);
    }
}
