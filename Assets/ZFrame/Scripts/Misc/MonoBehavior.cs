using UnityEngine;
using System.Collections;

public class MonoBehavior : MonoBehaviour
{
    private Transform m_Trans;
    public Transform cachedTransform { get { if (!m_Trans) m_Trans = transform; return m_Trans; } }
}
