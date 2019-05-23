using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Animation = Spine.Animation;
using AnimationState = Spine.AnimationState;

namespace E7.SpineTimeline
{
    public class SpineTimelineMixerBehaviour : PlayableBehaviour
    {
        public int trackIndex;
        public IEnumerable<IMarker> markers; //TODO : Use this to preview flip marker in edit mode + restore back.

        private AnimationState animationState;
        private SkeletonAnimation affectedSkeletonAnimation;
        private SkeletonGraphic affectedSkeletonGraphic;
        private bool isSkeletonGraphic;
        private bool originalFreezeState = false;

        public override void OnPlayableDestroy(Playable playable)
        {
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
            if(Application.isPlaying == false && affectedSkeletonAnimation != null)
			{
				affectedSkeletonAnimation.Initialize(overwrite: true);
			}
        }


        /// <summary>
        /// Prepare a fragment of AnimationState that blends to produce the current timeline state.
        /// </summary>
        private AnimationState InitializeAnimationState(Playable playable, object playerData)
        {
            bool firstTime = affectedSkeletonAnimation == null && affectedSkeletonGraphic == null;

            if (firstTime)
            {
                var skeletonAnimation = playerData as SkeletonAnimation;
                var skeletonGraphic = playerData as SkeletonGraphic;
                var hasSkeletonDataAsset = playerData as IHasSkeletonDataAsset;

                //Make this once and keep clearing it instead.
                //This implies we will not ever change skeleton data mid-timeline. (unlikely?)
                animationState = new AnimationState(hasSkeletonDataAsset.SkeletonDataAsset.GetAnimationStateData());
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

            animationState.SetEmptyAnimation(trackIndex, mixDuration: 0);

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
                    TrackEntry entry = animationState.SetAnimation(trackIndex, toAdd, clipData.loop);
                    entry.TrackTime = (float)playheadFromClipBegin;
                    entry.AllowImmediateQueue();
                    foundFirst = true;
                }
                else if (!foundSecond)
                {
                    //WIP? Apparently this produces a weird mix, I am not sure it is correct or not.
                    TrackEntry entry = animationState.SetAnimation(trackIndex, toAdd, clipData.loop);
                    entry.MixTime = (float)playheadFromClipBegin;
                    entry.MixDuration = 1f;
                    foundSecond = true;
                    break;
                }
            }

            return animationState;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if(playerData == null) return;
            //Debug.Log($"Track time {playable.GetTime()}");
            AnimationState animationState = InitializeAnimationState(playable, playerData);

            var skeletonAnimation = playerData as SkeletonAnimation;
            var skeletonGraphic = playerData as SkeletonGraphic;
            var skeletonComponent = playerData as ISkeletonComponent;

            float playTime = (float)playable.GetTime();
            var skel = skeletonComponent.Skeleton;

            //Unfreeze for update, then freeze back immediately.
            if (isSkeletonGraphic) { skeletonGraphic.freeze = false; }

            skel.SetToSetupPose();
            skel.Time = playTime;
            animationState.Update(0);
            animationState.Apply(skeletonComponent.Skeleton);
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