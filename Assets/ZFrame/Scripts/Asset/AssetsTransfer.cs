using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace ZFrame.Asset
{
    public class AssetsTransfer : IDisposable
    {

        const int BLOCK_SIZ = 1024 * 100;

        static Queue<AssetsTransfer> queTransfer = new Queue<AssetsTransfer>();
        static AssetsTransfer handling;
        static public bool IsTransfering { get { return handling != null; } }
        static long memoryUsed;
        static public long Memories { get { return memoryUsed; } }
        static public int Count;
        static public int Total;
        static public bool IsLock { get { return Count < Total; } }

        public byte[] srcBytes; //源文件
        public string dstFile;  //目标文件
        public Stream stream;   //流文件

        public AssetsTransfer(byte[] src, string dst)
        {
            srcBytes = src;
            dstFile = dst;
            memoryUsed += src.Length;
        }

        public void Begin()
        {
            if (File.Exists(dstFile)) {
                return;
            } else {
                queTransfer.Enqueue(this);
            }

            if (handling == null) {
                StartTransfer();
            }
        }

        private void onWritten(IAsyncResult iar)
        {
            memoryUsed -= stream.Length;
            stream.Close();

            StartTransfer();
        }

        private void Transfer()
        {
            AssetLoader.Log("Transfer -> {0}", dstFile);
            try {
                SystemTools.NeedDirectory(Path.GetDirectoryName(dstFile));
                stream = new FileStream(dstFile, FileMode.Create, FileAccess.Write);
                stream.BeginWrite(srcBytes, 0, srcBytes.Length, new AsyncCallback(onWritten), Path.GetFileName(dstFile));
            } catch (System.Exception e) {
                LogMgr.E(e.ToString());
            }
        }

        private bool m_Disposed;

        public void Dispose()
        {
            Dispose(true);

            System.GC.SuppressFinalize(this);
        }

        ~AssetsTransfer()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_Disposed) return;
            if (disposing) {
                if (stream != null) {
                    stream.Dispose();
                }
            }

            m_Disposed = true;
        }

        public static void StartTransfer()
        {
            if (handling != null) handling.Dispose();

            if (queTransfer.Count > 0) {
                handling = queTransfer.Dequeue();
                handling.Transfer();
            } else {
                handling = null;
            }
        }

        public static void StopTransfer()
        {
            if (handling != null) handling.Dispose();

            if (queTransfer.Count > 0) {
                queTransfer.Clear();
                LogMgr.I("Transfering Stopped");
            }
        }
    }
}
