using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.UGUI
{
	public class RootCamera : MonoBehaviour
	{
		[SerializeField] private Camera m_Camera;

		private void Start()
		{
			m_Camera.enabled = false;
		}
		
		private void OnTransformChildrenChanged()
		{
			m_Camera.enabled = transform.childCount > 0;
		}
	}
}
