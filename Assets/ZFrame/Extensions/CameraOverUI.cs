using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame.UGUI
{
    [RequireComponent(typeof(Camera))]
    public class CameraOverUI : MonoBehaviour
    {
        [SerializeField]
        private float m_StdAspect = 1.777778f;

        private float m_RawFov;

        private void UpdateFOV(int w, int h)
        {
            var aspect = (float)w / h;

            var cam = (Camera)GetComponent(typeof(Camera));
            cam.fieldOfView = aspect < m_StdAspect ? m_RawFov * m_StdAspect / aspect : m_RawFov;
        }

        private void Awake()
        {
            m_RawFov = ((Camera)GetComponent(typeof(Camera))).fieldOfView;
            if (AssetsMgr.Instance) AssetsMgr.Instance.onResolutionChanged += UpdateFOV;
            UpdateFOV(Screen.width, Screen.height);
        }

        private void OnDestroy()
        {
            if (AssetsMgr.Instance) AssetsMgr.Instance.onResolutionChanged -= UpdateFOV;
        }
    }
}
