using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ZFrame.Tween
{
    public delegate void CallbackOnUpdate(ZTweener tw);
    public delegate void CallbackOnComplete(ZTweener tw);
    public delegate float TweenEaseSampler(float start, float end, float value);

	public enum UpdateType 
	{
		Normal, Late, Fixed,
	}

	public enum LoopType
	{
		Restart, Yoyo, Incremental
	}

	public enum RotateMode
	{
		Fast, FastBeyond360, WorldAxisAdd, LocalAxisAdd
	}

	public enum Ease
	{
		Unset,
		Linear,
		InSine, OutSine, InOutSine,
		InQuad, OutQuad, InOutQuad,
		InCubic, OutCubic, InOutCubic,
		InQuart, OutQuart, InOutQuart,
		InQuint, OutQuint, InOutQuint,
		InExpo, OutExpo, InOutExpo,
		InCirc, OutCirc, InOutCirc,
		InElastic, OutElastic, InOutElastic,
		InBack, OutBack, InOutBack,
		InBounce, OutBounce, InOutBounce,
	}

    public partial class ZTweener
    {
        public static readonly TweenEaseSampler[] EaseAlgorithm = new TweenEaseSampler[] {
            null,
            ZTween.linear,
            ZTween.easeInSine, ZTween.easeOutSine, ZTween.easeInOutSine,
            ZTween.easeInQuad, ZTween.easeOutQuad, ZTween.easeInOutQuad,
            ZTween.easeInCubic, ZTween.easeOutCubic, ZTween.easeInOutCubic,
            ZTween.easeInQuart, ZTween.easeOutQuart, ZTween.easeInOutQuart,
            ZTween.easeInQuint, ZTween.easeOutQuint, ZTween.easeInOutQuint,
            ZTween.easeInExpo, ZTween.easeOutExpo, ZTween.easeInOutExpo,
            ZTween.easeInCirc, ZTween.easeOutCirc, ZTween.easeInOutCirc,
            ZTween.easeInElasticDef, ZTween.easeOutElasticDef, ZTween.easeInOutElasticDef,
            ZTween.easeInBackDef, ZTween.easeOutBackDef, ZTween.easeInOutBackDef,
            ZTween.easeInBounce, ZTween.easeOutBounce, ZTween.easeInOutBounce,
        };
    }
}

namespace ZFrame.Tween
{
#if USE_DOTWEEN
    using DG.Tweening;
    using DG.Tweening.Core;

    public partial class ZTweener
    {
        public Tween tween;
        public ZTweener(Tween tw) { tween = tw; }
        public ZTweener(Tween tw, object target)
        {
            tween = tw;
            tween.target = target;
        }

        public object target { get { return tween.target; } }
        public object tag { get { return tween.id; } }
        public float elapsed { get { return tween.ElapsedDirectionalPercentage(); } }
        public float lifetime { get { return tween.Delay() + tween.Duration(); } }
        public float timeScale { get { return tween.timeScale; } set { tween.timeScale = value; } }

        public bool IsTweening()
        {
            return tween != null && tween.IsPlaying();
        }

        public ZTweener SetTag(object tag)
        {
            if (tween != null) tween.SetId(tag);
            return this;
        }

        public ZTweener SetUpdate(UpdateType updateType, bool ignoreTimeScale)
        {
            if (tween != null) tween.SetUpdate((DG.Tweening.UpdateType)updateType, ignoreTimeScale);
            return this;
        }

        public ZTweener StartFrom(object from)
        {
            (tween as Tweener).ChangeStartValue(from);
            return this;
        }

        public ZTweener EndAt(object at)
        {
            (tween as Tweener).ChangeEndValue(at);
            return this;
        }

        /// <summary>
        /// Idle Tweener Only
        /// </summary>
        public ZTweener DelayFor(float time)
        {
            if (tween != null) tween.SetDelay(time);
            return this;
        }

        /// <summary>
        /// Sequence Only
        /// </summary>
        public ZTweener AppendDelay(float time)
        {
            var seq = tween as Sequence;
            if (seq != null) {
                seq.AppendInterval(time);
            } else {
                Debug.LogWarningFormat("Sequence expected for \"AppendInterval\", got {0}", this.GetType().Name);
            }
            return this;
        }

        /// <summary>
        /// Sequence Only
        /// </summary>
        public ZTweener PrependDelay(float time)
        {
            var seq = tween as Sequence;
            if (seq != null) {
                seq.PrependInterval(time);
            } else {
                Debug.LogWarningFormat("Sequence expected for \"PrependDelay\", got {0}", this.GetType().Name);
            }
            return this;
        }

        /// <summary>
        /// Sequence Only
        /// </summary>
        public ZTweener Insert(float pos, ZTweener tw)
        {
            var seq = tween as Sequence;
            if (seq != null) {
                seq.Insert(pos, tw.tween);
            } else {
                Debug.LogWarningFormat("Sequence expected for \"Insert\", got {0}", this.GetType().Name);
            }
            return this;
        }

        /// <summary>
        /// Sequence Only
        /// </summary>
        public ZTweener Join(ZTweener tw)
        {
            var seq = tween as Sequence;
            if (seq != null) {
                seq.Join(tw.tween);
            } else {
                Debug.LogWarningFormat("Sequence expected for \"Join\", got {0}", this.GetType().Name);
            }
            return this;
        }

        public ZTweener LoopFor(int loops, LoopType loopType)
        {
            if (tween != null) tween.SetLoops(loops, (DG.Tweening.LoopType)loopType);
            return this;
        }

        public ZTweener EaseBy(Ease ease)
        {
            if (tween != null) tween.SetEase((DG.Tweening.Ease)ease);
            return this;
        }

        public ZTweener StartWith(CallbackOnUpdate onStart)
        {
            if (tween != null && onStart != null) {
                tween.OnStart(() => onStart.Invoke(this));
            }
            return this;
        }

        public ZTweener UpdateWith(CallbackOnUpdate onUpdate)
        {
            if (tween != null && onUpdate != null) {
                tween.OnUpdate(() => onUpdate.Invoke(this));
            }
            return this;
        }

        public ZTweener CompleteWith(CallbackOnComplete onComplete)
        {
            if (tween != null) {
                if (onComplete != null) {
                    tween.OnComplete(() => onComplete.Invoke(this));
                } else {
                    tween.OnComplete(null);
                }
            }
            return this;
        }

        public ZTweener Reset()
        {
            this.tween.Restart();
            return this;
        }

        public ZTweener Play(bool forward)
        {
            if (tween != null) {
                if (forward) {
                    tween.PlayForward();
                } else {
                    tween.PlayBackwards();
                }
            }
            return this;
        }

        public ZTweener Stop(bool complete = false)
        {
            if (tween != null) tween.Kill(complete);
            return this;
        }

        public YieldInstruction WaitForCompletion()
        {
            return tween.WaitForCompletion();
        }
    }

    public static partial class ZTween
    {
        public static void Init()
        {
            DOTween.Init();
        }

        #region Tween Config
        
        public static int Stop(object tarOrTag, bool complete = false)
        {
            return DOTween.Kill(tarOrTag, complete);
        }
        #endregion

        #region Tween Alpha
        public static ZTweener TweenAlpha(this CanvasGroup self, float to, float duration)
        {
            return new ZTweener(self.DOFade(to, duration));
        }

        public static ZTweener TweenAlpha(this CanvasGroup self, float from, float to, float duration)
        {
            self.alpha = from;
            return new ZTweener(self.DOFade(to, duration).ChangeStartValue(from));
        }

        public static ZTweener TweenAlpha(this Graphic self, float to, float duration)
        {
            return new ZTweener(self.DOFade(to, duration));
        }

        public static ZTweener TweenAlpha(this Graphic self, float from, float to, float duration)
        {
            var c = self.color;
            c.a = from;
            self.color = c;
            return new ZTweener(self.DOFade(to, duration).ChangeStartValue(from));
        }
        #endregion

        #region Tween Color
        public static ZTweener TweenColor(this Graphic self, Color to, float duration)
        {
            return new ZTweener(self.DOColor(to, duration));
        }

        public static ZTweener TweenColor(this Graphic self, Color from, Color to, float duration)
        {
            self.color = from;
            return new ZTweener(self.DOColor(to, duration).ChangeStartValue(from));
        }

        public static ZTweener TweenColor(this Material self, Color to, string property, float duration)
        {
            return new ZTweener(self.DOColor(to, property, duration));
        }

        public static ZTweener TweenColor(this Material self, Color from, Color to, string property, float duration)
        {
            return new ZTweener(self.DOColor(to, property, duration).ChangeStartValue(from));
        }
        #endregion

        #region Tween String
        public static ZTweener TweenFill(this Image self, float to, float duration)
        {
            return new ZTweener(self.DOFillAmount(to, duration));
        }

        public static ZTweener TweenFill(this Image self, float from, float to, float duration)
        {
            self.fillAmount = from;
            return new ZTweener(self.DOFillAmount(to, duration).ChangeStartValue(from));
        }
        #endregion

        #region Tween String
        public static ZTweener TweenString(this Text self, string to, float duration)
        {
            return new ZTweener(self.DOText(to, duration));
        }

        public static ZTweener TweenString(this Text self, string from, string to, float duration)
        {
            self.text = from;
            return new ZTweener(self.DOText(to, duration).ChangeStartValue(from));
        }
        #endregion

        #region Tween Size
        public static ZTweener TweenSize(this RectTransform self, Vector2 to, float duration)
        {
            return new ZTweener(self.DOSizeDelta(to, duration));
        }

        public static ZTweener TweenSize(this RectTransform self, Vector2 from, Vector2 to, float duration)
        {
            self.sizeDelta = from;
            return new ZTweener(self.DOSizeDelta(to, duration).ChangeStartValue(from));
        }
        #endregion

        #region Tween Offset
        public static ZTweener TweenOffset(this Material self, Vector2 to, float duration)
        {
            return new ZTweener(self.DOOffset(to, duration));
        }

        public static ZTweener TweenOffset(this Material self, Vector2 from, Vector2 to, float duration)
        {
            return new ZTweener(self.DOOffset(to, duration).ChangeStartValue(from));
        }
        #endregion

        #region Tween Tilling
        public static ZTweener TweenTilling(this Material self, Vector2 to, float duration)
        {
            return new ZTweener(self.DOTiling(to, duration));
        }

        public static ZTweener TweenTilling(this Material self, Vector2 from, Vector2 to, float duration)
        {
            return new ZTweener(self.DOTiling(to, duration).ChangeStartValue(from));
        }
        #endregion

        #region Tween Anchor Position
        public static ZTweener TweenAnchorPos(this RectTransform self, Vector3 to, float durtion)
        {
            return new ZTweener(self.DOAnchorPos3D(to, durtion));
        }

        public static ZTweener TweenAnchorPos(this RectTransform self, Vector3 from, Vector3 to, float durtion)
        {
            self.anchoredPosition3D = from;
            return new ZTweener(self.DOAnchorPos3D(to, durtion).ChangeStartValue(from));
        }
        #endregion

        #region Tween Position
        public static ZTweener TweenPosition(this Transform self, Vector3 to, float durtion)
        {
            return new ZTweener(self.DOMove(to, durtion));
        }

        public static ZTweener TweenPosition(this Transform self, Vector3 from, Vector3 to, float durtion)
        {
            self.position = from;
            return new ZTweener(self.DOMove(to, durtion).ChangeStartValue(from));
        }
        #endregion

        #region Tween LocalPosition
        public static ZTweener TweenLocalPosition(this Transform self, Vector3 to, float durtion)
        {
            return new ZTweener(self.DOLocalMove(to, durtion));
        }

        public static ZTweener TweenLocalPosition(this Transform self, Vector3 from, Vector3 to, float durtion)
        {
            self.localPosition = from;
            return new ZTweener(self.DOLocalMove(to, durtion).ChangeStartValue(from));
        }
        #endregion

        #region Tween Rotation
        public static ZTweener TweenRotation(this Transform self, Vector3 to, float durtion, RotateMode mode = RotateMode.Fast)
        {
            return new ZTweener(self.DORotate(to, durtion, (DG.Tweening.RotateMode)mode));
        }

        public static ZTweener TweenRotation(this Transform self, Vector3 from, Vector3 to, float durtion, RotateMode mode = RotateMode.Fast)
        {
            self.rotation = Quaternion.Euler(from);
            return new ZTweener(self.DORotate(to, durtion, (DG.Tweening.RotateMode)mode).ChangeStartValue(from));
        }
        #endregion

        #region Tween LocalRotation
        public static ZTweener TweenLocalRotation(this Transform self, Vector3 to, float durtion, RotateMode mode = RotateMode.Fast)
        {
            return new ZTweener(self.DOLocalRotate(to, durtion, (DG.Tweening.RotateMode)mode));
        }

        public static ZTweener TweenLocalRotation(this Transform self, Vector3 from, Vector3 to, float durtion, RotateMode mode = RotateMode.Fast)
        {
            self.localRotation = Quaternion.Euler(from);
            return new ZTweener(self.DOLocalRotate(to, durtion, (DG.Tweening.RotateMode)mode).ChangeStartValue(from));
        }
        #endregion

        #region Tween LocalRotation
        public static ZTweener TweenScaling(this Transform self, Vector3 to, float durtion)
        {
            return new ZTweener(self.DOScale(to, durtion));
        }

        public static ZTweener TweenScaling(this Transform self, Vector3 from, Vector3 to, float durtion)
        {
            self.localScale = from;
            return new ZTweener(self.DOScale(to, durtion).ChangeStartValue(from));
        }
        #endregion

        #region Shake
        public static ZTweener ShakePosition(this Camera self, float duration, float strength = 3f, int vibrato = 10)
        {
            return new ZTweener(self.DOShakePosition(duration, strength, vibrato));
        }

        #endregion

        #region Tween T
        public static ZTweener Tween(this object self, DOGetter<float> getter, DOSetter<float> setter, float to, float duration)
        {
            return new ZTweener(DOTween.To(getter, setter, to, duration), self);
        }

        public static ZTweener Tween(this object self, DOGetter<int> getter, DOSetter<int> setter, int to, float duration)
        {
            return new ZTweener(DOTween.To(getter, setter, to, duration), self);
        }

        public static ZTweener Tween(this object self, DOGetter<Vector2> getter, DOSetter<Vector2> setter, Vector2 to, float duration)
        {
            return new ZTweener(DOTween.To(getter, setter, to, duration), self);
        }

        public static ZTweener Tween(this object self, DOGetter<Vector3> getter, DOSetter<Vector3> setter, Vector3 to, float duration)
        {
            return new ZTweener(DOTween.To(getter, setter, to, duration), self);
        }

        public static ZTweener Tween(this object self, DOGetter<Vector4> getter, DOSetter<Vector4> setter, Vector4 to, float duration)
        {
            return new ZTweener(DOTween.To(getter, setter, to, duration), self);
        }

        public static ZTweener Tween(this object self, DOGetter<Color> getter, DOSetter<Color> setter, Color to, float duration)
        {
            return new ZTweener(DOTween.To(getter, setter, to, duration), self);
        }

        #endregion

        #region Sequence
        public static ZTweener MakeSequence(params ZTweener[] tweens)
        {
            var seq = DOTween.Sequence();
            for (int i = 0; i < tweens.Length; ++i) {
                seq.Append(tweens[i].tween);
            }
            return new ZTweener(seq);
        }
        #endregion
    }
#else
    public partial class ZTweener
    {
        public object target { get; private set; }
        public object tag { get; private set; }
        public float elapsed { get { throw new System.NotImplementedException(); } }
        public float lifetime { get { throw new System.NotImplementedException(); } }
        public float timeScale { get; set; }

        public bool IsTweening()
        {
            return false;
        }

        public ZTweener SetTag(object tag)
        {
            this.tag = tag;
            return this;
        }

        public ZTweener SetUpdate(UpdateType updateType, bool ignoreTimeScale)
        {
            m_Parameter.updateType = updateType;
            m_Parameter.ignoreTimescale = ignoreTimeScale;
            return this;
        }

        public ZTweener DelayFor(float time)
        {
            m_Parameter.delay = time;
            return this;
        }


        public ZTweener LoopFor(int loops, LoopType loopType)
        {
            m_Parameter.loops = loops;
            m_Parameter.loopType = loopType;
            return this;
        }

        public ZTweener EaseBy(Ease ease)
        {
            m_Parameter.ease = ease;
            return this;
        }

        public ZTweener StartFrom(object from)
        {
            return this;
        }

        public ZTweener EndAt(object at)
        {
            return this;
        }

        /// <summary>
        /// Sequence Only
        /// </summary>
        public ZTweener AppendDelay(float time)
        {
            return this;
        }

        /// <summary>
        /// Sequence Only
        /// </summary>
        public ZTweener PrependDelay(float time)
        {
            return this;
        }

        /// <summary>
        /// Sequence Only
        /// </summary>
        public ZTweener Insert(float pos, ZTweener tw)
        {
            return this;
        }

        /// <summary>
        /// Sequence Only
        /// </summary>
        public ZTweener Join(ZTweener tw)
        {
            return this;
        }

        public ZTweener StartWith(CallbackOnUpdate onStart)
        {
            m_OnStart = onStart;
            return this;
        }

        public ZTweener UpdateWith(CallbackOnUpdate onUpdate)
        {
            m_OnUpdate = onUpdate;
            return this;
        }

        public ZTweener CompleteWith(CallbackOnComplete onComplete)
        {
            m_OnComplete = onComplete;
            return this;
        }

        public ZTweener Reset()
        {
            return this;
        }

        public ZTweener Play(bool forward)
        {
            return this;
        }

        public ZTweener Stop(bool complete = false)
        {
            return this;
        }

        public YieldInstruction WaitForCompletion()
        {
            throw new System.NotImplementedException();
        }
    }
    
    public static partial class ZTween
    {
        private static ITweenKit __Kit;
        private static ITweenKit m_Kit {
            get {
#if !ZFRAME_USE_THIRD_PART_TWEEN
                if (__Kit == null) __Kit = new TweenKit();
#endif
                return __Kit;
            }
        }

        public static void Init(ITweenKit kit = null)
        {
            __Kit = kit;
            m_Kit.Init();
        }

        #region Tween Config
        public static bool IsTweening(this object self)
        {
            return m_Kit.IsTweening(self);
        }
        

        public static object SetTag(this object self, object tag)
        {
            return m_Kit.SetTag(self, tag);
        }

        public static object PlayForward(this object self, bool forward)
        {
            return m_Kit.PlayForward(self, forward);
        }

        public static object SetTimeScale(this object self, float timeScale)
        {
            return m_Kit.SetTimeScale(self, timeScale);
        }

        public static object SetUpdate(this object self, UpdateType updateType, bool ignoreTimeScale)
        {
            return m_Kit.SetUpdate(self, updateType, ignoreTimeScale);
        }

        public static object DelayFor(this object self, float time)
        {
            return m_Kit.DelayFor(self, time);
        }

        public static object LoopFor(this object self, int loops, LoopType loopType)
        {
            return m_Kit.LoopFor(self, loops, loopType);
        }

        public static object EaseBy(this object self, Ease ease)
        {
            return m_Kit.EaseBy(self, ease);
        }

        public static object StartFrom(this object self, object from)
        {
            return m_Kit.StartFrom(self, from);
        }

        public static object EndAt(this object self, object at)
        {
            return m_Kit.EndAt(self, at);
        }

        public static object StartWith(this object self, CallbackOnUpdate onStart)
        {
            return m_Kit.StartWith(self, onStart);
        }

        public static object UpdateWith(this object self, CallbackOnUpdate onUpdate)
        {
            return m_Kit.UpdateWith(self, onUpdate);
        }

        public static object CompleteWith(this object self, CallbackOnComplete onComplete)
        {
            return m_Kit.CompleteWith(self, onComplete);
        }

        public static object Stop(this object self, bool complete = false)
        {
            return m_Kit.Stop(self, complete);
        }

        public static int Finish(object tarOrTag, bool complete = false)
        {
            return m_Kit.Finish(tarOrTag, complete);
        }
        #endregion

        #region Tween Alpha
        public static object TweenAlpha(this CanvasGroup self, float to, float duration)
        {
            return m_Kit.TweenAlpha(self, self.alpha, to, duration);
        }

        public static object TweenAlpha(this CanvasGroup self, float from, float to, float duration)
        {
            return m_Kit.TweenAlpha(self, from, to, duration);
        }

        public static object TweenAlpha(this Graphic self, float to, float duration)
        {
            return m_Kit.TweenAlpha(self, self.color.a, to, duration);
        }

        public static object TweenAlpha(this Graphic self, float from, float to, float duration)
        {
            return m_Kit.TweenAlpha(self, from, to, duration);
        }
        #endregion

        #region Tween Color
        public static object TweenColor(this Graphic self, Color to, float duration)
        {
            return m_Kit.TweenColor(self, self.color, to, duration);
        }

        public static object TweenColor(this Graphic self, Color from, Color to, float duration)
        {
            return m_Kit.TweenColor(self, from, to, duration);
        }

        public static ZTweener TweenColor(this Material self, Color to, string property, float duration)
        {
            throw new System.NotImplementedException();
        }

        public static ZTweener TweenColor(this Material self, Color from, Color to, string property, float duration)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Tween FillAmount
        public static object TweenFill(this Image self, float to, float duration)
        {
            return m_Kit.TweenRange(self, self.fillAmount, to, duration);
        }

        public static object TweenFill(this Image self, float from, float to, float duration)
        {
            return m_Kit.TweenRange(self, from, to, duration);
        }
        #endregion

        #region Tween String
        public static ZTweener TweenString(this Text self, string to, float duration)
        {
            throw new System.NotImplementedException();
        }

        public static ZTweener TweenString(this Text self, string from, string to, float duration)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Tween Size
        public static object TweenSize(this RectTransform self, Vector2 to, float duration)
        {
            return m_Kit.TweenSizeDelta(self, self.sizeDelta, to, duration);
        }

        public static object TweenSize(this RectTransform self, Vector2 from, Vector2 to, float duration)
        {
            return m_Kit.TweenSizeDelta(self, from, to, duration);
        }
        #endregion

        #region Tween Offset
        public static ZTweener TweenOffset(this Material self, Vector2 to, float duration)
        {
            throw new System.NotImplementedException();
        }

        public static ZTweener TweenOffset(this Material self, Vector2 from, Vector2 to, float duration)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Tween Tilling
        public static ZTweener TweenTilling(this Material self, Vector2 to, float duration)
        {
            throw new System.NotImplementedException();
        }

        public static ZTweener TweenTilling(this Material self, Vector2 from, Vector2 to, float duration)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Tween Anchor Position
        public static object TweenAnchorPos(this RectTransform self, Vector3 to, float duration)
        {
            return m_Kit.TweenAnchoredPosition(self, self.anchoredPosition3D, to, duration);
        }

        public static object TweenAnchorPos(this RectTransform self, Vector3 from, Vector3 to, float duration)
        {
            return m_Kit.TweenAnchoredPosition(self, from, to, duration);
        }
        #endregion

        #region Tween Position
        public static object TweenPosition(this Transform self, Vector3 to, float duration)
        {
            return m_Kit.TweenPosition(self, self.position, to, duration, Space.World);
        }

        public static object TweenPosition(this Transform self, Vector3 from, Vector3 to, float duration)
        {
            return m_Kit.TweenPosition(self, from, to, duration, Space.World);
        }
        #endregion

        #region Tween LocalPosition
        public static object TweenLocalPosition(this Transform self, Vector3 to, float duration)
        {
            return m_Kit.TweenPosition(self, self.localPosition, to, duration, Space.Self);
        }

        public static object TweenLocalPosition(this Transform self, Vector3 from, Vector3 to, float duration)
        {
            return m_Kit.TweenPosition(self, from, to, duration, Space.Self);
        }
        #endregion

        #region Tween Rotation
        public static object TweenEulerAngles(this Transform self, Vector3 to, float duration)
        {
            return m_Kit.TweenEulerAngles(self, self.eulerAngles, to, duration, Space.World);
        }

        public static object TweenEulerAngles(this Transform self, Vector3 from, Vector3 to, float duration)
        {
            return m_Kit.TweenEulerAngles(self, from, to, duration, Space.World);
        }
        #endregion

        #region Tween LocalRotation
        public static object TweenLocalEulerAngles(this Transform self, Vector3 to, float duration)
        {
            return m_Kit.TweenEulerAngles(self, self.localEulerAngles, to, duration, Space.Self);
        }

        public static object TweenLocalEulerAngles(this Transform self, Vector3 from, Vector3 to, float duration)
        {
            return m_Kit.TweenEulerAngles(self, from, to, duration, Space.Self);
        }
        #endregion

        #region Tween LocalScale
        public static object TweenScaling(this Transform self, Vector3 to, float duration)
        {
            return m_Kit.TweenScaling(self, self.localScale, to, duration);
        }

        public static object TweenScaling(this Transform self, Vector3 from, Vector3 to, float duration)
        {
            return m_Kit.TweenScaling(self, from, to, duration);
        }
        #endregion

        #region Shake
        public static ZTweener ShakePosition(this Camera self, float duration, float strength = 3f, int vibrato = 10)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Tween T
        public static object TweenAny(this object self, TweenGetValue<float> getter, TweenSetValue<float> setter, float from, float to, float duration)
        {
            return m_Kit.TweenAny(self, getter, setter, from, to, duration);
        }

        public static object TweenAny(this object self, TweenGetValue<Vector2> getter, TweenSetValue<Vector2> setter, Vector2 from, Vector2 to, float duration)
        {
            return m_Kit.TweenAny(self, getter, setter, from, to, duration);
        }

        public static object TweenAny(this object self, TweenGetValue<Vector3> getter, TweenSetValue<Vector3> setter, Vector3 from, Vector3 to, float duration)
        {
            return m_Kit.TweenAny(self, getter, setter, from, to, duration);
        }
        public static object TweenAny(this object self, TweenGetValue<Vector4> getter, TweenSetValue<Vector4> setter, Vector4 from, Vector4 to, float duration)
        {
            return m_Kit.TweenAny(self, getter, setter, from, to, duration);
        }
        public static object TweenAny(this object self, TweenGetValue<Color> getter, TweenSetValue<Color> setter, Color from, Color to, float duration)
        {
            return m_Kit.TweenAny(self, getter, setter, from, to, duration);
        }

        #endregion

        #region Sequence
        public static ZTweener MakeSequence(params ZTweener[] tweens)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
#endif
}
