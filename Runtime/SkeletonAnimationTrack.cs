using Spine.Unity;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.SpineTimeline
{
    [TrackColor(0.9960785f, 0.2509804f, 0.003921569f)]
    [TrackClipType(typeof(SpineTimelineClip))]
    [TrackBindingType(typeof(SkeletonAnimation))]
    public class SkeletonAnimationTrack : TrackAsset
    {
        public int trackIndex = 0;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var scriptPlayable = ScriptPlayable<SpineTimelineMixerBehaviour>.Create(graph, inputCount);
            var mixerBehaviour = scriptPlayable.GetBehaviour();
            mixerBehaviour.trackIndex = this.trackIndex;
            return scriptPlayable;
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            SkeletonAnimation skeletonAnimation = (SkeletonAnimation)director.GetGenericBinding(this);
            if (skeletonAnimation != null)
            {
                driver.AddFromComponent(skeletonAnimation.gameObject, skeletonAnimation);
            }
        }
    }
}