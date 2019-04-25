using UnityEngine;
using System.Collections;

namespace ZFrame.Tween
{
    public interface ITweenable
    {
        ZTweener Tween(object from, object to, float duration);
    }

    public interface ITweenable<T>
    {
        ZTweener Tween(T to, float duration);
        ZTweener Tween(T from, T to, float duration);
    }
}
