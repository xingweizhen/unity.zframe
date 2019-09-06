using UnityEngine;
using System.Collections;
using System.Text;
using TinyJSON;

public static class JSONTools {

    public static System.Enum ToEnum(this Variant self, System.Enum def)
    {
        var enumType = def.GetType();
        if (enumType.BaseType == typeof(System.Enum)) {
            try {
                if (self is ProxyNumber) {
                    var intVal = (int)self;
                    if (System.Enum.IsDefined(enumType, intVal)) {
                        def = (System.Enum)System.Enum.ToObject(enumType, intVal);
                    }
                } else if (self is ProxyString) {
                    var value = (string)self;
                    if (!string.IsNullOrEmpty(value)) {
                        def = (System.Enum)System.Enum.Parse(enumType, value, true);
                    }
                }
            } catch (System.Exception e) {
                LogMgr.E(string.Format("JSON->Enum({0}) Fail: {1}", enumType.FullName, e.Message));
            }
        }

        return def;
    }

    public static float ConvTo(this Variant self, string key, float defautValue)
    {
        Variant ret = self[key];
        return ret != null ? (float)ret : defautValue;
    }

    public static bool ConvTo(this Variant self, string key, bool defautValue)
    {
        Variant ret = self[key];
        return ret != null ? (bool)ret : defautValue;
    }

    public static string ConvTo(this Variant self, string key, string defautValue)
    {
        Variant ret = self[key];
        return ret != null ? (string)ret : defautValue;
    }

    public static System.Enum ConvTo(this Variant self, string key, System.Enum defautValue)
    {
        Variant ret = self[key];
        return ret != null ? ret.ToEnum(defautValue) : defautValue;
    }
    
    public static Variant ToVariant(this System.Object self)
    {
        return JSON.Load(JSON.Dump(self));
    }
}
