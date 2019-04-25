using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ZFrame.UGUI;

namespace ZFrame.Asset
{
    [CreateAssetMenu(menuName = "Settings/ArtStandardChecker")]
    public class ArtStandardChecker : ScriptableObject
    {
        [System.Serializable]
        public class CheckConfig
        {
            [NamedProperty("规范类别"), Tooltip("定义一个名称，比如：角色、怪物或物件")]
            public string cfgName;

            [NamedProperty("贴图大小")]
            public int textureSize;
            [NamedProperty("三角面")]
            public int triangles;
            [NamedProperty("骨骼数")]
            public int bones;
            public List<string> paths;

            public void Check()
            {
                foreach (var path in paths) {
                    if (bones > 0 || triangles > 0) {
                        var files = System.IO.Directory.GetFiles(path, "*.FBX", System.IO.SearchOption.AllDirectories);
                        foreach (var file in files) {
                            var model = AssetDatabase.LoadAssetAtPath(file, typeof(GameObject)) as GameObject;
                            if (!model) continue;

                            Mesh mesh = null;
                            var meshFilter = model.GetComponent(typeof(MeshFilter)) as MeshFilter;
                            if (meshFilter) {
                                mesh = meshFilter.sharedMesh;
                            } else {
                                var skin = model.GetComponentInChildren(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
                                if (skin) {
                                    mesh = skin.sharedMesh;
                                    if (bones > 0 && skin.bones.Length > bones) {
                                        CheckFailure(model, file, "模型骨骼数量", bones, skin.bones.Length);
                                    }
                                }
                            }

                            if (mesh && triangles > 0) {
                                var tris = mesh.triangles.Length / 3;
                                if (tris > triangles) {
                                    CheckFailure(model, file, "模型三角面", triangles, tris);
                                }
                            }
                        }
                    }


                    if (textureSize > 0) {
                        var files = System.IO.Directory.GetFiles(path, "*.tga", System.IO.SearchOption.AllDirectories);
                        foreach (var file in files) {
                            var tex = AssetDatabase.LoadAssetAtPath(file, typeof(Texture2D)) as Texture2D;
                            if (!tex) continue;

                            if (tex.width > textureSize || tex.height > textureSize) {
                                CheckFailure(tex, file, "贴图大小", textureSize, Mathf.Max(tex.width, tex.height));
                            }
                        }
                    }
                }
            }

            private static void CheckFailure(Object context, string file, string item, int expected, int real)
            {
                if (real > expected * 1.5f) {
                    Debug.LogErrorFormat(context, "[{0}] {1}超标：期望值<={2}，实际={3}", file, item, expected, real);
                } else {
                    Debug.LogWarningFormat(context, "[{0}] {1}超标：期望值<={2}，实际={3}", file, item, expected, real);
                }
            }
        }

        [SerializeField, HideInInspector]
        private List<CheckConfig> m_Cfgs;

        public void Check(int index = -1)
        {
            if (index < 0) {
                foreach (var cfg in m_Cfgs) {
                    cfg.Check();
                }
            } else if (index < m_Cfgs.Count) {
                m_Cfgs[index].Check();
            }
        }
    }
}
