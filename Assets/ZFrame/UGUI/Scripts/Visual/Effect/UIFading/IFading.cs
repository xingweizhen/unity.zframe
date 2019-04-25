﻿using UnityEngine;
using System.Collections;

namespace ZFrame.UGUI
{
    using Tween;

    public interface IFading
    {        
        ZTweener tweener { get; }
        float lifetime { get; }
        FadeGroup group { get; }
        bool DOFade(bool reset, bool forward);
    }
}
