using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.Timeline
{
    [TrackColor(0.9960785f, 0.2509804f, 0.003921569f)]
	[TrackClipType(typeof(SpineTimelineClip))]
    public abstract class SpineTrack : TrackAsset
    {
#pragma warning disable 0649
        [SerializeField] private SpineTimelineMixerBehaviour template;
#pragma warning restore 0649

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var scriptPlayable = ScriptPlayable<SpineTimelineMixerBehaviour>.Create(graph, template, inputCount);
            var mixerBehaviour = scriptPlayable.GetBehaviour();
            mixerBehaviour.markers = GetMarkers();
            return scriptPlayable;
        }
    }
}