using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ZFrame.Asset
{
    public class EntryInf
    {
        public string md5;
    }

    [System.Serializable]
    public class AssetInf : EntryInf
    {
        public long siz;
    }
    
    [System.Serializable]
	public class DownloadInf
	{
		public long siz;
		public int minLevel;
		public int maxLevel;
	}

    [System.Serializable]
    public class ResInf
    {
        public string version;
        public int code;
        public string timeCreated;
        public string whoCreated;
        public Dictionary<string, EntryInf> Assets;
        [TinyJSON.SkipIfNull]
        public Dictionary<string, DownloadInf> Downloads;

        public ResInf()
        {
            Assets = new Dictionary<string, EntryInf>();
        }

        public ResInf(ResInf srcInf = null) : this()
        {
            if (srcInf != null) {
                version = srcInf.version;
                code = srcInf.code;
                timeCreated = srcInf.timeCreated;
                whoCreated = srcInf.whoCreated;
            } else {
                timeCreated = "";
                whoCreated = "";
            }
        }
    }
}
