using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.UGUI
{
	public class UIGroup : MonoBehaviour
	{
		private class Element
		{
			public GameObject go;
			public int index;
		}
		private readonly List<Element> m_Elements = new List<Element>();
		
		public GameObject Set(int i, int index)
		{
			if (i < m_Elements.Count) {
				var elm = m_Elements[i];
				elm.index = index;
				return elm.go;
			}

			return null;
		}

		public GameObject Get(int i)
		{
			return i < m_Elements.Count ? m_Elements[i].go : null;
		}

		public GameObject Find(int index)
		{
			foreach (var elm in m_Elements) {
				if (elm.index == index) return elm.go;
			}

			return null;
		}

		public void Add(GameObject go)
		{
			m_Elements.Add(new Element { index = m_Elements.Count, go = go });
		}
		
		public void Add(GameObject go, int index)
		{
			m_Elements.Add(new Element { index = index, go = go });
		}

		public void Remove(GameObject go)
		{
			for (int i = 0; i < m_Elements.Count; ++i) {
				var elm = m_Elements[i];
				if (elm.go == go) {
					m_Elements.RemoveAt(i);
					break;
				}
			}
		}
		
		public bool SetIndex(GameObject go, int index)
		{
			foreach (var elm in m_Elements) {
				if (elm.go == go) {
					elm.index = index;
					return true;
				}
			}

			return false;
		}
		
		public bool SetIndex(Component com, int index)
		{
			return com && SetIndex(com.gameObject, index);
		}
		
		public int GetIndex(GameObject go)
		{
			foreach (var elm in m_Elements) {
				if (elm.go == go) return elm.index;
			}
			
			return -1;
		}
		
		public int GetIndex(Component com)
		{
			return com ? GetIndex(com.gameObject) : -1;
		}
	}
}
