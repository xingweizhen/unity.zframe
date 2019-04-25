using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public static class SystemTools
{
    public static string ToString(object obj)
    {
        return obj != null ? obj.ToString() : string.Empty;
    }

    /// <summary>
    /// 获取一个路径所在的目录路径。输入：aaa/bbb/ccc；返回； aaa/bbb
    /// </summary>
    /// <returns></returns>
    public static string GetDirPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;

        var index = path.LastIndexOf('/');
        if (index < 0) return string.Empty;

        return path.Substring(0, index);
    }
    
    /// <summary>
    /// 移除一个文件路径的扩展名。输入: aaa/bbb/ccc.xx；返回：aaa/bbb/ccc
    /// </summary>
    /// <returns></returns>
    public static string TrimPathExtension(string path)
    {
        var idx = path.LastIndexOf('.');
        if (idx < 0) return path;

        return path.Substring(0, idx);
    }


    /// <summary>
    /// 创建一个目录，自动创建不存在的父级目录
    /// </summary>
    /// <param name="path"></param>
    public static void NeedDirectory(string path)
    {
        if (string.IsNullOrEmpty(path) || path == ".") return;

        var dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir)) {
            NeedDirectory(dir);
        }
        Directory.CreateDirectory(path);
    }

    /// <summary>  
    /// 复制指定目录的所有文件  
    /// </summary>  
    /// <param name="sourceDir">原始目录</param>  
    /// <param name="targetDir">目标目录</param>   
    public static void CopyDirectory(string sourceDir, string targetDir, string pattern = "*", System.Func<string, bool> filter = null)
    {
        if (!Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir);

        //复制当前目录文件
        foreach (string srcPath in Directory.GetFiles(sourceDir, pattern)) {
            var fileName = Path.GetFileName(srcPath);
            if (filter != null && filter(srcPath)) {
                continue;
            }
            string dstPath = Path.Combine(targetDir, fileName);
            File.Copy(srcPath, dstPath, true);
        }

        //复制子目录  
        foreach (string srcPath in Directory.GetDirectories(sourceDir)) {
            string dstPath = Path.Combine(targetDir, Path.GetFileName(srcPath));
            CopyDirectory(srcPath, dstPath, pattern, filter);
        }
    }
}
