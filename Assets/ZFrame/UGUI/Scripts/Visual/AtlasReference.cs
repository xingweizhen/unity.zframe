using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649
namespace ZFrame.UGUI
{
    [CreateAssetMenu(menuName = "资源库/图集引用", fileName = "New Atlas Reference.asset")]
    public class AtlasReference : ScriptableObject
    {
        [System.Serializable]
        private class AtlasRef
        {
            public string source, destina;
        }

        [SerializeField, ElementList]
        private AtlasRef[] m_Refs;

        public string GetRef(string source)
        {
            for (int i = 0; i < m_Refs.Length; ++i) {
                if (string.CompareOrdinal(source, m_Refs[i].source) == 0) return m_Refs[i].destina;
            }
            return source;
        }

        public void GetSources(string destina, ICollection<string> atlasNames)
        {
            atlasNames.Add(destina);
            for (int i = 0; i < m_Refs.Length; ++i) {
                if (string.CompareOrdinal(destina, m_Refs[i].destina) == 0) {
                    atlasNames.Add(m_Refs[i].source);
                }
            }
        }
    }
}
