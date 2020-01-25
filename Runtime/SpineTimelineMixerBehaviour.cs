using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Animation = Spine.Animation;
using AnimationState = Spine.AnimationState;

namespace E7.Timeline
{
    /// <summary>
    /// Common mixer for all kinds of Spine-based track.
    /// </summary>
    [System.Serializable]
    public class SpineTimelineMixerBehaviour : PlayableBehaviour
    {
        // public enum PostPlaybackMode
        // {
        //     ApplyFinalSkeleton,
        //     BackToPriorSkeleton
        // }

#pragma warning disable 0649
        [SerializeField] private int trackIndex;
        //[SerializeField] private PostPlaybackMode postPlaybackMode;
#pragma warning restore 0649

        internal IEnumerable<IMarker> markers { set; private get; } //TODO : Use this to preview flip marker in edit mode + restore back.

        /// <summary>
        /// This animation state is separated from the one on your object. As long as the timeline is playing
        /// the one on your object will be made ineffective while applying this one continuously.
        /// </summary>
        private AnimationState timelineAnimationState;

        private SkeletonAnimation affectedSkeletonAnimation;
        private SkeletonGraphic affectedSkeletonGraphic;
        private bool isSkeletonGraphic;
        private bool originalFreezeState = false;

        public override void OnPlayableDestroy(Playable playable)
        {
            //Unfreezing make skeleton stored in the object take effect again.
            if (isSkeletonGraphic)
            {
                if (affectedSkeletonGraphic != null)
                {
                    affectedSkeletonGraphic.freeze = originalFreezeState;
                }
            }
            else
            {
                if (affectedSkeletonAnimation != null)
                {
                    affectedSkeletonAnimation.enabled = originalFreezeState;
                }
            }

            // Reverse SkeletonAnimation's `skeleton` non-serialized property which can't be registered by the driver.
            if (Application.isPlaying == false && affectedSkeletonAnimation != null)
            {
                affectedSkeletonAnimation.Initialize(overwrite: true);
            }

            // if (Application.isPlaying && postPlaybackMode == PostPlaybackMode.ApplyFinalSkeleton)
            // {
            //     //In the case that the object is not in playing state, it maybe desirable to copy timeline skeleton to be the real one
            //     //after the timeline ends.
            //     if (isSkeletonGraphic)
            //     {
            //         if (affectedSkeletonGraphic != null && timelineAnimationState != null)
            //         {
            //             affectedSkeletonGraphic.set
            //         }
            //     }
            //     else
            //     {
            //         if (affectedSkeletonAnimation != null && timelineAnimationState != null)
            //         {
            //             affectedSkeletonAnimation.enabled = originalFreezeState;
            //             timelineAnimationState.Apply(affectedSkeletonGraphic.Skeleton);
            //         }
            //     }
            // }
        }


        /// <summary>
        /// Prepare a fragment of AnimationState that blends to produce the current timeline state.
        /// </summary>
        private void UpdateTimelineAnimationState(Playable playable, object playerData)
        {
            bool firstTime = affectedSkeletonAnimation == null && affectedSkeletonGraphic == null;

            if (firstTime)
            {
                var skeletonAnimation = playerData as SkeletonAnimation;
                var skeletonGraphic = playerData as SkeletonGraphic;
                var hasSkeletonDataAsset = playerData as IHasSkeletonDataAsset;

                //Make this once and keep clearing it instead.
                //This implies we will not ever change skeleton data mid-timeline. (unlikely?)
                timelineAnimationState = new AnimationState(hasSkeletonDataAsset.SkeletonDataAsset.GetAnimationStateData());
                isSkeletonGraphic = skeletonGraphic != null;
                if (!isSkeletonGraphic)
                {
                    affectedSkeletonAnimation = skeletonAnimation;
                    //Prevent regular update from interfering.
                    originalFreezeState = skeletonAnimation.enabled;
                    skeletonAnimation.enabled = false;
                }
                else
                {
                    affectedSkeletonGraphic = skeletonGraphic;
                    //Prevent regular update from interfering.
                    originalFreezeState = skeletonGraphic.freeze;
                    skeletonGraphic.freeze = true;
                }
            }

            timelineAnimationState.SetEmptyAnimation(trackIndex, mixDuration: 0);

            // Find at most 2 blended clips that is affecting the playhead.
            // Spine could mix 3 clips but timeline's interface allows only 2 naturally.

            int inputCount = playable.GetInputCount();
            bool foundFirst = false;
            bool foundSecond = false;

            for (int i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                if (inputWeight == 0)
                {
                    continue;
                }

                ScriptPlayable<SpineTimelineClipBehaviour> clip = (ScriptPlayable<SpineTimelineClipBehaviour>)playable.GetInput(i);
                SpineTimelineClipBehaviour clipData = clip.GetBehaviour();
                var playheadFromClipBegin = clip.GetTime();
                var clipDuration = clip.GetDuration();
                if (clipData.animationReference == null)
                {
                    continue;
                }

                Animation toAdd = clipData.animationReference.Animation;

                //Debug.Log($"{i} {playheadFromClipBegin} {inputWeight} {toAdd.Name} {toAdd.Duration}");

                if (!foundFirst)
                {
                    TrackEntry entry = timelineAnimationState.SetAnimation(trackIndex, toAdd, clipData.loop);
                    entry.TrackTime = (float)playheadFromClipBegin;
                    entry.AllowImmediateQueue();
                    foundFirst = true;
                }
                else if (!foundSecond)
                {
                    //WIP? Apparently this produces a weird mix, I am not sure it is correct or not.
                    TrackEntry entry = timelineAnimationState.SetAnimation(trackIndex, toAdd, clipData.loop);
                    entry.MixTime = (float)playheadFromClipBegin;
                    entry.MixDuration = 1f;
                    foundSecond = true;
                    break;
                }
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if(playerData == null) return;
            //Debug.Log($"Track time {playable.GetTime()}");
            UpdateTimelineAnimationState(playable, playerData);

            var skeletonAnimation = playerData as SkeletonAnimation;
            var skeletonGraphic = playerData as SkeletonGraphic;
            var skeletonComponent = playerData as ISkeletonComponent;

            float playTime = (float)playable.GetTime();
            var skel = skeletonComponent.Skeleton;

            //Unfreeze for update, then freeze back immediately.
            if (isSkeletonGraphic) { skeletonGraphic.freeze = false; }

            skel.SetToSetupPose();
            skel.Time = playTime;
            timelineAnimationState.Update(0);
            timelineAnimationState.Apply(skeletonComponent.Skeleton);
            skel.UpdateWorldTransform();

            if (isSkeletonGraphic)
            {
                skeletonGraphic.LateUpdate();
                if (isSkeletonGraphic) { skeletonGraphic.freeze = true; }
            }
            else
            {
                skeletonAnimation.LateUpdate();
            }
        }
	}
}