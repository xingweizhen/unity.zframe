using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ZFrame.Editors
{
    using Asset;
    [CustomEditor(typeof(AssetDownload))]
    public class AssetDownloadEditor : MonoBehaviorEditor
    {
        public override void OnInspectorGUI()
        {
            DefaultInspector();

            var self = (AssetDownload)target;
            using (var itor = self.ForEachDownload()) {
                while (itor.MoveNext()) {
                    var key = itor.Current.Key;
                    var value = itor.Current.Value;
                    EditorGUILayout.LabelField(key, string.Format("{0}/{1}", value.x, value.y));
                }
            }

            EditorGUILayout.Separator();
            ShowDescFields();
        }
    }
}
