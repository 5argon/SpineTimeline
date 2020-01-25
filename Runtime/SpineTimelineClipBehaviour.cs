using System;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Playables;

namespace E7.Timeline
{
    [Serializable]
    public class SpineTimelineClipBehaviour : PlayableBehaviour
    {
        public AnimationReferenceAsset animationReference;
        public bool loop;
    }
}