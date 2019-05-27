using System;
using System.Collections.Generic;
using System.Text;

namespace clientlib.utils
{
    /// <summary>
    /// Int64
    /// </summary>
    public class WInt64 : WIntBase
    {
        private const int BYTE_SIZE = 8;

        private static byte[] _ByteV = new byte[BYTE_SIZE];

        [System.Security.SecuritySafeCritical]
        public static unsafe void GetBytes(long value, byte[] bytes)
        {
            fixed (byte* numPtr = bytes) {
                *(long*)numPtr = value;
            }
        }

        [System.Security.SecuritySafeCritical]
        public static unsafe void GetBytes(ulong value, byte[] bytes)
        {
            fixed (byte* numPtr = bytes) {
                *(ulong*)numPtr = value;
            }
        }

        public WInt64()
            : this(0) { }

        public WInt64(string value)
            : this(Int64.Parse(value)) { }

        public WInt64(long value)
            : base(BYTE_SIZE)
        {
            this.value = value;
        }

        public long value {
            get { return BitConverter.ToInt64(readValue(), 0); }
            set {
                GetBytes(value, _ByteV);
                writeValue(_ByteV);
            }
        }
    }
}
