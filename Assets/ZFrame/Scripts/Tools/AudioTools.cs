using UnityEngine;
using System;

public static class AudioTools
{
    public static short[] GetBuffer(this AudioClip self, int offset = 0, int length = 0)
    {
        var len = length;
        if (len == 0) len = self.samples;
        var data = new float[len * self.channels];

        self.GetData (data, offset);

        short[] buffer = new short[data.Length];

        int rescaleFactor = short.MaxValue;

        for (int i = 0; i < data.Length; i++) {
            var intData = (short)(data [i] * rescaleFactor);
            buffer[i] = intData;
        }
        return buffer;
    }

    public static void SetBuffer(this AudioClip self, short[] buffer)
    {
        float[] data = new float[buffer.Length];

        int rescaleFactor = short.MaxValue;

        for (int i = 0; i < buffer.Length; ++i) {
            float intData = buffer[i];
            data [i] = intData / rescaleFactor;
        }

        self.SetData(data, 0);
    }

	public static byte[] GetBytes(this AudioClip self, int offset = 0, int length = 0)
	{
        var len = length;
        if (len == 0) len = self.samples;
		var data = new float[len * self.channels];

		self.GetData (data, 0);

		byte[] bytesData = new byte[data.Length];

        int rescaleFactor = sbyte.MaxValue;

		for (int i = 0; i < data.Length; i++) {
            var intData = (byte)(data [i] * rescaleFactor);
            bytesData[i] = intData;
		}
        
        return bytesData;
	}

	public static void SetBytes(this AudioClip self, byte[] bytes)
    {
        float[] data = new float[bytes.Length];

        int rescaleFactor = sbyte.MaxValue;

		for (int i = 0; i < bytes.Length; i += 2) {
            float intData = (sbyte)bytes[i];
			data [i] = intData / rescaleFactor;
		}

        self.SetData(data, 0);
    }
}
