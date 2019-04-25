using System;
using System.IO;

#pragma warning disable 0219,0414
public class CLZMA
{

    static public long LenOfPackage = 0;

    static bool stdInMode = false;

    static Int32 dictionary = 1 << 23;
    static Int32 posStateBits = 2;
    static Int32 litContextBits = 3;
    static Int32 litPosBits = 0;
    static Int32 algorithm = 2;
    static Int32 numFastBytes = 128;
    static string mf = "bt4";
    static bool eos = stdInMode;

    static SevenZip.CoderPropID[] propIDs =
    {
        SevenZip.CoderPropID.DictionarySize,
        SevenZip.CoderPropID.PosStateBits,
        SevenZip.CoderPropID.LitContextBits,
        SevenZip.CoderPropID.LitPosBits,
        SevenZip.CoderPropID.Algorithm,
        SevenZip.CoderPropID.NumFastBytes,
        SevenZip.CoderPropID.MatchFinder,
        SevenZip.CoderPropID.EndMarker
    };
    static object[] properties =
    {
        (Int32)(dictionary),
        (Int32)(posStateBits),
        (Int32)(litContextBits),
        (Int32)(litPosBits),
        (Int32)(algorithm),
        (Int32)(numFastBytes),
        mf,
        eos
    };

    public static byte[] Compress(Stream inStream, string outPath = null, SevenZip.ICodeProgress progress = null)
    {
        byte[] nbytes = null;

        if (inStream != null) {
            bool saveAsFile = !string.IsNullOrEmpty(outPath);
            Stream outStream = saveAsFile ?
                (Stream)new FileStream(outPath, FileMode.Create, FileAccess.ReadWrite) : (Stream)new MemoryStream();
            
            SevenZip.Compression.LZMA.Encoder encoder = new SevenZip.Compression.LZMA.Encoder();
            // 写入属性
            encoder.SetCoderProperties(propIDs, properties);
            encoder.WriteCoderProperties(outStream);
            // 写入输入长度
            Int64 fileSize = eos || stdInMode ? -1 : inStream.Length;
            for (int i = 0; i < 8; i++) {
                outStream.WriteByte((Byte)(fileSize >> (8 * i)));
            }
            encoder.Code(inStream, outStream, fileSize, -1, progress);

            inStream.Close();

            if (!saveAsFile) {
                outStream.Position = 0;
                BinaryReader sr = new BinaryReader(outStream);
                nbytes = sr.ReadBytes((int)outStream.Length);
                sr.Close();
            }
            outStream.Flush();
            outStream.Close();
        }
        return nbytes;
    }


    public static long Decompress(Stream inStream, long inSize, Stream outStream, SevenZip.ICodeProgress progress = null)
    {
        long length = 0;

        if (inStream != null && outStream != null) {
            // 设置属性
            byte[] properties = new byte[5];
            if (inStream.Read(properties, 0, 5) != 5) {
                throw (new Exception("input .lzma is too short"));
            }

            SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
            decoder.SetDecoderProperties(properties);
            // 读出长度
            long outSize = 0;
            for (int i = 0; i < 8; i++) {
                int v = inStream.ReadByte();
                if (v < 0) {
                    throw (new Exception("Can't Read 1"));
                }
                outSize |= ((long)(byte)v) << (8 * i);
            }

            decoder.Code(inStream, outStream, inSize, outSize, progress);
            length = outStream.Length;

            outStream.Flush();
        }

        return length;
    }

    public static byte[] Decompress(Stream inStream, string outPath = "", SevenZip.ICodeProgress progress = null)
    {
        byte[] nbytes = null;

        if (inStream != null) {
            bool saveAsFile = !string.IsNullOrEmpty(outPath);
            Stream outStream = saveAsFile ?
                (Stream)new FileStream(outPath, FileMode.Create, FileAccess.ReadWrite) : (Stream)new MemoryStream();

            // 设置属性
            byte[] properties = new byte[5];
            if (inStream.Read(properties, 0, 5) != 5) {
                throw (new Exception("input .lzma is too short"));
            }

            SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
            decoder.SetDecoderProperties(properties);
            // 读出长度
            long outSize = 0;
            for (int i = 0; i < 8; i++) {
                int v = inStream.ReadByte();
                if (v < 0) {
                    throw (new Exception("Can't Read 1"));
                }
                outSize |= ((long)(byte)v) << (8 * i);
            }

            long compressedSize = inStream.Length - inStream.Position;
            decoder.Code(inStream, outStream, compressedSize, outSize, progress);

            inStream.Close();

            if (!saveAsFile) {
                outStream.Position = 0;
                using (var sr = new BinaryReader(outStream)) {
                    nbytes = sr.ReadBytes((int)outStream.Length);
                    sr.Close();
                }
            }
            outStream.Flush();
            outStream.Close();
        }
        return nbytes;
    }
    
    public static byte[] Compress(byte[] inBytes, string outPath = "", SevenZip.ICodeProgress progress = null)
    {
        return Compress(new MemoryStream(inBytes), outPath, progress);
    }

    public static byte[] Decompress(byte[] inBytes, string outPath = "", SevenZip.ICodeProgress progress = null)
    {
        return Decompress(new MemoryStream(inBytes), outPath, progress);
    }    
}
