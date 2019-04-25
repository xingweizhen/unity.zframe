using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrame
{
    public static class Yields
    {
        public static readonly WaitForEndOfFrame EndOfFrame = new WaitForEndOfFrame();

        private static readonly Dictionary<Boxing<int>, WaitForSeconds> WaitSecondsPool = new Dictionary<Boxing<int>, WaitForSeconds>();

        public static WaitForSeconds Seconds(float seconds)
        {
            var secsInt = Mathf.CeilToInt(seconds * 10);

            var wait = WaitSecondsPool.GetValue(secsInt);
            if (wait == null) {
                wait = new WaitForSeconds(secsInt / 10f);
                WaitSecondsPool.Add(secsInt, wait);
            }

            return wait;
        }

        private static readonly Dictionary<Boxing<int>, WaitForSecondsRealtime> WaitRealSecondsPool = new Dictionary<Boxing<int>, WaitForSecondsRealtime>();

        public static WaitForSecondsRealtime RealSeconds(float seconds)
        {
            var secsInt = Mathf.CeilToInt(seconds * 10);

            var wait = WaitRealSecondsPool.GetValue(secsInt);
            if (wait == null) {
                wait = new WaitForSecondsRealtime(secsInt / 10f);
                WaitRealSecondsPool.Add(secsInt, wait);
            }

            return wait;
        }
    }
}
