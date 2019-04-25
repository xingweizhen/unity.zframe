using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.IO;
using System.Text;
using System.Threading;

namespace ZFrame
{
    using Asset;
    public static class Bundle
    {
        public static void Pack(string inDir, string pattern, string outFile)
        {
            var dir = new DirectoryInfo(inDir);
            FileInfo[] files = dir.GetFiles(pattern, SearchOption.AllDirectories);
            Pack(dir, files, outFile);
        }

        public static void Pack(DirectoryInfo root, ICollection<FileInfo> collection, string outFile)
        {
            new Packer().Start(root, collection, outFile);
        }

        public class Packer : SevenZip.ICodeProgress
        {
            private string m_Root;
            private List<FileInfo> m_Files;
            private string m_SavePath;

            private string m_Name;
            private int m_Size;
            private int m_Index, m_Total;
            public float progress { get; private set; }

            public Packer()
            {
                m_Files = new List<FileInfo>();
            }

            public void Start(DirectoryInfo root, ICollection<FileInfo> collection, string outFile)
            {
                m_Root = root.FullName;
                m_SavePath = outFile;
                m_Index = 0;
                m_Size = 0;
                progress = 0;

                m_Files.Clear();
                foreach (var f in collection) if (f.Exists) m_Files.Add(f);
                m_Total = m_Files.Count;
                if (m_Total == 0) {
                    LogMgr.D("没有文件需要压缩。");
                    return;
                }

                new Thread(new ThreadStart(Processing)).Start();

#if UNITY_EDITOR
                if (!Application.isPlaying) {
                    while (progress < 1) {
                        UnityEditor.EditorUtility.DisplayProgressBar("文件打包",
                            string.Format("正在压缩... {0}[{1}/{2}]", m_Name, m_Index, m_Total), progress);
                        Thread.Sleep(1);
                    }
                    UnityEditor.EditorUtility.ClearProgressBar();
                }
#endif
            }

            public void SetProgress(long inSize, long outSize)
            {
                if (inSize >= 0 && m_Size > 0) {
                    progress = (m_Index - 1 + (float)inSize / m_Size) / m_Total;
                }
            }

            private void Processing()
            {
                SystemTools.NeedDirectory(Path.GetDirectoryName(m_SavePath));
                FileStream fout = File.Create(m_SavePath);
                if (fout == null) goto END;
                var tick = System.DateTime.Now.Ticks;
                var fbw = new BinaryWriter(fout);

                var startIdx = m_Root.Length + 1;
                fbw.Write(m_Total);            
                foreach (var f in m_Files) {
                    m_Index += 1;

                    m_Name = f.FullName.Substring(startIdx).Replace('\\', '/');
                    var nameBytes = Encoding.UTF8.GetBytes(m_Name);
                    fbw.Write(nameBytes.Length);
                    fbw.Write(nameBytes);

                    var fRead = File.Open(f.FullName, FileMode.Open, FileAccess.Read);
                    m_Size = (int)fRead.Length;
                    fbw.Write(Encoding.UTF8.GetBytes(CMD5.MD5Stream(fRead)));

                    fRead.Position = 0;
                    var nbytes = CLZMA.Compress(fRead, null, this);

                    fbw.Write(nbytes.Length);
                    fbw.Write(nbytes, 0, nbytes.Length);

                    fRead.Close();
                }

                fbw.Flush(); fbw.Close(); fout.Close();
                var span = new System.TimeSpan(System.DateTime.Now.Ticks - tick);
                LogMgr.D("压缩完成，耗时：{0:N3}秒", span.TotalSeconds);
END:
                progress = 1f; 
            }
        }

        public class Unpacker : SevenZip.ICodeProgress
        {
            public static bool isQuiting = false;

            private const int INT_SIZE = sizeof(int);
            private const int MD5_LEN = 32;

            /// <summary>
            /// 临时的缓冲区大小，用在要解析的数据被块中断时
            /// </summary>
            private const int TMP_SIZE = 256;

            /// <summary>
            /// 缓冲区
            /// </summary>
            private readonly byte[] m_Buffer;

            private readonly byte[] m_IntBuffer = new byte[INT_SIZE];
            private readonly byte[] m_NameBuffer = new byte[TMP_SIZE];
            
            /// <summary>
            /// 每次读取的缓冲区大小
            /// </summary>
            private readonly int m_BlockSize;

            /// <summary>
            /// 临时缓冲区的使用大小
            /// </summary>
            private int m_TmpSize = 0;

            /// <summary>
            /// 总文件数量
            /// </summary>
            private int m_Total;
            /// <summary>
            /// 正在处理的文件索引（从0到 m-Total-1）
            /// </summary>
            private int m_Index;
            
            /// <summary>
            /// 解包进度
            /// </summary>
            public float progress { get; private set; }

            public System.Exception exception { get; private set; }

            /// <summary>
            /// 当前解包的文件
            /// </summary>
            private FileStream m_Out;
            /// <summary>
            /// 正在解包的文件名
            /// </summary>
            private string m_Name;

            /// <summary>
            /// 当前解包的文件剩余大小
            /// </summary>
            private int m_Remain;
            
            private class FileInf { public string name; public string md5; public int siz; }
            private Queue<FileInf> m_HashQueue;

            public Unpacker(int blockSize)
            {
                m_BlockSize = blockSize;
                m_Buffer = new byte[TMP_SIZE + blockSize];
                m_HashQueue = new Queue<FileInf>();

                AssetLoader.Instance.AppQuit += Abort;
            }

            ~Unpacker()
            {
                AssetLoader.Instance.AppQuit -= Abort;
            }
            
            private void Abort()
            {
                if (m_Stream != null) m_Stream.Close();
                if (m_Out != null) m_Out.Close();
            }

            private string m_Output;
            private FileStream m_Stream;
            public void Start(string bundlePath, string outputPath)
            {
                Assert.IsNull(m_Out);
                SystemTools.NeedDirectory(outputPath);

                m_HashQueue.Clear();
                m_Output = outputPath;
                m_TmpSize = 0;
                m_Remain = 0;
                m_Stream = new FileStream(bundlePath, 
                    FileMode.Open, FileAccess.Read, FileShare.Read, 20480, true);
                
                m_Stream.Read(m_Buffer, 0, INT_SIZE);
                m_Total = System.BitConverter.ToInt32(m_Buffer, 0);
                m_Index = 0;
                progress = 0;
                exception = null;

                LogMgr.D("包{0}总共有{1}个文件", bundlePath, m_Total);

                new Thread(new ThreadStart(Processing)) { Priority = System.Threading.ThreadPriority.Lowest }.Start();
            }
            
            public bool PeekUnpacking(out string name, out string md5, out int siz)
            {
                lock (m_HashQueue) {
                    name = null; md5 = null; siz = 0;
                    if (m_HashQueue.Count > 0) {
                        var map = m_HashQueue.Peek();
                        if (map.siz > 0) {
                            name = map.name;
                            md5 = map.md5;
                            siz = map.siz;
                            m_HashQueue.Dequeue();
                            return true;
                        }
                    }

                    return false;
                }
            }
            
            public void SetProgress(long inSize, long outSize)
            {
                progress = Mathf.Min(0.999f, (m_Index - 1 + (float)inSize / outSize) / m_Total);
            }

            private bool TestBuffer(int offset, int siz, int buffSize)
            {
                return offset + siz <= buffSize;
            }

            private void CacheBuffer(int offset, int buffSize)
            {
                m_TmpSize = buffSize - offset;
                System.Array.Copy(m_Buffer, offset, m_Buffer, TMP_SIZE - m_TmpSize, m_TmpSize);
            }

            private void Handling(int buffSize)
            {
                buffSize += TMP_SIZE;
                var offset = TMP_SIZE - m_TmpSize;
                while (offset < buffSize) {
                    var _offset = offset;
                    if (m_Out == null) {
                        if (TestBuffer(offset, INT_SIZE, buffSize)) {
                            // 文件名
                            var len = System.BitConverter.ToInt32(m_Buffer, offset);
                            offset += INT_SIZE;
                            if (TestBuffer(offset, len, buffSize)) {
                                var name = Encoding.UTF8.GetString(m_Buffer, offset, len);
                                m_Name = name;
                                offset += len;

                                if (TestBuffer(offset, MD5_LEN, buffSize)) {
                                    var md5 = Encoding.UTF8.GetString(m_Buffer, offset, MD5_LEN);
                                    offset += MD5_LEN;

                                    lock (m_HashQueue) {
                                        m_HashQueue.Enqueue(new FileInf() { name = name, md5 = md5 });
                                    }

                                    // 文件长度
                                    if (TestBuffer(offset, INT_SIZE, buffSize)) {
                                        m_Remain = System.BitConverter.ToInt32(m_Buffer, offset);
                                        offset += INT_SIZE;
                                        
                                        // 文件内容
                                        m_Index += 1;
                                        var path = Path.Combine(m_Output, name + ".tmp");
                                        SystemTools.NeedDirectory(Path.GetDirectoryName(path));
                                        m_Out = File.Open(path, FileMode.Create, FileAccess.ReadWrite);
                                    }
                                }
                            }
                        };
                    }

                    if (m_Out == null) {
                        CacheBuffer(_offset, buffSize);
                        return;
                    }

                    m_TmpSize = 0;

                    var nWrite = Mathf.Min(m_Remain, buffSize - offset);
                    m_Out.Write(m_Buffer, offset, nWrite);
                    m_Remain -= nWrite;
                    if (m_Remain == 0) {
                        // 解压
                        m_Out.Seek(0, SeekOrigin.Begin);
                        var saveFile = new FileStream(Path.Combine(m_Output, m_Name), FileMode.Create, FileAccess.ReadWrite);
                        var siz = CLZMA.Decompress(m_Out, m_Out.Length, saveFile, this);

                        // 关闭并删除临时文件
                        m_Out.Close(); m_Out = null;
                        File.Delete(Path.Combine(m_Output, m_Name + ".tmp"));

                        lock (m_HashQueue) {
                            m_HashQueue.Peek().siz = (int)siz;
                        }
                    }
                    offset += nWrite;
                }
            }
            
            private bool TestRead(byte[] buffer, int count)
            {
                var nRead = m_Stream.Read(buffer, 0, count);
                return nRead == count;
            }

            private void Handling()
            {
                for (; !isQuiting;) {
                    if (!TestRead(m_IntBuffer, m_IntBuffer.Length)) break;
                    var nameLen = System.BitConverter.ToInt32(m_IntBuffer, 0);

                    if (!TestRead(m_NameBuffer, nameLen)) break;
                    var name = Encoding.UTF8.GetString(m_NameBuffer, 0, nameLen);

                    if (!TestRead(m_NameBuffer, MD5_LEN)) break;
                    var md5 = Encoding.UTF8.GetString(m_NameBuffer, 0, MD5_LEN);

                    if (!TestRead(m_IntBuffer, m_IntBuffer.Length)) break;
                    var compressedSize = System.BitConverter.ToInt32(m_IntBuffer, 0);

                    m_Index += 1;
                    var savePath = Path.Combine(m_Output, name);
                    SystemTools.NeedDirectory(Path.GetDirectoryName(savePath));
                    //if (File.Exists(savePath)) {
                    //    m_Stream.Seek(compressedSize, SeekOrigin.Current);
                    //    continue;
                    //}

                    var tmpPath = Path.Combine(m_Output, name + ".tmp");
                    var saveFile = new FileStream(tmpPath, FileMode.Create, FileAccess.ReadWrite);
                    var siz = CLZMA.Decompress(m_Stream, compressedSize, saveFile, this);
                    saveFile.Close();
                    File.Delete(savePath);
                    File.Move(tmpPath, savePath);

                    lock (m_HashQueue) {
                        m_HashQueue.Enqueue(new FileInf() { name = name, md5 = md5, siz = (int)siz });
                    }
                }
            }

            private void Processing()
            {
                try {
                    var tick = System.DateTime.Now.Ticks;
                    Handling();
                    //for (;;) {
                    //    var nBytes = m_Stream.Read(m_Buffer, TMP_SIZE, m_BlockSize);
                    //    if (nBytes > 0) {
                    //        // 处理
                    //        Handling(nBytes);
                    //    } else {
                    //        //操作结束
                    //        m_Stream.Close();
                    //        m_Stream = null;
                    //        break;
                    //    }
                    //}
                    progress = 1f;
                    var span = new System.TimeSpan(System.DateTime.Now.Ticks - tick);
                    LogMgr.D("解压完成，耗时：{0:N3}秒", span.TotalSeconds);
                } catch (IOException e) {
                    progress = -1;
                    exception = e;
                } catch (System.Exception e) {                    
                    progress = -1;
                    exception = e;
                } finally {
                    m_Stream.Close();
                    m_Stream = null;
                }
            }
        }
    }

}
