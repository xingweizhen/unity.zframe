
/*

 * Improved version to C# LibLZF Port:

 * Copyright (c) 2010 Roman Atachiants <kelindar@gmail.com>

 * 

 * Original CLZF Port:

 * Copyright (c) 2005 Oren J. Maurice <oymaurice@hazorea.org.il>

 * 

 * Original LibLZF Library & Algorithm:

 * Copyright (c) 2000-2008 Marc Alexander Lehmann <schmorp@schmorp.de>

 * 

 * Redistribution and use in source and binary forms, with or without modifica-

 * tion, are permitted provided that the following conditions are met:

 * 

 *   1.  Redistributions of source code must retain the above copyright notice,

 *       this list of conditions and the following disclaimer.

 * 

 *   2.  Redistributions in binary form must reproduce the above copyright

 *       notice, this list of conditions and the following disclaimer in the

 *       documentation and/or other materials provided with the distribution.

 * 

 *   3.  The name of the author may not be used to endorse or promote products

 *       derived from this software without specific prior written permission.

 * 

 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR IMPLIED

 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MER-

 * CHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO

 * EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPE-

 * CIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,

 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;

 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,

 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTH-

 * ERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED

 * OF THE POSSIBILITY OF SUCH DAMAGE.

 *

 * Alternatively, the contents of this file may be used under the terms of

 * the GNU General Public License version 2 (the "GPL"), in which case the

 * provisions of the GPL are applicable instead of the above. If you wish to

 * allow the use of your version of this file only under the terms of the

 * GPL and not to allow others to use your version of this file under the

 * BSD license, indicate your decision by deleting the provisions above and

 * replace them with the notice and other provisions required by the GPL. If

 * you do not delete the provisions above, a recipient may use your version

 * of this file under either the BSD or the GPL.

 */

using System;
using System.Runtime.InteropServices;
using clientlib.net;

/* Benchmark with Alice29 Canterbury Corpus

		---------------------------------------

		(Compression) Original CLZF C#

		Raw = 152089, Compressed = 101092

		 8292,4743 ms.

		---------------------------------------

		(Compression) My LZF C#

		Raw = 152089, Compressed = 101092

		 33,0019 ms.

		---------------------------------------

		(Compression) Zlib using SharpZipLib

		Raw = 152089, Compressed = 54388

		 8389,4799 ms.

		---------------------------------------

		(Compression) QuickLZ C#

		Raw = 152089, Compressed = 83494

		 80,0046 ms.

		---------------------------------------

		(Decompression) Original CLZF C#

		Decompressed = 152089

		 16,0009 ms.

		---------------------------------------

		(Decompression) My LZF C#

		Decompressed = 152089

		 15,0009 ms.

		---------------------------------------

		(Decompression) Zlib using SharpZipLib

		Decompressed = 152089

		 3577,2046 ms.

		---------------------------------------

		(Decompression) QuickLZ C#

		Decompressed = 152089

		 21,0012 ms.

	*/





/// <summary>

/// Improved C# LZF Compressor, a very small data compression library. The compression algorithm is extremely fast. 

#pragma warning disable 0219,0414
public static class CLZF2
{
#if false
	private static readonly uint HLOG = 14;

	private static readonly uint HSIZE = (1 << 14);

	private static readonly uint MAX_LIT = (1 << 5);

	private static readonly uint MAX_OFF = (1 << 13);

	private static readonly uint MAX_REF = ((1 << 8) + (1 << 3));



	/// <summary>

	/// Hashtable, that can be allocated only once

	/// </summary>

	private static readonly long[] HashTable = new long[HSIZE];



	// Compresses inputBytes

	public static byte[] Compress(byte[] inputBytes)
	{

		// Starting guess, increase it later if needed

		int outputByteCountGuess = inputBytes.Length * 2;

		byte[] tempBuffer = new byte[outputByteCountGuess];

		int byteCount = lzf_compress(inputBytes, ref tempBuffer);



		// If byteCount is 0, then increase buffer and try again

		while (byteCount == 0)
		{

			outputByteCountGuess *= 2;

			tempBuffer = new byte[outputByteCountGuess];

			byteCount = lzf_compress(inputBytes, ref tempBuffer);

		}



		byte[] outputBytes = new byte[byteCount];

		Buffer.BlockCopy(tempBuffer, 0, outputBytes, 0, byteCount);

		return outputBytes;

	}



	// Decompress outputBytes

	public static byte[] Decompress(byte[] inputBytes)
	{

		// Starting guess, increase it later if needed
		int outputByteCountGuess = inputBytes.Length * 2;
		byte[] tempBuffer = new byte[outputByteCountGuess];
		int byteCount = lzf_decompress(inputBytes, ref tempBuffer);

		// If byteCount is 0, then increase buffer and try again
		while (byteCount == 0)
		{
			outputByteCountGuess *= 2;
			tempBuffer = new byte[outputByteCountGuess];
			byteCount = lzf_decompress(inputBytes, ref tempBuffer);
		}

        // If byteCount is less than 0, throw an IndexOutOfRangeException
        if (byteCount < 0) {
            throw new System.IndexOutOfRangeException("DataError");
        } else {
    		byte[] outputBytes = new byte[byteCount];
    		Buffer.BlockCopy(tempBuffer, 0, outputBytes, 0, byteCount);
    		return outputBytes;
        }
	}



	/// <summary>

	/// Compresses the data using LibLZF algorithm

	/// </summary>

	/// <param name="input">Reference to the data to compress</param>

	/// <param name="output">Reference to a buffer which will contain the compressed data</param>

	/// <returns>The size of the compressed archive in the output buffer</returns>

	public static int lzf_compress(byte[] input, ref byte[] output)
	{

		int inputLength = input.Length;

		int outputLength = output.Length;



		Array.Clear(HashTable, 0, (int)HSIZE);



		long hslot;

		uint iidx = 0;

		uint oidx = 0;

		long reference;



		uint hval = (uint)(((input[iidx]) << 8) | input[iidx + 1]); // FRST(in_data, iidx);

		long off;

		int lit = 0;



		for (; ; )
		{

			if (iidx < inputLength - 2)
			{

				hval = (hval << 8) | input[iidx + 2];

				hslot = ((hval ^ (hval << 5)) >> (int)(((3 * 8 - HLOG)) - hval * 5) & (HSIZE - 1));

				reference = HashTable[hslot];

				HashTable[hslot] = (long)iidx;





				if ((off = iidx - reference - 1) < MAX_OFF

					&& iidx + 4 < inputLength

					&& reference > 0

					&& input[reference + 0] == input[iidx + 0]

					&& input[reference + 1] == input[iidx + 1]

					&& input[reference + 2] == input[iidx + 2]

					)
				{

					/* match found at *reference++ */

					uint len = 2;

					uint maxlen = (uint)inputLength - iidx - len;

					maxlen = maxlen > MAX_REF ? MAX_REF : maxlen;



					if (oidx + lit + 1 + 3 >= outputLength)

						return 0;



					do

						len++;

					while (len < maxlen && input[reference + len] == input[iidx + len]);



					if (lit != 0)
					{

						output[oidx++] = (byte)(lit - 1);

						lit = -lit;

						do

							output[oidx++] = input[iidx + lit];

						while ((++lit) != 0);

					}



					len -= 2;

					iidx++;



					if (len < 7)
					{

						output[oidx++] = (byte)((off >> 8) + (len << 5));

					}

					else
					{

						output[oidx++] = (byte)((off >> 8) + (7 << 5));

						output[oidx++] = (byte)(len - 7);

					}



					output[oidx++] = (byte)off;



					iidx += len - 1;

					hval = (uint)(((input[iidx]) << 8) | input[iidx + 1]);



					hval = (hval << 8) | input[iidx + 2];

					HashTable[((hval ^ (hval << 5)) >> (int)(((3 * 8 - HLOG)) - hval * 5) & (HSIZE - 1))] = iidx;

					iidx++;



					hval = (hval << 8) | input[iidx + 2];

					HashTable[((hval ^ (hval << 5)) >> (int)(((3 * 8 - HLOG)) - hval * 5) & (HSIZE - 1))] = iidx;

					iidx++;

					continue;

				}

			}

			else if (iidx == inputLength)

				break;



			/* one more literal byte we must copy */

			lit++;

			iidx++;



			if (lit == MAX_LIT)
			{

				if (oidx + 1 + MAX_LIT >= outputLength)

					return 0;



				output[oidx++] = (byte)(MAX_LIT - 1);

				lit = -lit;

				do

					output[oidx++] = input[iidx + lit];

				while ((++lit) != 0);

			}

		}



		if (lit != 0)
		{

			if (oidx + lit + 1 >= outputLength)

				return 0;



			output[oidx++] = (byte)(lit - 1);

			lit = -lit;

			do

				output[oidx++] = input[iidx + lit];

			while ((++lit) != 0);

		}



		return (int)oidx;

	}





	/// <summary>
	/// Decompresses the data using LibLZF algorithm
	/// </summary>
	/// <param name="input">Reference to the data to decompress</param>
	/// <param name="output">Reference to a buffer which will contain the decompressed data</param>
	/// <returns>Returns decompressed size</returns>
	public static int lzf_decompress(byte[] input, ref byte[] output)
	{
		int inputLength = input.Length;
		int outputLength = output.Length;

		uint iidx = 0;
		uint oidx = 0;

		do
		{
            if (iidx >= input.Length) goto EXCEPTION_INDEX_OUT_OF_RANGE;
			uint ctrl = input[iidx++];

			if (ctrl < (1 << 5)) /* literal run */
			{
				ctrl++;

				if (oidx + ctrl > outputLength)
				{
					//SET_ERRNO (E2BIG);
					return 0;
				}

				do {
                    if (iidx >= input.Length || oidx >= output.Length) goto EXCEPTION_INDEX_OUT_OF_RANGE;
					output[oidx++] = input[iidx++];
                }
				while ((--ctrl) != 0);

			}
			else /* back reference */
			{
				uint len = ctrl >> 5;

				int reference = (int)(oidx - ((ctrl & 0x1f) << 8) - 1);

				if (len == 7) {
                    if (iidx >= input.Length) goto EXCEPTION_INDEX_OUT_OF_RANGE;
					len += input[iidx++];
                }

                if (iidx >= input.Length) goto EXCEPTION_INDEX_OUT_OF_RANGE;
				reference -= input[iidx++];

				if (oidx + len + 2 > outputLength)
				{
					//SET_ERRNO (E2BIG);
					return 0;
				}

				if (reference < 0)
				{
					//SET_ERRNO (EINVAL);
					return 0;
				}


                if (oidx >= output.Length || reference >= output.Length) goto EXCEPTION_INDEX_OUT_OF_RANGE;
				output[oidx++] = output[reference++];
                if (oidx >= output.Length || reference >= output.Length) goto EXCEPTION_INDEX_OUT_OF_RANGE;
				output[oidx++] = output[reference++];

				do {
                    if (oidx >= output.Length || reference >= output.Length) goto EXCEPTION_INDEX_OUT_OF_RANGE;
					output[oidx++] = output[reference++];
                }
				while ((--len) != 0);

			}

		} while (iidx < inputLength);

		return (int)oidx;

    EXCEPTION_INDEX_OUT_OF_RANGE:
        return -1;
	}

    public static long Package(string inDir, string pattern, string outFile)
    {
        System.IO.FileInfo[] files = new System.IO.DirectoryInfo(inDir).GetFiles(pattern);

        System.IO.FileStream fout = System.IO.File.Create(outFile);
        if (fout == null) return 0;
        System.IO.BinaryWriter fbw = new System.IO.BinaryWriter(fout);

        fbw.Write(files.Length);

        int n = 0;
        int max = files.Length;
        foreach (System.IO.FileInfo f in files)
        {
            n++;
#if UNITY_EDITOR
            if (!UnityEngine.Application.isPlaying) {
                UnityEditor.EditorUtility.DisplayProgressBar("Compress with LZF2 & Package",
                    "Compressing... " + f.Name + "[" + n + "/" + max + "]", (float)n / max);
            }
#endif
            fbw.Write(f.Name.Length);
            fbw.Write(System.Text.Encoding.UTF8.GetBytes(f.Name));
            byte[] nbytes = Compress(System.IO.File.ReadAllBytes(f.FullName));
            fbw.Write(nbytes.Length);
            fbw.Write(nbytes, 0, nbytes.Length);
            UnityEngine.Debug.Log(f.Name.Length + "[" + f.Name + "]" + nbytes.Length);
        }

        long size = fout.Length;
        fbw.Close();
        fout.Close();
#if UNITY_EDITOR
        if (!UnityEngine.Application.isPlaying) {
            UnityEditor.EditorUtility.ClearProgressBar();
        }
#endif
        return size;
    }

    public static void Extract(string inFile, string outDir, System.Collections.Generic.List<string> skipList)
    {
        Extract(inFile, outDir, null, null, skipList);
    }

    public delegate void DelegateProgress(long current, long total, string fileName, long fileSize);
    public delegate void DelegateFinish(string srcFile);
    public static System.Collections.IEnumerator Extract(string inFile, string outDir, DelegateProgress progress, DelegateFinish finish,
        System.Collections.Generic.List<string> skipList = null)
    {
        UnityEngine.Debug.Log(inFile + "->" + outDir);
        bool bFail = false;
        if (System.IO.File.Exists(inFile)) {
            System.IO.FileStream fin = System.IO.File.OpenRead(inFile);
            //UnityEngine.Debug.Log("Open " + inFile + " = " + fin);
            if (fin != null) {
                System.IO.BinaryReader fbr = new System.IO.BinaryReader(fin);
        
                if (!System.IO.Directory.Exists(outDir)) {
                    System.IO.Directory.CreateDirectory(outDir);
                }
                int nFiles = fbr.ReadInt32();
                for (int i = 0; i < nFiles; ++i)
                {
                    yield return 1;
                    int nameLen = fbr.ReadInt32();
                    string fileName = System.Text.Encoding.UTF8.GetString(fbr.ReadBytes(nameLen));
                    int byteLen = fbr.ReadInt32();

                    long fileSize = 0;
                    if (skipList != null && skipList.Contains(fileName)) {
                        UnityEngine.Debug.Log("Skip: " + fileName + " Length: " + byteLen);
                        fbr.ReadBytes(byteLen);
                    } else {
                        UnityEngine.Debug.Log("Extract: " + fileName + " Length: " + byteLen);
                        byte[] nbytes = null;
                        try {
                            if (byteLen > 0) {
                                nbytes = Decompress(fbr.ReadBytes(byteLen));
                            } else {
                                throw new System.IndexOutOfRangeException("ZeroLength");
                            }
                        } catch (System.Exception e) {
                            UnityEngine.Debug.LogError(e.Message);
                            fileSize = -1;
                            bFail = true;
                        }

                        if (nbytes != null) {
                            fileSize = nbytes.LongLength;
                            System.IO.FileStream fs = System.IO.File.Create(outDir + "/" + fileName);
                            fs.Write(nbytes, 0, nbytes.Length);
                            fs.Close();
                        }
                    }
                    if (progress != null) {
                        progress(i + 1, nFiles, fileName, fileSize);
                    }
                    if (bFail) break;
                }
                fbr.Close();
                fin.Close();
            } else {
                UnityEngine.Debug.LogError("Open " + inFile + " = " + fin);
            }
        }
        if (finish != null && !bFail) finish(inFile);
    }
#endif

#if ENABLE_CUSTOM_ENCRYPT_API
    [DllImport(IoBuffer.DLL_NAME)]
    extern static int LZF_Compress(ref System.IntPtr inbytes, int inLen, ref System.IntPtr outbytes, int outlen);
	
    [DllImport(IoBuffer.DLL_NAME)]
    extern static int LZF_Decompress(ref System.IntPtr inbytes, int inLen, ref System.IntPtr outbytes, int outlen);

	public static byte[] DllCompress(byte[] inputBytes)
	{
		IntPtr inBuffer = Marshal.AllocHGlobal(inputBytes.Length);
		Marshal.Copy(inputBytes, 0, inBuffer, inputBytes.Length);
		
		// Starting guess, increase it later if needed
		int outputByteCountGuess = inputBytes.Length * 2;
		IntPtr outBuffer = Marshal.AllocHGlobal(outputByteCountGuess);
		int byteCount = LZF_Compress(ref inBuffer, inputBytes.Length, ref outBuffer, outputByteCountGuess);
		
		// If byteCount is 0, then increase buffer and try again
		while (byteCount == 0)
		{
		    Marshal.FreeHGlobal(outBuffer);
		
		    outputByteCountGuess *= 2;
		    outBuffer = Marshal.AllocHGlobal(outputByteCountGuess);
			byteCount = LZF_Compress(ref inBuffer, inputBytes.Length, ref outBuffer, outputByteCountGuess);
		}
		byte[] outputBytes = new byte[byteCount];
		Marshal.Copy(outBuffer, outputBytes, 0, byteCount);
		
		Marshal.FreeHGlobal(outBuffer);
		Marshal.FreeHGlobal(inBuffer);
		
		return outputBytes;
	}
	
	public static byte[] DllDecompress(byte[] inputBytes)
	{
		IntPtr inBuffer = Marshal.AllocHGlobal(inputBytes.Length);
		Marshal.Copy(inputBytes, 0, inBuffer, inputBytes.Length);
		
		// Starting guess, increase it later if needed
		int outputByteCountGuess = inputBytes.Length * 2;
		IntPtr outBuffer = Marshal.AllocHGlobal(outputByteCountGuess);
		int byteCount = LZF_Decompress(ref inBuffer, inputBytes.Length, ref outBuffer, outputByteCountGuess);
		
		// If byteCount is 0, then increase buffer and try again
		while (byteCount == 0)
		{
		    Marshal.FreeHGlobal(outBuffer);
		
		    outputByteCountGuess *= 2;
		    outBuffer = Marshal.AllocHGlobal(outputByteCountGuess);
			byteCount = LZF_Decompress(ref inBuffer, inputBytes.Length, ref outBuffer, outputByteCountGuess);
		}
		byte[] outputBytes = new byte[byteCount];
		Marshal.Copy(outBuffer, outputBytes, 0, byteCount);
		
		Marshal.FreeHGlobal(outBuffer);
		Marshal.FreeHGlobal(inBuffer);
		
		return outputBytes;
	}

    [DllImport(IoBuffer.DLL_NAME)]
    extern public static void Encrypt(byte[] bytes, int length);

    [DllImport(IoBuffer.DLL_NAME)]
    extern public static void Decrypt(byte[] bytes, int length);
#else
    public static byte[] DllCompress(byte[] inputBytes) { return inputBytes; }

    public static byte[] DllDecompress(byte[] inputBytes) { return inputBytes; }

    public static void Encrypt(byte[] bytes, int length) {}
	
	public static void Decrypt(byte[] bytes, int length) {}
#endif
}

