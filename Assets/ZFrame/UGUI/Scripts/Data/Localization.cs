using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TinyJSON;

namespace ZFrame.UGUI
{
    [SettingsMenu("Resources", "本地化文件")]
    public class Localization : ScriptableObject
    {
        public const char SEP = ',';

        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("localizeText")]
        [NamedProperty("导出文件")]
        private TextAsset m_LocalizeText;
#if UNITY_EDITOR
        [System.Serializable]
        public struct CustomLoc
        {
            public string key, value;
            public override string ToString()
            {
                return string.Format("[{0}={1}]", key, value);
            }
        }

        [SerializeField]
        private CustomLoc[] m_CustomTexts;
        public CustomLoc[] customTexts { get { return m_CustomTexts; } set { m_CustomTexts = value; } }

        public IEnumerator<CustomLoc> GetIterator(string lang)
        {
            var langIdx = FindLangIndex(lang);
            foreach (var kv in m_Dict) {
                yield return new CustomLoc { key = kv.Key, value = kv.Value[langIdx] };
            }
        }
#endif

        private string[] m_Langs;
        public string[] langs { get { return m_Langs; } }

        private Dictionary<string, string[]> m_Dict;

        public string[] GetKeys()
        {
            return m_Dict.Keys.ToArray();
        }

        private int m_DefLang = -1;
        public string defLang {
            get { return m_Langs[m_DefLang]; }
            set {
                if (!InitLocalization()) return;

                for (int i = 0; i < m_Langs.Length; ++i) {
                    if (string.CompareOrdinal(value, m_Langs[i]) == 0) {
                        m_DefLang = i;
                        return;
                    }
                }
            }
        }

        private int m_CurrentLang;
        public string currentLang {
            get { return m_Langs[m_CurrentLang]; }
            set {
                if (!InitLocalization()) return;

                for (int i = 0; i < m_Langs.Length; ++i) {
                    if (string.CompareOrdinal(value, m_Langs[i]) == 0) {
                        m_CurrentLang = i;
                        return;
                    }
                }

                if (m_DefLang >= 0) {
                    m_CurrentLang = m_DefLang;
                    LogMgr.W("不存在该本地化配置：{0}，默认使用{1}。", value, defLang);
                } else {
                    LogMgr.W("不存在该本地化配置：{0}，也没有默认语言。", value);
                }
            }
        }

        private bool InitLocalization()
        {
            if (m_Dict == null) {
                m_Dict = new Dictionary<string, string[]>();
                // 加载本地化文本
                if (m_LocalizeText) {
                    LoadLocalization(m_LocalizeText.text, out m_Langs, m_Dict);
                } else {
                    LogMgr.W("本地化设置失败：本地化文本不存在");
                }
            }

            return m_Dict != null;
        }

        public void Reset()
        {
            m_Langs = null;
            m_Dict = null;
        }

        public int FindLangIndex(string lang)
        {
            for (int i = 0; i < m_Langs.Length; ++i) {
                if (string.Compare(lang, m_Langs[i], true) == 0) {
                    return i;
                }
            }
            return -1;
        }

        private string Internal_Get(string key, int lang)
        {
            string ret = null;
            string[] values;
            if (m_Dict.TryGetValue(key, out values)) {                
                if (values.Length > lang) {
                    ret = values[lang];
                }

                if (string.IsNullOrEmpty(ret) && m_DefLang >= 0) {
                    ret = values[m_DefLang];
                }
            }

            return ret;
        }

        public string Get(string key, int lang)
        {
            if (m_Dict == null) {
                LogMgr.W("本地化配置未初始化。");
                return key;
            }

            return Internal_Get(key, lang);
        }

        public string Get(string key, string lang)
        {
            if (m_Dict == null) {
                LogMgr.W("本地化配置未初始化。");
                return key;
            }

            var langIdx = -1;
            for (int i = 0; i < m_Langs.Length; ++i) {
                if (m_Langs[i] == lang) {
                    langIdx = i;
                    break;
                }
            }

            if (langIdx < 0) return null;

            return Internal_Get(key, langIdx);
        }

        /// <summary>
        /// 获取本地化文本，如果不存在则返回null值
        /// </summary>
        public string Get(string key)
        {
            if (m_Dict == null) {
                LogMgr.W("本地化配置未初始化。");
                return key;
            }

            return Internal_Get(key, m_CurrentLang);
        }

        public IEnumerator<string> Find(string value, string lang)
        {
            var langIdx = FindLangIndex(lang);
            foreach (var kv in m_Dict) {
                var loc = kv.Value[langIdx];
                if (string.CompareOrdinal(value, loc) == 0) yield return kv.Key;
            }
        }

        /// <summary>
        /// 更新一个本地化配置
        /// </summary>
        public bool Set(string key, int langIdx, string value)
        {
            string[] values;
            if (m_Dict.TryGetValue(key, out values) && langIdx < values.Length && values[langIdx] != value) {
                values[langIdx] = value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 设置或者更改一个本地化配置
        /// </summary>
        public void Set(string key, string value, bool forceSet = false)
        {
            if (m_Dict == null) {
                LogMgr.W("本地化配置未初始化。");
                return;
            }

            if (m_CurrentLang < m_Langs.Length) {
                string[] values;
                m_Dict.TryGetValue(key, out values);

                if (values == null) {
                    values = new string[m_Langs.Length];
                    m_Dict[key] = values;

                    values[0] = key;
                } else if (values.Length < m_Langs.Length) {
                    System.Array.Resize(ref values, m_Langs.Length);
                }
                for (int i = 1; i < m_Langs.Length; ++i) {
                    if (i == m_CurrentLang) {
                        if (forceSet || string.IsNullOrEmpty(values[i])) {
                            values[i] = value;
                        }
                    } else if (values[i] == null) {
                        values[i] = string.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// 判断一个key是否被本地化
        /// </summary>
        public bool IsLocalized(string key)
        {
            if (m_Dict == null) {
                LogMgr.W("本地化配置未初始化。");
                return false;
            }

            string[] values;
            if (m_Dict.TryGetValue(key, out values)) {
                if (values.Length > m_CurrentLang) {
                    return true;
                }
            }

            return false;
        }

        public static string[] SplitCsvLine(string line)
        {
            return (from System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(line,
                    @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)",
                    System.Text.RegularExpressions.RegexOptions.ExplicitCapture)
                    select m.Groups[1].Value).ToArray();
        }

        /// <summary>
        /// 加载本地化数据
        /// </summary>
        public static void LoadLocalization(string text, out string[] langs, Dictionary<string, string[]> dict)
        {
            text = text.Trim();
            using (System.IO.StringReader reader = new System.IO.StringReader(text)) {
                // 表头
                var header = reader.ReadLine();
                if (string.IsNullOrEmpty(header)) {
                    header = string.Format("KEY{0}cn{0}en", SEP);
                }

                langs = SplitCsvLine(header);

                for (; ; ) {
                    var line = reader.ReadLine();
                    if (line == null) break;

                    var values = SplitCsvLine(line);
                    if (values.Length > 0) {
                        if (values.Length < langs.Length) {
                            System.Array.Resize(ref values, langs.Length);
                        }
                        for (int i = 0; i < values.Length; i++) {
                            if (values[i] != null) {
                                values[i] = values[i].Replace("\\n", "\n").Replace("\"\"", "\"");
                            } else {
                                values[i] = string.Empty;
                            }
                        }

                        try {
                            dict.Add(values[0], values);
                        } catch (System.Exception) {
                            LogMgr.E("LoadLocalization ERROR:{0}", JSON.Dump(values));
                            throw;
                        }
                    }
                }
            }
        }

#if UNITY_EDITOR

        public void MarkLocalization(List<string> keys)
        {
            keys.AddRange(m_Dict.Keys);
        }

        public void SaveLocalization()
        {
            var path = UnityEditor.AssetDatabase.GetAssetPath(m_LocalizeText);
            var list = new List<string>();

            var strbld = new System.Text.StringBuilder();
            foreach (var values in m_Dict.Values) {
                if (string.IsNullOrEmpty(values[m_CurrentLang])) {
                    LogMgr.D("移除：{0}", values[0]);
                    continue;
                }

                for (int i = 0; i < values.Length; ++i) {
                    var value = values[i].Replace("\r", string.Empty).Replace("\n", "\\n");
                    if (i > 0) strbld.Append(SEP);
                    if (value.Contains(',')) {
                        value = value.Replace("\"", "\"\"");
                        strbld.AppendFormat("\"{0}\"", value);
                    } else {
                        strbld.Append(value);
                    }
                }
                list.Add(strbld.ToString());
                strbld.Remove(0, strbld.Length);
            }
            list.Sort(string.CompareOrdinal);

            using (var stream = new System.IO.StreamWriter(path)) {
                stream.Write("KEY");
                for (int i = 1; i < m_Langs.Length; ++i) {
                    stream.Write(SEP + m_Langs[i]);
                }
                stream.WriteLine();
                foreach (var line in list) {
                    stream.Write(line);
                    stream.WriteLine();
                }
            }
        }
#endif
    }
}

