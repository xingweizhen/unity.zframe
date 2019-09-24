using System.Diagnostics;
using UnityEngine;

/// <summary>
/// 序列号字段在面板中显示的名称
/// </summary>
[Conditional(ZFrame.Log.UNITY_EDITOR)]
public class NamedPropertyAttribute : PropertyAttribute
{
    public string name { get; private set; }
    public NamedPropertyAttribute(string name)
    {
        this.name = name;
    }
}

[Conditional(ZFrame.Log.UNITY_EDITOR)]
public class EnumValueAttribute : NamedPropertyAttribute
{
    public string format { get; private set; }
    public EnumValueAttribute(string name = null, string format = "{0}:{1}") : base(name)
    {
        this.format = format;
    }
}

[Conditional(ZFrame.Log.UNITY_EDITOR)]
public class EnumFlagsValueAttribute : PropertyAttribute
{
    public readonly string[] flags;
    public EnumFlagsValueAttribute(System.Type enumType)
    {
        flags = System.Enum.GetNames(enumType);
    }
}

[Conditional(ZFrame.Log.UNITY_EDITOR)]
public class AssetRefAttribute : NamedPropertyAttribute
{
    public System.Type type { get; private set; }
    public int mode { get; private set; }
    /// <summary>
    /// 仅保存包名，不包括资源名
    /// </summary>
    public bool bundleOnly { set { mode = value ? 1 : 0; } get { return mode > 0; } }
    /// <summary>
    /// 仅保存包名的情况下，以名字模式保存（结尾不带/）
    /// </summary>
    public bool nameOnly { set { mode = value ? 2 : 0; } get { return mode > 1; } }

    public AssetRefAttribute(string name = null, System.Type type = null) : base(name)
    {
        this.type = type ?? typeof(Object);
    }
}

[Conditional(ZFrame.Log.UNITY_EDITOR)]
public class AssetPathAttribute : NamedPropertyAttribute
{
    public System.Type type { get; private set; }
    
    public AssetPathAttribute(string name = null, System.Type type = null) : base(name)
    {
        this.type = type ?? typeof(Object);
    }
}

[Conditional(ZFrame.Log.UNITY_EDITOR)]
public class AudioRefAttribute : PropertyAttribute
{
    
}

[Conditional(ZFrame.Log.UNITY_EDITOR)]
public class ElementListAttribute : NamedPropertyAttribute
{
    //public bool allowDrag = true, allowAdd = true, allowRemove = true;
    public ElementListAttribute(string name = null) : base(name)
    {

    }
}

[Conditional(ZFrame.Log.UNITY_EDITOR)]
public class ReadonlyFieldAttribute : PropertyAttribute
{
    
}

[Conditional(ZFrame.Log.UNITY_EDITOR)]
public class NavMeshAreaAttribute : PropertyAttribute
{

}