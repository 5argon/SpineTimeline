using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.Timeline
{
    [Serializable]
    public class SpineTimelineClip : PlayableAsset, ITimelineClipAsset
    {
        public SpineTimelineClipBehaviour template = new SpineTimelineClipBehaviour();

        public ClipCaps clipCaps
        {
            get
            {
                if (template.animationReference != null && template.loop)
                {
                    return ClipCaps.Blending | ClipCaps.ClipIn | ClipCaps.Extrapolation | ClipCaps.SpeedMultiplier | ClipCaps.Looping;
                }
                else
                {
                    return ClipCaps.Blending | ClipCaps.ClipIn | ClipCaps.Extrapolation | ClipCaps.SpeedMultiplier;
                }
            }
        }

        public override double duration
        {
            get
            {
                if (template.animationReference != null)
                {
                    return template.animationReference.Animation.Duration;
                }
                else
                {
                    return 0;
                }
            }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<SpineTimelineClipBehaviour>.Create(graph, template);
            playable.GetBehaviour();
            return playable;
        }
    }
}