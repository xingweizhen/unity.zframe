using System;
using System.Diagnostics;

namespace ZFrame
{
    [AttributeUsage(AttributeTargets.All), Conditional("UNITY_EDITOR")]
    public class DescriptionAttribute : Attribute
    {
        private string m_Desc;
        public string description { get { return m_Desc; } }
        public DescriptionAttribute(string description = null)
        {
            m_Desc = description;
        }
    }


    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false), Conditional("UNITY_EDITOR")]
    public sealed class SettingsMenuAttribute : Attribute
    {
        public readonly string location;
        public readonly string name;

        public SettingsMenuAttribute(string location, string name)
        {
            this.location = location;
            this.name = name;
        }
    }
}