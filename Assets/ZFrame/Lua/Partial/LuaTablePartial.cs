using ILuaState = System.IntPtr;

#if ULUA

#else
namespace XLua
{
    public partial class LuaTable : LuaBase
    {
        /// <summary>
        /// 把该表的字段field的值压栈
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public ILuaState PushField(string field)
        {
            var L = luaEnv.L;
            push(L);
            L.GetField(-1, field);
            L.Remove(-2);
            return L;
        }

        public bool CallFunc(int nRet, string field)
        {
            var L = PushField(field);
            if (L.IsFunction(-1)) {
                return L.Func(nRet);
            } else L.Pop(1);
            return false;
        }

        public bool CallFunc<T>(int nRet, string field, T a)
        {
            var L = PushField(field);
            if (L.IsFunction(-1)) {
                return L.Func(nRet, a);
            } else L.Pop(1);
            return false;
        }

        public bool CallFunc<T1, T2>(int nRet, string field, T1 a, T2 b)
        {
            var L = PushField(field);
            if (L.IsFunction(-1)) {
                return L.Func(nRet, a, b);
            } else L.Pop(1);
            return false;
        }

        public bool CallFunc<T1, T2, T3>(int nRet, string field, T1 a, T2 b, T3 c)
        {
            var L = PushField(field);
            if (L.IsFunction(-1)) {
                return L.Func(nRet, a, b, c);
            } else L.Pop(1);
            return false;
        }
    }
}
#endif
