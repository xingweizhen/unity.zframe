//
//  ScriptKeywordReplace.cs
//  ZFrame
//
//  Created by xingweizhen on 10/12/2017.
//
//

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ZFrame.Editors
{
    public class ScriptKeywordReplace : UnityEditor.AssetModificationProcessor
    {
        public readonly static string BOM
            = System.Text.Encoding.UTF8.GetString(new byte[] {0xEF, 0xBB, 0xBF});

        //public static void OnWillCreateAsset(string path)
        //{
        //    path = path.Replace(".meta", "");
        //    int index = path.LastIndexOf(".");
        //    if (index < 0) return;

        //    string file = path.Substring(index);
        //    if (file != ".cs" && file != ".js" && file != ".boo") return;
        //    string fileExtension = file;

        //    index = Application.dataPath.LastIndexOf("Assets");
        //    path = Application.dataPath.Substring(0, index) + path;
        //    file = System.IO.File.ReadAllText(path);

        //    file = file.Replace("#CREATIONDATE#", System.DateTime.Now.ToString("d"));
        //    file = file.Replace("#PROJECTNAME#", PlayerSettings.productName);
        //    file = file.Replace("#AUTHOR#", System.Environment.UserName);
        //    file = file.Replace("#FILEEXTENSION#", fileExtension);

        //    System.IO.File.WriteAllText(path, BOM + file);
        //    AssetDatabase.Refresh();
        //}
    }
}