# Spine Timeline

- Minimum version is 2019.1 with .NET 4.6. Backward compatibility is not maintained since I want to go all out with `UIElements`, C# 6, Playables notification API, and timeline markers.
- Official Timeline integration from Spine [is coming](https://github.com/pharan/spine-unity-experimental/tree/spine-timeline/Assets/spine-unity-experimental/Spine%20Timeline/Documentation), but I need to animate my `SkeletonGraphic` now so I made this while I wait.

## `SkeletonAnimationTrack` + `SkeletonGraphicTrack`

Tracks that bind to respective type and both uses `SpineTimelineMixerBehaviour`.

## `SpineTimelineMixerBehaviour`

A per-track behaviour which create a new `AnimationState` every evaluation and apply it onto the skeleton. This make it possible to scrub the timeline and get any moment's Spine state at will.

- Track layering is not planned at the moment since my game won't use it.

## `SpineTimelineClip`

- The clip is just a data storage and has no behaviour logic, all logic are on the track's mixer behaviour.
- Length of the actual animation is visible on clicking. It is from `Duration` info from your `AnimationReferenceAsset`.
- Checking `Loop` will change HOLD to loop indicator, allowing you to gauge how many loops it is going to occur.
- It is possible to use `SpeedMultiplier`. (Hold shift and drag the edge)
- It is possible to use `ClipIn`. Drag the left edge in to start the animation from any point while not affecting the looping part. This behaviour we want is exactly `TrackEntry.TrackTime`.
- It is possible to use all `Extrapolation` mode. For example to fill the clip's gap with the last frame of previous clip with `Hold` mode.
- Cross fading is still not supported but would be via `MixTime` and `MixDuration`. Also my game is not using it..

![gif](.Documentation/speed-loop.gif)

## Track behaviour

- On an empty area it will respect `Extrapolation` mode. So if you want the animation to freeze after it ended, use `Hold` for example. Disadvantage of this approach is that the timeline is always evaluated even when there is no clip on the playhead. This allows deterministic scrubbing so I think it is a good trade.
- If no `Extrapolation` mode is on the empty area, the skeleton is repreatedly set to setup pose.
- While the track is playing, it will try to suppress normal `Update()` of respective component. `SkeletonGraphic` get its `freeze` turned on for the duration of the track. `SkeletonAnimation` timeline seems to override fine as long as you don't have any active `AnimationState` playing on it.

## `SpineTimelineMarker` + `SpineTimelineNotificationReceiver`

![marker](.Documentation/marker.png)

Uses the new Playable notification API in 2019.1 to trigger one-off action when playhead passed through a point. So it is not a sampled state. Only works at runtime.

`SetAnimation` and `SetEmptyAnimation` is applied immediately, but its effect will not start until timeline finished playing according to suppress behaviour mentioned earlier.

Markers with `INotification` affect track's total length, so if this marker is the final thing on the track it allows you to "send off" the skeleton by telling it to continue doing something after the track's playable had been destroyed. (`PlayableDirector` must be on `None` wrap mode and not `Hold` or `Loop`.)

`FlipX`, and `FlipY` applies immediately and will not restore after the timeline ended.

Change time scale do not affect animation played from timeline's clip, it only affect component-updated animation. (For example, after the timeline is over the `SetAnimation` will use the time scale.)

![marker](.Documentation/receiver.png)

The rule of timeline is that bound `GameObject`'s **all** components will receive an `INotification`, so attach `SpineTimelineNotificationReceiver` near either `SkeletonAnimation` or `SkeletonGraphic` for it to receive the marker and take action.

## Issues

See Issues section.