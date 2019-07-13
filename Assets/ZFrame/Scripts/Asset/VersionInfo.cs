using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Asset
{
	[CreateAssetMenu(menuName = "资源库/版本信息")]
    [SettingsMenu("Resources", "应用版本号")]
	public class VersionInfo : ScriptableObject
	{
		[SerializeField, HideInInspector]
		private int m_Version;

		[SerializeField, HideInInspector]
		private int m_Code;

        private void Set(int value, int shift)
        {
            m_Version &= ~(0xFF << shift);
            m_Version |= (value & 0xFF) << shift;
        }

        public int major { get { return (m_Version >> 24) & 0xFF; } set { Set(value, 24); } }
        public int minor { get { return (m_Version >> 16) & 0xFF; } set { Set(value, 16); } }
        public int build { get { return (m_Version >> 8) & 0xFF; } set { Set(value, 8); } }
        public int revision { get { return m_Version & 0xFF; } set { Set(value, 0); } }

        public int code {
			get {
				return m_Code;
			}
			set {
#if UNITY_EDITOR
				using (var so = new UnityEditor.SerializedObject(this)) {
					so.FindProperty("m_Code").intValue = value;
					so.ApplyModifiedProperties();
				}
#endif
			}
		}

		public string version {
			get {
				return string.Format("{0}.{1}.{2}.{3}",
					m_Version >> 24, (m_Version >> 16) & 0xFF, (m_Version >> 8) & 0xFF, m_Version & 0xFF);
			}
#if UNITY_EDITOR
			set {
				using (var so = new UnityEditor.SerializedObject(this)) {
					var ver = new System.Version(value);
					so.FindProperty("m_Version").intValue = (ver.Major << 24) + (ver.Minor << 16) + (ver.Build << 8) + ver.Revision;
					so.ApplyModifiedProperties();
				}
			}
#endif		
		}
	}
}
