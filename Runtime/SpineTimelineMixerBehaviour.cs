using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Playables;
using Animation = Spine.Animation;
using AnimationState = Spine.AnimationState;

namespace E7.SpineTimeline
{
    public class SpineTimelineMixerBehaviour : PlayableBehaviour {

        public int trackIndex;
        private SkeletonAnimation affectedSkeletonAnimation;
        private SkeletonGraphic affectedSkeletonGraphic;
        private bool isSkeletonGraphic;

        public override void OnPlayableDestroy(Playable playable)
        {
            if(isSkeletonGraphic)
            {
                affectedSkeletonGraphic.freeze = false;
            }
			//Reverse SkeletonAnimation's `skeleton` non-serialized property which can't be registered by the driver.
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
            var skeletonAnimation = playerData as SkeletonAnimation;
            var skeletonGraphic = playerData as SkeletonGraphic;
            var hasSkeletonDataAsset = playerData as IHasSkeletonDataAsset;
            isSkeletonGraphic = skeletonGraphic != null;
            if (!isSkeletonGraphic)
            {
                affectedSkeletonAnimation = skeletonAnimation;
            }
            else
            {
                affectedSkeletonGraphic = skeletonGraphic;
            }

            var skelData = hasSkeletonDataAsset.SkeletonDataAsset.GetSkeletonData(quiet: false);

            AnimationState animationState = new AnimationState(hasSkeletonDataAsset.SkeletonDataAsset.GetAnimationStateData());

            //TODO: Find at most 2 blended clips that is affecting the playhead.
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
                    TrackEntry entry = animationState.SetAnimation(0, toAdd, clipData.loop);
                    entry.TrackTime = (float)playheadFromClipBegin;
                    entry.AllowImmediateQueue();
                    foundFirst = true;
                }
                else if (!foundSecond)
                {
                    //WIP
                    TrackEntry entry = animationState.SetAnimation(0, toAdd, clipData.loop);
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
            //Debug.Log($"Track time {playable.GetTime()}");
            AnimationState animationState = InitializeAnimationState(playable, playerData);

            var skeletonAnimation = playerData as SkeletonAnimation;
            var skeletonGraphic = playerData as SkeletonGraphic;
            var skeletonComponent = playerData as ISkeletonComponent;

            float playTime = (float)playable.GetTime();
            var skel = skeletonComponent.Skeleton;

            if (isSkeletonGraphic)
            {
                skeletonGraphic.freeze = false;
            }

            skel.SetToSetupPose();
            skel.Time = playTime;
            animationState.Update(0);
            animationState.Apply(skeletonComponent.Skeleton);
            skel.UpdateWorldTransform();

            if (isSkeletonGraphic)
            {
                skeletonGraphic.LateUpdate();

                //Prevent normal LateUpdate of the MonoBehaviour from wrecking the animation
                //Since we didn't replace the animation state on the actual object.
                skeletonGraphic.freeze = true;
            }
            else
            {
                skeletonAnimation.LateUpdate();
            }
        }
	}
}