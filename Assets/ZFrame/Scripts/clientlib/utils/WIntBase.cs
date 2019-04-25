using System;
using System.Collections.Generic;
using System.Text;

namespace clientlib.utils
{
    public abstract class WIntBase
    {
        private const int DICT_SIZE = 16;
        private static byte[] srcDict = new byte[DICT_SIZE];
        private static byte[] destDict = new byte[DICT_SIZE];

        private byte[] _saveValue;
        private byte[] _readValue;

        static WIntBase()
        {
            for (byte i = 0; i < DICT_SIZE; i++)
            {
                srcDict[i] = i;
            }

            Random rand = new Random();
            Array.Sort(srcDict, new Comparison<byte>((b, c) => { return b==c?0:(rand.Next(2)==0?-1:1); }));

            for (byte i = 0; i < DICT_SIZE; i++)
            {
                destDict[srcDict[i]] = i;
            }
        }

        protected WIntBase(int size)
        {
            _saveValue = new byte[size];
            _readValue = new byte[size];
        }

        public byte[] saveValue
        {
            get
            {
                return _saveValue;
            }
        }

        protected byte[] readValue()
        {
            byte[] _readV = _readValue;
            for (int i = 0; i < _saveValue.Length; i++)
            {
                _readV[i] = (byte)(srcDict[(_saveValue[i] & 0x0F)] | (srcDict[((_saveValue[i] >> 4) & 0x0F)] << 4));
            }
            return _readV;
        }

        protected void writeValue(byte[] byteV)
        {
            for (int i = 0; i < _saveValue.Length; i++)
            {
                if (i < byteV.Length)
                {
                    _saveValue[i] = (byte)(destDict[(byteV[i] & 0x0F)] | (destDict[((byteV[i] >> 4) & 0x0F)] << 4));
                }
                else
                {
                    _saveValue[i] = (byte)(destDict[0] | (destDict[0] << 4));
                }
            }
        }
    }
}
