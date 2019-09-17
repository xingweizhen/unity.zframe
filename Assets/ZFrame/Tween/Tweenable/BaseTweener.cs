using UnityEngine;
using System.Collections;

namespace ZFrame.Tween 
{
	public abstract class BaseTweener : MonoBehaviour, ITweenable
	{
		public abstract object Tween(object from, object to, float duration);

		protected virtual void OnDisable()
		{
            ZTween.Finish(this);
		}
	}
}
