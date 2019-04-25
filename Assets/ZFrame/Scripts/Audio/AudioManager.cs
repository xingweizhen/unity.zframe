using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

namespace ZFrame
{
    using Asset;

    public interface IAudioMgr
    {
        void PlayUISfx(string sfxName, string defSfx);
    }
    
    public class AudioManager : MonoSingleton<AudioManager>, IAudioMgr
    {
        public const string DUMMY_SFX = "_";
        public static IAudioMgr Audio;
        
        /// <summary>
        /// 录制的语音片段
        /// </summary>
        private static AudioClip s_RecordingClip;
        /// <summary>
        /// 录制的语音采样率
        /// </summary>
        private const int s_RecordingSampleRate = 8000;
        /// <summary>
        /// 录音最大时长
        /// </summary>
        [SerializeField]
        private int m_MaxRecordingLength = 10;

        [SerializeField]
        private AudioMixer m_Mixer;

        [SerializeField]
        private GameObject[] sources;

        private List<AudioSource> m_UniqueSrouces = new List<AudioSource>();

        protected override void Awaking()
        {
            base.Awaking();
            
            Audio = this;
        }

        private void OnClipLoaded(string a, object o, object p)
        {
            var clip = o as AudioClip;
            var src = p as AudioSource;
            if (clip) {
                // 正在播放同样的循环音乐，忽略
                if (src.isPlaying && src.loop && src.clip == clip) return;

                src.clip = clip;
                src.Play();
            }
        }

        public GameObject GetTemplate(string template)
        {
            for (int i = 0; i < sources.Length; ++i) {
                var src = sources[i];
                if (src && src.name == template) {
                    return src;
                }
            }

            return null;
        }

        public AudioSource GetSource(string template)
        {
            var prefab = GetTemplate(template);
            Assert.IsNotNull(prefab,
                string.Format("<AudioSource> with name '{0}' not exist!", template));

            var go = ObjectPoolManager.AddChild(gameObject, prefab);
            go.SetActive(true);
            return go.GetComponent<AudioSource>();
        }

        public AudioSource FindSource(string template)
        {
            for (int i = 0; i < m_UniqueSrouces.Count; ++i) {
                if (m_UniqueSrouces[i].name == template) {
                    return m_UniqueSrouces[i];
                }
            }

            var src = GetSource(template);
            m_UniqueSrouces.Add(src);
            return src;
        }

        public void Play(string clipName, string template)
        {
            var clip = AssetLoader.Instance.Load(typeof(AudioClip), clipName) as AudioClip;
            if (clip) {
                var src = GetSource(template);
                src.clip = clip;
                src.Play();
                ObjectPoolManager.DestroyPooled(src.gameObject, clip.length);
            }
        }

        public void Replay(string clipName, string template)
        {
            var src = FindSource(template);
            AssetLoader.Instance.LoadAsync(typeof(AudioClip), clipName, LoadMethod.Cache, OnClipLoaded, src);
        }

        public void PlayAsync(string clipName, string template)
        {
            var src = GetSource(template);
            AssetLoader.Instance.LoadAsync(typeof(AudioClip), clipName, LoadMethod.Cache, OnClipLoaded, src);
        }

        public void PlayUISfx(string sfxName, string defSfx)
        {
            if (string.IsNullOrEmpty(sfxName)) {
                sfxName = defSfx;
            } else if (string.CompareOrdinal(sfxName, DUMMY_SFX) == 0) {
                return;
            }
            

            Play(sfxName, "Sfx2D");
        }

        public byte[] Recording()
        {
            byte[] nbytes = null;
            if (s_RecordingClip == null) {
                Microphone.End(null);
                s_RecordingClip = Microphone.Start(null, false, m_MaxRecordingLength, s_RecordingSampleRate);
            } else {
                int recordingLength;
                int lastPos = Microphone.GetPosition(null);
                if (Microphone.IsRecording(null)) {
                    recordingLength = lastPos / s_RecordingSampleRate;
                } else {
                    recordingLength = m_MaxRecordingLength;
                }

                Microphone.End(null);

                if (recordingLength >= 1) {
                    nbytes = s_RecordingClip.GetBytes();
                }
                s_RecordingClip = null;
            }
            return nbytes;
        }

        public AudioClip CreateClip(byte[] nbytes, int lengthSamples, int channels, int freq)
        {
            var clip = AudioClip.Create("byte", lengthSamples, channels, freq, false);
            clip.SetBytes(nbytes);
            return clip;
        }

        public void Stop(string template)
        {
            var src = FindSource(template);
            src.Stop();
        }

        public void SetParam(string param, float value)
        {
            if (!m_Mixer.SetFloat(param, value)) {
                LogMgr.W("设置AudioMixer参数'{0}'失败。", param);
            }
        }

        public float GetParam(string param)
        {
            var value = 0f;
            if (m_Mixer.GetFloat(param, out value)) {
                return value;
            }
            LogMgr.W("获取AudioMixer参数'{0}'失败。", param);
            return 0f;
        }
    }
}
