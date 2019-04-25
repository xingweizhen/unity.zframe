using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using ZFrame.Asset;

public class GameEntry : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        if (AssetLoader.Instance == null) {
            InitEntry();
        } else {
#if UNITY_EDITOR && !UNITY_STANDALONE
            if (AssetBundleLoader.I) {
                var objs = FindObjectsOfType(typeof(Renderer));
                if (objs != null) {
                    foreach (Renderer rdr in objs) {
                        foreach (var m in rdr.sharedMaterials) {
                            if (m && m.shader) m.shader = Shader.Find(m.shader.name);
                        }
                    }
                }
            }
#endif
        }

        //GameSettings.Instance.GetSettings();
    }

    public static void InitEntry()
    {
        var obj = Resources.Load("AssetsMgr");
        GoTools.NewChild(null, obj as GameObject);
    }
}
