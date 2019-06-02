using System.Diagnostics;
using UnityEngine;

/// <summary>
/// 序列号字段在面板中显示的名称
/// </summary>
[Conditional(LogMgr.UNITY_EDITOR)]
public class NamedPropertyAttribute : PropertyAttribute
{
    public string name { get; private set; }
    public NamedPropertyAttribute(string name)
    {
        this.name = name;
    }
}

[Conditional(LogMgr.UNITY_EDITOR)]
public class EnumValueAttribute : NamedPropertyAttribute
{
    public string format { get; private set; }
    public EnumValueAttribute(string name = null, string format = "{0}:{1}") : base(name)
    {
        this.format = format;
    }
}

[Conditional(LogMgr.UNITY_EDITOR)]
public class EnumFlagsValueAttribute : PropertyAttribute
{
    public readonly string[] flags;
    public EnumFlagsValueAttribute(System.Type enumType)
    {
        flags = System.Enum.GetNames(enumType);
    }
}

[Conditional(LogMgr.UNITY_EDITOR)]
public class AssetRefAttribute : NamedPropertyAttribute
{
    public System.Type type { get; private set; }
    /// <summary>
    /// 仅保存包名，不包括资源名
    /// </summary>
    public bool bundleOnly;
    public AssetRefAttribute(string name = null, System.Type type = null) : base(name)
    {
        this.type = type ?? typeof(Object);
    }
}

[Conditional(LogMgr.UNITY_EDITOR)]
public class AssetPathAttribute : NamedPropertyAttribute
{
    public System.Type type { get; private set; }
    
    public AssetPathAttribute(string name = null, System.Type type = null) : base(name)
    {
        this.type = type ?? typeof(Object);
    }
}

[Conditional(LogMgr.UNITY_EDITOR)]
public class AudioRefAttribute : PropertyAttribute
{
    
}

[Conditional(LogMgr.UNITY_EDITOR)]
public class ElementListAttribute : NamedPropertyAttribute
{
    //public bool allowDrag = true, allowAdd = true, allowRemove = true;
    public ElementListAttribute(string name = null) : base(name)
    {

    }
}

[Conditional(LogMgr.UNITY_EDITOR)]
public class ReadonlyFieldAttribute : PropertyAttribute
{
    
}

[Conditional(LogMgr.UNITY_EDITOR)]
public class NavMeshAreaAttribute : PropertyAttribute
{

}