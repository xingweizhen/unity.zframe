using UnityEngine;
using System.Collections;

namespace ZFrame.Tween
{
    public interface ITweenable
    {
        object Tween(object from, object to, float duration);
    }

    public interface ITweenable<T>
    {
        object Tween(T to, float duration);
        object Tween(T from, T to, float duration);
    }
}
