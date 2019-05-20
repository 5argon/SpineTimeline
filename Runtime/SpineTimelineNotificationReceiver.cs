using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Playables;

namespace E7.SpineTimeline
{
    public class SpineTimelineNotificationReceiver : MonoBehaviour, INotificationReceiver
    {
        private SkeletonAnimation skeletonAnimation;
        private SkeletonGraphic skeletonGraphic;

        private SkeletonAnimation SkeletonAnimation
        {
            get
            {
                if (skeletonAnimation == null)
                {
                    skeletonAnimation = GetComponent<SkeletonAnimation>();
                }
                return skeletonAnimation;
            }
        }

        private SkeletonGraphic SkeletonGraphic
        {
            get
            {
                if (skeletonGraphic == null)
                {
                    skeletonGraphic = GetComponent<SkeletonGraphic>();
                }
                return skeletonGraphic;
            }
        }

        private IEnumerable<Spine.AnimationState> AnimationStates
        {
            get
            {
                if (SkeletonAnimation != null)
                {
                    yield return SkeletonAnimation.AnimationState;
                }
                if (SkeletonGraphic != null)
                {
                    yield return SkeletonGraphic.AnimationState;
                }
            }
        }

        private IEnumerable<Spine.Skeleton> Skeletons
        {
            get
            {
                if (SkeletonAnimation != null)
                {
                    yield return SkeletonAnimation.Skeleton;
                }
                if (SkeletonGraphic != null)
                {
                    yield return SkeletonGraphic.Skeleton;
                }
            }
        }

        public void Awake()
        {
            var sg = SkeletonGraphic;
            var sa = SkeletonAnimation;
        }

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is SpineTimelineMarker stm)
            {
                Debug.Log($"noti");
                switch (stm.action)
                {
                    case SpineTimelineAction.SetAnimation:
                        foreach(var x in AnimationStates) x.SetAnimation(stm.trackIndex, stm.animationReference, stm.loop);
                        break;
                    case SpineTimelineAction.SetEmptyAnimation:
                        foreach (var x in AnimationStates) x.SetEmptyAnimation(stm.trackIndex, 0);
                        break;
                    case SpineTimelineAction.FlipX:
                        foreach (var x in Skeletons) x.ScaleX = -x.ScaleX;
                        break;
                    case SpineTimelineAction.FlipY:
                        foreach (var x in Skeletons) x.ScaleY = -x.ScaleY;
                        break;
                }
                if (stm.changeTimeScale)
                {
                    if (SkeletonAnimation != null)
                    {
                        SkeletonAnimation.timeScale = stm.timeScale;
                    }
                    if (SkeletonGraphic != null)
                    {
                        SkeletonGraphic.timeScale = stm.timeScale;
                    }
                }
            }
        }
    }
}