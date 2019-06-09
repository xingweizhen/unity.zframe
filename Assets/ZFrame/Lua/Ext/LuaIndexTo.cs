using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if ULUA
using LuaInterface;
#else
using XLua;
#endif
using TinyJSON;
using ZFrame;
using ILuaState = System.IntPtr;

public static class LuaIndexTo
{
    #region 全局值压栈
    public static void GetGlobal(this ILuaState self, string key1, object key2)
    {
        self.GetGlobal(key1);
        if (self.IsTable(-1)) {
            self.PushAnyObject(key2);
            self.GetTable(-2);
            self.Replace(-2);
        }
    }

    public static void GetGlobal(this ILuaState self, string key1, object key2, object key3)
    {
        self.GetGlobal(key1, key2);
        if (self.IsTable(-1)) {
            self.PushAnyObject(key3);
            self.GetTable(-2);
            self.Replace(-2);
        }
    }

    #endregion

    /// <summary>
    /// 把指定索引的值转成单精度浮点数
    /// </summary>
    /// <returns>The single.</returns>
    /// <param name="self">Self.</param>
    /// <param name="index">Index.</param>
    public static float ToSingle(this ILuaState self, int index)
    {
        return (float)self.ToNumber(index);
    }

    /// <summary>
    /// 把指定索引的值强制转成字符串
    /// </summary>
    public static string ToLuaString(this ILuaState self, int index)
    {
        LuaTypes luaT = self.Type(index);

        switch (luaT) {
            case LuaTypes.LUA_TSTRING:
            case LuaTypes.LUA_TNUMBER:
                return self.ToString(index);
            case LuaTypes.LUA_TBOOLEAN:
                return self.ToBoolean(index).ToString();
            case LuaTypes.LUA_TNIL:
            case LuaTypes.LUA_TNONE:
                return null;
            default:
                self.GetGlobal("tostring");
                self.PushValue(index);
                self.PCall(1, 1, 0);
                var ret = self.ToString(-1);
                self.Pop(1);
                return ret;
        }
    }

    /// <summary>
    /// 把指定索引的值转成LuaTable类型
    /// </summary>
    public static LuaTable ToLuaTable(this ILuaState self, int index)
    {
        if (self.IsTable(index)) {
            self.PushValue(index);
            var translator = ObjectTranslatorPool.Instance.Find(self);
            return new LuaTable(self.L_Ref(LuaIndexes.LUA_REGISTRYINDEX), translator.luaEnv);
        }
        return null;
    }

    /// <summary>
    /// 把指定索引的值转成LuaFunction类型
    /// </summary>
    public static LuaFunction ToLuaFunction(this ILuaState self, int index)
    {
        if (self.IsFunction(index)) {
            self.PushValue(index);
            var translator = ObjectTranslatorPool.Instance.Find(self);
            return new LuaFunction(self.L_Ref(LuaIndexes.LUA_REGISTRYINDEX), translator.luaEnv);
        }
        return null;
    }

    public static Object L_ToUnityObject(this ILuaState self, int index)
    {
        self.GetField(index, "root");
        var obj = self.ToUserData(-1) as Object;
        self.Pop(1);
        
        self.GetField(index, "com");
        string comName = null;
        System.Type comType = null;
        if (self.IsUserData(-1)) {
            comType = self.ToUserData(-1) as System.Type;
        } else {
            comName = self.OptString(-1, null);
        }
        self.Pop(1);

        var root = Object2GameObject(obj);
        GameObject go = null;

        self.GetField(index, "path");
        if (self.IsNumber(-1)) {
            if (root) {
                var trans = root.transform.GetChild(self.ToInteger(-1));
                go = trans ? trans.gameObject : null;
            }
        } else {
            var path = self.OptString(-1, null);
            
            if (string.IsNullOrEmpty(path)) {
                go = root;
            } else {
                if (root) {
                    var trans = root.transform.Find(path);
                    go = trans ? trans.gameObject : null;
                } else {
                    go = GoTools.Seek(path);
                }
            }
        }
        self.Pop(1);

        if (go != null) {
            if (comType != null) return go.GetComponent(comType);
            if (!string.IsNullOrEmpty(comName)) return go.GetComponent(comName);
        }
        
        return go;
    }

    /// <summary>
    /// 把指定索引的值转成一个UnityEngine.Object
    /// </summary>
    public static Object ToUnityObject(this ILuaState self, int index)
    {
        var luaT = self.Type(index);
        switch (luaT) {
            case LuaTypes.LUA_TSTRING:
                return GoTools.Seek(self.ToString(index));
            case LuaTypes.LUA_TUSERDATA: return self.ToUserData(index) as Object;
            case LuaTypes.LUA_TTABLE:
                return self.L_ToUnityObject(index);
            default: break;
        }

        return null;
    }

    private static GameObject Object2GameObject(Object obj)
    {
        var go = obj as GameObject;
        if (go) {
            return go;
        } else {
            var com = obj as Component;
            if (com) return com.gameObject;
        }
        return null;
    }

    /// <summary>
    /// 把指定索引的值转成一个UnityEngine.GameObject
    /// 如果索引的值是一个字符串，将调用GameObject.Find来尝试找到一个GameObject
    /// </summary>
    public static GameObject ToGameObject(this ILuaState self, int index)
    {
        var obj = self.ToUnityObject(index);
        var com = obj as Component;
        if (com) return com.gameObject;

        return obj as GameObject;
    }

    /// <summary>
    /// 把指定索引的值转成指定类型的UnityEngine.Component
    /// 如果类型不匹配，尝试获取其相关的GameObject，
    /// 再从GameoObject上获取其挂载的Component
    /// </summary>
    public static Component ToComponent(this ILuaState self, int index, System.Type type)
    {
        var obj = self.ToUnityObject(index);
        if (obj == null) return null;

        if (type.IsInstanceOfType(obj)) {
            return obj as Component;
        }

        var com = obj as Component;
        if (com) return com.GetComponent(type);

        var go = obj as GameObject;
        if (go) return go.GetComponent(type);

        return null;
    }
    /// <summary>
    /// 把指定索引的值转成指定类型的UnityEngine.Component。泛型版本
    /// </summary>
    public static T ToComponent<T>(this ILuaState self, int index) where T : Component
    {
        return self.ToComponent(index, typeof(T)) as T;
    }

    public static object ToYieldValue(this ILuaState self, int index)
    {
        var luaT = self.Type(index);
        switch (luaT) {
            case LuaTypes.LUA_TNIL:
            case LuaTypes.LUA_TNONE:
                return null;
            case LuaTypes.LUA_TNUMBER:
                return Yields.Seconds(self.ToSingle(index));
            case LuaTypes.LUA_TSTRING: {
                    var str = self.ToString(index);
                    float seconds;
                    if (float.TryParse(str, out seconds)) {
                        return Yields.RealSeconds(seconds);
                    }
                }
                break;
            default: break;
        }

        return self.ToAnyObject(index);
    }

    /// <summary>
    /// 把栈顶的数值转为一个Variant表示的值
    /// </summary>
    public static Variant ToJsonValue(ILuaState L)
    {
        var luaT = L.Type(-1);
        Variant ret = null;
        switch (luaT) {
            case LuaTypes.LUA_TTABLE: ret = L.ToJsonObj(); break;
            case LuaTypes.LUA_TBOOLEAN: ret = new ProxyBoolean(L.ToBoolean(-1)); break;
            case LuaTypes.LUA_TSTRING: ret = new ProxyString(L.ToString(-1)); break;
            case LuaTypes.LUA_TNUMBER: {
                    if (L.IsLong(-1)) {
                        ret = new ProxyNumber(L.ToLong(-1));
                    } else {
                        ret = new ProxyNumber(L.ToNumber(-1));
                    }
                    break;
                }
            case LuaTypes.LUA_TFUNCTION: ret = new ProxyUserdata(L.ToLuaFunction(-1)); break;
            case LuaTypes.LUA_TLIGHTUSERDATA:
            case LuaTypes.LUA_TUSERDATA: ret = new ProxyUserdata(L.ToUserData(-1)); break;
            case LuaTypes.LUA_TNIL:
            case LuaTypes.LUA_TNONE: break;

            default:
                LogMgr.W("ToJsonValue: Unsupport type {0}", luaT);
                break;
        }
        return ret;
    }

    /// <summary>
    /// 把栈顶的表转为一个ProxyObject或ProxyArray
    /// 转为ProxyArray时，其下标可能和Lua表不一致
    /// </summary>
    public static Variant ToJsonObj(this ILuaState self)
    {
        Variant ret = null;
        var top = self.GetTop();
        self.PushNil();
        if (self.Next(top)) {
            var key = self.ToAnyObject(-2) as string;
            if (key != null) {
                var obj = new ProxyObject();
                ret = obj;
                var value = ToJsonValue(self);
                obj[key] = value;
                self.Pop(1);
                while (self.Next(top)) {
                    key = self.ToString(-2);
                    value = ToJsonValue(self);
                    obj[key] = value;
                    self.Pop(1);
                }
            } else {
                var array = new ProxyArray();
                ret = array;
                array.Add(ToJsonValue(self));
                self.Pop(1);
                while (self.Next(top)) {
                    array.Add(ToJsonValue(self));
                    self.Pop(1);
                }
            }
        } else {
            return new ProxyArray();
        }
        return ret;
    }

    /// <summary>
    /// 把指定索引的Lua表转为一个TinyJson类
    /// </summary>
    public static Variant ToJsonObj(this ILuaState self, int index)
    {
        Variant ret = null;
        if (self.IsTable(index)) {
            self.PushValue(index);
            ret = self.ToJsonObj();
            self.Pop(1);
        }
        return ret;
    }


    /// <summary>
    /// Unity的结构体(Vector2等），也以Lua表形式保存，所以这里要判断一下
    /// </summary>
    public static object ToAnyTable(this ILuaState self, int index)
    {
        if (self.GetMetaTable(index)) {
            self.PushString(TableAPI.META_CLASS);
            self.RawGet(-2);

            if (!self.IsNil(-1)) {
                var klass = self.ToLuaString(-1);
                self.Pop(2);
                
                switch (klass) {
                    case UnityEngine_Vector2.CLASS:
                        return self.ToVector2(index);
                    case UnityEngine_Vector3.CLASS:
                        return self.ToVector3(index);
                    case UnityEngine_Vector4.CLASS:
                        return self.ToVector4(index);
                    case UnityEngine_Quaternion.CLASS:
                        return self.ToQuaternion(index);
                    case UnityEngine_Bounds.CLASS:
                        return self.ToBounds(index);
                    case UnityEngine_Color.CLASS:
                        return self.ToColor(index);
                    case "UnityObject":
                        return self.L_ToUnityObject(index);
                }
            } else {
                self.Pop(2);
            }
        }

        return self.ToLuaTable(index);
    }

    /// <summary>
    /// 将指定栈位置的数据转为它确切的类型
    /// 即：自动判断其类型，然后做转换；包括Unity内置类型。
    /// </summary>
    public static object ToAnyObject(this ILuaState self, int index)
    {
        var luaT = self.Type(index);
        switch (luaT) {
            case LuaTypes.LUA_TNUMBER:
                return (float)self.ToNumber(index);
            case LuaTypes.LUA_TSTRING:
                return self.ToString(index);
            case LuaTypes.LUA_TUSERDATA:
                return self.ToTranslator().FastGetCSObj(self, index);
            case LuaTypes.LUA_TBOOLEAN:
                return self.ToBoolean(index);
            case LuaTypes.LUA_TTABLE:
                return self.ToAnyTable(index);
            case LuaTypes.LUA_TFUNCTION:
                return self.ToLuaFunction(index);
            default:
                return null;
        }
    }

    #region 转换多个索引

    public static T[] ToArray<T, TRet>(this ILuaState self, System.Func<ILuaState, int, TRet> To, int index)
    {
        if (index < 0) index = self.GetTop() + 1 + index;

        LuaTypes luaT = self.Type(index);

        if (luaT == LuaTypes.LUA_TTABLE) {
            var type = typeof(T);
            int n = self.ObjLen(index);
            var array = new T[n];
            var i = 0;
            self.PushNil();
            while (self.Next(index)) {
                array[i++] = (T)System.Convert.ChangeType(To(self, -1), type);
                self.Pop(1);
            }
            return array;
        } else if (luaT == LuaTypes.LUA_TUSERDATA) {
            return (T[])self.ChkUserData(index, typeof(T[]));
        }

        return null;
    }

    public static bool[] ToArrayBoolean(this ILuaState self, int index)
    {
        return self.ToArray<bool, bool>(LuaExt.ToBoolean, index);
    }

    public static string[] ToArrayString(this ILuaState self, int index)
    {
        return self.ToArray<string, string>(LuaExt.ToString, index);
    }

    public static T[] ToArrayNumber<T>(this ILuaState self, int index)
    {
        return self.ToArray<T, double>(LuaExt.ToNumber, index);
    }

    public static T[] ToArrayObject<T>(this ILuaState self, int index)
    {
        return self.ToArray<T, object>(LuaIndexTo.ToAnyObject, index);
    }

    public static string[] ToParamsString(this ILuaState L, int index, int count)
    {
        var strs = new string[count];
        for (int i = 0; i < strs.Length; ++i) {
            strs[i] = L.ToLuaString(index + i);
        }
        return strs;
    }

    public static object[] ToParamsObject(this ILuaState L, int index, int count)
    {
        if (count > 0) {
            var objs = new object[count];
            for (int i = 0; i < objs.Length; ++i) {
                objs[i] = L.ToAnyObject(index + i);
            }
            return objs;
        }
        return null;
    }

    public static T[] ToParamsObject<T>(this ILuaState L, int index, int count)
    {
        if (count > 0) {
            var objs = new T[count];
            for (int i = 0; i < objs.Length; ++i) {
                objs[i] = (T)L.ToAnyObject(index + i);
            }
            return objs;
        }
        return null;
    }

    public static object ToStringArg(this ILuaState self, int index)
    {
        var luaT = self.Type(index);
        switch (luaT) {
            case LuaTypes.LUA_TBOOLEAN:
                return self.ToBoolean(index);
            case LuaTypes.LUA_TNUMBER:
                return self.ToNumber(index);
            case LuaTypes.LUA_TSTRING:
                return self.ToString(index);
            default:
                return self.ToLuaString(index);
        }
    }

    public static void ToStringFromatArgs(this ILuaState self, int index, int nArgs, out string format, out object[] args)
    {
        format = self.ChkString(index);
        args = new object[nArgs];
        index += 1;
        for (int i = 0; i < nArgs; ++i) {
            args[i] = self.ToStringArg(index + i);
        }
    }

    public static string ToCSFormatString(this ILuaState self, int index)
    {
        var nArgs = self.GetTop() - index;

        if (nArgs == 0) {
            return self.ToString(index);
        }

        if (nArgs == 1) {
            return string.Format(self.ToString(index), self.ToStringArg(index + 1));
        }

        if (nArgs == 2) {
            return string.Format(self.ToString(index), self.ToStringArg(index + 1),
                self.ToStringArg(index + 2));
        }

        if (nArgs == 3) {
            return string.Format(self.ToString(index), self.ToStringArg(index + 1),
                self.ToStringArg(index + 2), self.ToStringArg(index + 3));
        }

        string format;
        object[] args;
        self.ToStringFromatArgs(index, nArgs, out format, out args);
        return string.Format(format, args);
    }

    #endregion

    public class KeyValue<TKey, TValue>
    {
        public TKey key;
        public TValue value;
        private KeyValue() { }

        public static readonly KeyValue<TKey, TValue> Tmp = new KeyValue<TKey, TValue>();
    }
    public static IEnumerator<KeyValue<TKey, TValue>> ToEnumerator<TKey, TValue>(this ILuaState self, int index)
    {
        if (self.IsTable(index)) {
            self.AbsIndex(ref index);
            self.PushNil();
            while (self.Next(index)) {
                var kv = KeyValue<TKey, TValue>.Tmp;
                self.ToTranslator().Get(self, -1, out kv.key);
                self.ToTranslator().Get(self, -2, out kv.value);
                self.Pop(1);
                yield return kv;
            }
        }
    }

#region 临时

    public static ObjectTranslator ToTranslator(this ILuaState self)
    {
        return ObjectTranslatorPool.Instance.Find(self);
    }
#endregion
}
