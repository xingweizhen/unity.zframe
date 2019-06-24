using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ZFrame.Settings
{
	public class BuildPlayerSettings : ScriptableObject
	{
		[SerializeField] private string m_AndroidKeystoreName;
		[SerializeField] private string m_AndroidKeyaliasPass;
		[SerializeField] private string m_AndroidkeystorePass;

		public void ApplyAndroidKeystore()
		{
			if (!string.IsNullOrEmpty(m_AndroidKeystoreName)) PlayerSettings.Android.keystoreName = m_AndroidKeystoreName;
			if (!string.IsNullOrEmpty(m_AndroidKeyaliasPass)) PlayerSettings.Android.keyaliasPass = m_AndroidKeyaliasPass;
			if (!string.IsNullOrEmpty(m_AndroidkeystorePass)) PlayerSettings.Android.keystorePass = m_AndroidkeystorePass;
		}
	}
}
