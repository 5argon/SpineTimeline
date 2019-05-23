using UnityEngine.Timeline;
using UnityEngine.Playables;
using Spine.Unity;
using UnityEngine;

namespace E7.SpineTimeline
{
    public class SpineTimelineMarker : Marker, INotification , INotificationOptionProvider
    {
        public SpineTimelineAction action;
        [Space]
        public int trackIndex;
        public AnimationReferenceAsset animationReference;
        public bool loop;
        [Space]
        public bool changeTimeScale;
        public float timeScale;

        public PropertyName id => (int)action;

        public NotificationFlags flags => NotificationFlags.Retroactive;
    }
}