using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.SpineTimeline
{
    [TrackColor(0.9960785f, 0.2509804f, 0.003921569f)]
	[TrackClipType(typeof(SpineTimelineClip))]
    public abstract class SpineTrack : TrackAsset
    {
		public int trackIndex = 0;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            SpineTimelineMixerBehaviour template = new SpineTimelineMixerBehaviour();
            template.trackIndex = this.trackIndex;
            template.markers = GetMarkers();

            var scriptPlayable = ScriptPlayable<SpineTimelineMixerBehaviour>.Create(graph, template, inputCount);
            var mixerBehaviour = scriptPlayable.GetBehaviour();
            mixerBehaviour.trackIndex = this.trackIndex;
            return scriptPlayable;
        }
    }
}