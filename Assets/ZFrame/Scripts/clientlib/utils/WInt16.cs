using System;
using System.Collections.Generic;
using System.Text;

namespace clientlib.utils
{
    /// <summary>
    /// Int16
    /// </summary>
    public class WInt16 : WIntBase
    {
        private const int BYTE_SIZE = 2;

        private static byte[] _ByteV = new byte[BYTE_SIZE];

        [System.Security.SecuritySafeCritical]
        public static unsafe void GetBytes(short value, byte[] bytes)
        {
            fixed (byte* numPtr = bytes) {
                *(short*)numPtr = value;
            }
        }

        [System.Security.SecuritySafeCritical]
        public static unsafe void GetBytes(ushort value, byte[] bytes)
        {
            fixed (byte* numPtr = bytes) {
                *(ushort*)numPtr = value;
            }
        }

        public WInt16()
            : this(0) { }

        public WInt16(short value)
            : base(BYTE_SIZE)
        {
            this.value = value;
        }

        public short value {
            get { return BitConverter.ToInt16(readValue(), 0); }
            set {
                GetBytes(value, _ByteV);
                writeValue(_ByteV);
            }
        }
    }
}
