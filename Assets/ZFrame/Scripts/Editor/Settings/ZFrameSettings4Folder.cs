using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.Settings
{
	public abstract class ZFrameSettings4Folder : ScriptableObject
	{
		[System.Serializable]
		protected class SettingsBase
		{
			[NamedProperty("名称"), HideInInspector] public string name;
			[HideInInspector] public List<string> folders;

			[SerializeField, HideInInspector] protected bool m_Disable;

			// Settings Fields
		}

		// 设置列表的变量名必须定义为`m_SettingsList`
		// [SerializeField, HideInInspector]
		// private List<SettingsBase> m_SettingsList;
	}
}
