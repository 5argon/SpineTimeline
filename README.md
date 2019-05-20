# Spine Timeline

## Issues

Issue related to the component's `Update` / `LateUptate` stealing timeline's control : 
 
- `SkeletonAnimationTrack` : When the timeline is not controlling but the playable graph is still active, the `AnimationState` that may be playing is able to play through while the graph is stopping.
- `SkeletonGraphicTrack` : It has to control `freeze` to fix some bugs. In turn your `freeze` that you set beforehand will be lost.