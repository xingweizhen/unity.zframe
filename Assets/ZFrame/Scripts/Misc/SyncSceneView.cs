using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SyncSceneView : MonoSingleton<SyncSceneView>
{
#if UNITY_EDITOR
    private Camera m_Cam;
    public Transform target;

    private void Start()
    {
        m_Cam = GetComponent(typeof(Camera)) as Camera;
    }

    // Update is called once per frame
    void Update()
    {
        var sceneView = UnityEditor.SceneView.lastActiveSceneView;
        if (sceneView == null) return;
        
        var cam = sceneView.camera;
        cam.fieldOfView = m_Cam.fieldOfView;
        sceneView.orthographic = m_Cam.orthographic;
        sceneView.rotation = m_Cam.transform.rotation;
        sceneView.pivot = target.position;

        var position = cam.transform.position;
        position.z = -Vector3.Distance(m_Cam.transform.position, target.position);
        cam.transform.position = position;

        sceneView.Repaint();
    }
#endif
}
