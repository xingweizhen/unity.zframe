using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ILuaState = System.IntPtr;

namespace ZFrame.NetEngine
{
    using PB_Message = List<PB_Field>;
    using IMessage = clientlib.net.INetMsg;

    public enum PB_Type
    {
        Int32, Int64, Float, String,
    }
    public class PB_Field : IComparer<PB_Field>
    {
        public PB_Type type;
        public string name;
        public int index;
        public bool repeated;
        public PB_Field(PB_Type type, string name, int index, bool repeated)
        {
            this.type = type;
            this.name = name;
            this.index = index;
            this.repeated = repeated;
        }

        int IComparer<PB_Field>.Compare(PB_Field x, PB_Field y)
        {
            return x.index - y.index;
        }
    }
    /*
     * message 123 {
     *     int32 id;
     *     string name;
     * }
     */
    
    public static class ProtoBufSyntax
    {
        private const int LEN_MSG = 6;
        private static char[] SPLIT_FIELD = new char[] { ';' };
        private static char[] SPLIT_SEG = new char[] { ' ', '=' };

        private static PB_Message Error(string line, string reason)
        {
            LogMgr.E(string.Format("{0}:{1}", line, reason));
            return null;
        }

        public static PB_Message Parse(string protoDef)
        {
            protoDef = protoDef.Trim();
            if (!protoDef.StartsWith("message", System.StringComparison.OrdinalIgnoreCase)) {
                return Error(protoDef, "Invalid proto head");
            }

            int indexL = protoDef.IndexOf('{');
            int indexR = protoDef.IndexOf('}');

            // Message ID
            string strId = protoDef.Substring(LEN_MSG, indexL - LEN_MSG - 1).Trim();
            int id = 0;
            if (!int.TryParse(strId, out id)) return Error(strId, "Invalid Message ID");

            PB_Message message = new PB_Message();
            // Message Fields
            string strFields = protoDef.Substring(indexL + 1, indexR - indexL - 2).Trim();
            string[] fields = strFields.Split(SPLIT_FIELD, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < fields.Length; ++i) {
                string[] segs = fields[i].Split(SPLIT_SEG, System.StringSplitOptions.RemoveEmptyEntries);
                bool repeated = segs[0].EndsWith("[]");
                string strType = repeated ? segs[0].Substring(0, segs[0].Length - 2) : segs[0];
                string name = segs[1];
                string strIndex = segs[2];
                PB_Type type;
                try {
                    type = (PB_Type)PB_Type.Parse(typeof(PB_Type), strType, true);
                } catch (System.Exception e) {
                    return Error(fields[i], e.Message);
                }
                int index = 0;
                if (!int.TryParse(strIndex, out index)) return Error(fields[i], "Invalid index.");
                message.Add(new PB_Field(type, name, index, repeated));
            }
            message.Sort();
            return message;
        }

        private static void PushField(this ILuaState self, IMessage nm, PB_Field field)
        {
            switch (field.type) {
                case PB_Type.Int32: self.SetDict(field.name, nm.readU32()); break;
                case PB_Type.Int64: self.SetDict(field.name, nm.readU64()); break;
                case PB_Type.Float: self.SetDict(field.name, nm.readFloat()); break;
                case PB_Type.String: self.SetDict(field.name, nm.readString()); break;
            }
        }

        public static void PushMessage(this ILuaState self, IMessage nm, PB_Message pb)
        {
            self.PushValue(1);
            for (int i = 0; i < pb.Count; ++i) {
                var field = pb[i];
                if (field.repeated) {
                    self.GetField(-1, field.name);
                    if (!self.IsTable(-1)) {
                        self.Pop(1);
                        self.PushString(field.name);
                        self.NewTable();
                    }
                    int count = nm.readU32();
                    for (int n = 0; n < count; ++n) {
                        self.PushField(nm, field);
                    }
                    self.SetTable(-3);
                } else {
                    self.PushField(nm, field);
                }
            }
        }
    }
}
