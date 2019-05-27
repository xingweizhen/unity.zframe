using System;
using System.Collections.Generic;
using System.Text;

namespace clientlib.utils
{
    /// <summary>
    /// Int32
    /// </summary>
    public class WInt32 : WIntBase
    {
        private const int BYTE_SIZE = 4;
        private static byte[] _ByteV = new byte[BYTE_SIZE];

        [System.Security.SecuritySafeCritical]
        public static unsafe void GetBytes(int value, byte[] bytes)
        {
            fixed (byte* numPtr = bytes) {
                *(int*)numPtr = value;
            }
        }

        [System.Security.SecuritySafeCritical]
        public static unsafe void GetBytes(uint value, byte[] bytes)
        {
            fixed (byte* numPtr = bytes) {
                *(uint*)numPtr = value;
            }
        }

        public WInt32()
            : this(0) { }

        public WInt32(int value)
            : base(BYTE_SIZE)
        {
            this.value = value;
        }

        public int value {
            get { return BitConverter.ToInt32(readValue(), 0); }
            set {
                GetBytes(value, _ByteV);
                writeValue(_ByteV);
            }
        }
    }
}
