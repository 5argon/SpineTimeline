using Spine.Unity;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.Timeline
{
    [TrackBindingType(typeof(SkeletonGraphic))]
	public class SkeletonGraphicTrack : SpineTrack {

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var sp = base.CreateTrackMixer(graph, go, inputCount);
            return sp;
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            SkeletonGraphic skeletonGraphic = (SkeletonGraphic)director.GetGenericBinding(this);
            if (skeletonGraphic != null)
            {
               driver.AddFromName<SkeletonGraphic>(skeletonGraphic.gameObject, "freeze");
            }
        }
    }
}