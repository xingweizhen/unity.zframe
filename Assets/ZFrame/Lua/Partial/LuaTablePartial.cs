using ILuaState = System.IntPtr;

#if ULUA

#else
namespace XLua
{
    using LuaDLL;
    public partial class LuaTable : LuaBase
    {
        public ILuaState PushField(string field)
        {
            var L = luaEnv.L;
            push(L);
            L.GetField(-1, field);
            L.Remove(-2);
            return L;
        }

        public ILuaState CallFunc(int nRet, string field)
        {
            var L = PushField(field);
            if (L.IsFunction(-1)) {
                L.Func(nRet);
            } else L.Pop(1);
            return L;
        }

        public ILuaState CallFunc<T>(int nRet, string field, T a)
        {
            var L = PushField(field);
            if (L.IsFunction(-1)) {
                L.Func(nRet, a);
            } else L.Pop(1);
            return L;
        }

        public ILuaState CallFunc<T1, T2>(int nRet, string field, T1 a, T2 b)
        {
            var L = PushField(field);
            if (L.IsFunction(-1)) {
                L.Func(nRet, a, b);
            } else L.Pop(1);
            return L;
        }

        public ILuaState CallFunc<T1, T2, T3>(int nRet, string field, T1 a, T2 b, T3 c)
        {
            var L = PushField(field);
            if (L.IsFunction(-1)) {
                L.Func(nRet, a, b, c);
            } else L.Pop(1);
            return L;
        }
    }
}
#endif
