using Spine.Unity;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.Timeline
{
    [TrackBindingType(typeof(SkeletonAnimation))]
    public class SkeletonAnimationTrack : SpineTrack
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var sp = base.CreateTrackMixer(graph, go, inputCount);
            return sp;
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            SkeletonAnimation skeletonAnimation = (SkeletonAnimation)director.GetGenericBinding(this);
            if (skeletonAnimation != null)
            {
                driver.AddFromName<SkeletonAnimation>(skeletonAnimation.gameObject, "m_Enabled");
            }
        }
    }
}