using UnityEngine;
using System.Collections;
#if false
using LuaInterface;
using ILuaState = System.IntPtr;
#if UNITY_5
using Assert = UnityEngine.Assertions.Assert;
#else
using Assert = ZFrame.Assertions.Assert;
#endif

public static class WrapToLua
{
    #region Wrap Methods

    /// <summary>
    /// 元表名字
    /// </summary>
    public static string GetMetaName(string fullName) { return fullName; }

    [MonoPInvokeCallback(typeof(LuaCSFunction))]
    public static int import(ILuaState L)
    {
        var libName = L.ChkString(1);
        Assert.IsFalse(string.IsNullOrEmpty(libName));

        L.GetGlobal("package", "loaded", libName);
        if (L.IsNil(-1)) {
            L.Pop(1);
            var type = LuaBinder.Bind(L, libName);
            if (type != null) {
                // [类表][类元表]
                L.PushValue(-1);
                // [类表][类元表][类元表]
                L.SetMetaTable(-3);
                // [类表][类元表]
                BindBaseType(L, type);
            } else {
                L.PushNil();
                LogMgr.W("import failure: {0} isn't wrapped", libName);
            }
        }
        return 1;
    }

    /// <summary>
    /// 注册一个新的类型表(+1)
    /// </summary>
    public static string RegistType(this ILuaState self, System.Type type)
    {
        var libName = type.FullName.Replace('+', '.');

        self.GetGlobal("package", "loaded");
        self.NewTable();
        self.PushValue(-1);
        self.SetField(-3, libName);
        self.Replace(-2);

        return GetMetaName(libName);
    }

    /// <summary>
    /// 把函数和字段注册到栈顶的表中
    /// </summary>
    public static void RegistMembers(this ILuaState self, LuaMethod[] methods, LuaField[] fields)
    {
        // 成员函数
        if (methods != null) {
            for (int i = 0; i < methods.Length; i++) {
                self.PushString(methods[i].name);
                self.PushCSharpProtectedFunction(methods[i].func);
                self.RawSet(-3);
            }
        }

        // 字段/属性([1]=get; [2]=set)
        if (fields != null) {
            for (int i = 0; i < fields.Length; i++) {
                self.PushString(fields[i].name);
                self.CreateTable(2, 0);

                if (fields[i].getter != null) {
                    self.PushCSharpProtectedFunction(fields[i].getter);
                    self.RawSetI(-2, 1);
                }

                if (fields[i].setter != null) {
                    self.PushCSharpProtectedFunction(fields[i].setter);
                    self.RawSetI(-2, 2);
                }

                self.RawSet(-3);
            }
        }
    }

    /// <summary>
    /// 根据配置在Lua环境生成类表，并生成一个元表，保留类型表和元表在栈顶(+2)
    /// 调用后需要设置元表为类表的元表
    /// </summary>
    public static void WrapCSharpObject(this ILuaState self, System.Type type, LuaMethod[] methods, LuaField[] fields)
    {
        // 保存类型
        var metaName = self.RegistType(type);

        // 元表
        self.L_NewMetaTable(metaName);

        // 元方法放入元表
        self.SetToLuaIndex();
        self.SetToLuaNewIndex();
        self.SetDict("__tostring", MetaMethods.LToString);
        self.SetDict("__gc", MetaMethods.GC);
        self.SetDict("__eq", MetaMethods.Eq);
        self.SetRegistryIndex("__call", MetaMethods.ToLua_TableCall);
        self.SetDict("CLASS", metaName);

        self.RegistMembers(methods, fields);
    }
    
    /// <summary>
    /// 在Lua环境生成一个空的类表，并生成一个元表，保留类型表和元表在栈顶(+2)
    /// 调用后需要设置元表为类表的元表
    /// </summary>
    public static string WrapCSharpObject(this ILuaState self, System.Type type)
    {
        // 保存类型
        var metaName = self.RegistType(type);

        // 元表
        self.L_NewMetaTable(metaName);

        // 元方法放入元表
        self.SetToLuaIndex();
        self.SetToLuaNewIndex();
        self.SetDict("__tostring", MetaMethods.LToString);
        self.SetDict("__gc", MetaMethods.GC);
        self.SetDict("CLASS", metaName);

        return metaName;
    }
    
    /// <summary>
    /// 绑定基类，不会改变栈大小
    /// </summary>
    public static void BindBaseType(ILuaState L, System.Type type)
    {
        // 和基类连接
        var baseType = type.BaseType;
        while (baseType != null) {
            // [...][派生类元表]

            // 尝试绑定基类
            var subTypeName = baseType.FullName.Replace('+', '.');
            var subMetaName = GetMetaName(subTypeName);

            L.L_GetMetaTable(subMetaName);
            if (L.IsNil(-1)) {
                L.Pop(1);
                if (LuaBinder.Bind(L, subTypeName) == null) {
                    L.WrapCSharpObject(baseType);
                }
                // [...][派生类元表][基类表][基类元表]
                L.PushValue(-1);
                L.PushValue(-1);
                // [...][派生类元表][基类表][基类元表][基类元表][基类元表]
                L.SetMetaTable(-4);
                // [...][派生类元表][基类表][基类元表][基类元表]
                L.SetMetaTable(-4);
                // [...][派生类元表][基类表][基类元表]
                L.Remove(-3);
                // [...][基类表][基类元表]
                L.Remove(-2);
            } else {
                // [...][派生类元表][基类元表]
                L.PushValue(-1);
                // [...][派生类元表][基类元表][基类元表]
                L.SetMetaTable(-3);
                // [...][派生类元表][基类元表]
                L.Remove(-2);
            }

            baseType = baseType.BaseType;
        }
        L.Remove(-1);
    }

    /// <summary>
    /// 绑定CSharp类型的方法和属性
    /// 会自动绑定基类，最总保留该类表在栈顶(+1)
    /// </summary>
    public static string BindRecursively(this ILuaState self, System.Type type)
    {
        var typeName = type.FullName.Replace('+', '.');
        var metaName = GetMetaName(typeName);
        self.L_GetMetaTable(metaName);
        if (!self.IsNil(-1)) return metaName;
        self.Pop(1);
        
        // 导入类型
        if (LuaBinder.Bind(self, typeName) == null) {
            self.WrapCSharpObject(type);
        }
        // [类表][类元表]
        self.PushValue(-1);
        // [类表][类元表][类元表]
        self.SetMetaTable(-3);
        // [类表][类元表]

        // 和基类连接
        BindBaseType(self, type);

        return metaName;
    }

    #endregion
}
#endif
