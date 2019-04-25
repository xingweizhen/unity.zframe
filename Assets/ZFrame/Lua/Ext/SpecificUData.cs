using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if ULUA
using LuaInterface;
#else
using XLua;
#endif
using ILuaState = System.IntPtr;

/// <summary>
/// 需要特殊处理的UserData
/// </summary>
public static class SpecificUData
{
    /// <summary>
    /// 把一个udata压到栈位置index，并指定它的元表引用
    /// </summary>
    //private static void PushMetaObject(ILuaState L, ObjectTranslator translator, object o, int metaRef)
    //{
    //    if (o == null) {
    //        L.PushNil();
    //        return;
    //    }

    //    int weakTableRef = translator.weakTableRef;
    //    int index = -1;
    //    bool found = translator.objectsBackMap.TryGetValue(o, out index);

    //    if (found) {
    //        if (LuaDLL.tolua_pushudata(L, weakTableRef, index)) {
    //            return;
    //        }

    //        translator.collectObject(index);
    //    }

    //    index = translator.addObject(o, false);
    //    LuaDLL.tolua_pushnewudata(L, metaRef, weakTableRef, index);
    //}    
}
