using System;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Playables;

namespace E7.SpineTimeline
{
    [Serializable]
    public class SpineTimelineClipBehaviour : PlayableBehaviour
    {
        public AnimationReferenceAsset animationReference;
        public bool loop;
    }
}