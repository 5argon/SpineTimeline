using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace E7.Timeline
{
    [CustomEditor(typeof(SpineTimelineMarker))]
    public class SpineTimelineMarkerDrawer : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            // public SpineTimelineAction action;
            // [Space]
            // public int trackIndex;
            // public AnimationReferenceAsset animationReference;
            // public bool loop;
            // [Space]
            // public bool changeTimeScale;
            // public float timeScale;
            var ve = new VisualElement();

            var action = serializedObject.FindProperty(nameof(SpineTimelineMarker.action));
            var trackIndex = serializedObject.FindProperty(nameof(SpineTimelineMarker.trackIndex));
            var animationReference = serializedObject.FindProperty(nameof(SpineTimelineMarker.animationReference));
            var loop = serializedObject.FindProperty(nameof(SpineTimelineMarker.loop));
            var changeTimeScale = serializedObject.FindProperty(nameof(SpineTimelineMarker.changeTimeScale));
            var timeScale = serializedObject.FindProperty(nameof(SpineTimelineMarker.timeScale));

            var actionField = new PropertyField(action);
            ve.Add(actionField);
            var sta = (SpineTimelineAction)action.enumValueIndex;

            if (sta == SpineTimelineAction.SetAnimation || sta == SpineTimelineAction.SetEmptyAnimation)
            {
                ve.Add(new PropertyField(trackIndex));
            }

            if (sta == SpineTimelineAction.SetAnimation)
            {
                ve.Add(new PropertyField(animationReference));
                ve.Add(new PropertyField(loop));
            }

            ve.Add(new PropertyField(changeTimeScale));
            ve.Add(new PropertyField(timeScale));

            serializedObject.ApplyModifiedProperties();
            return ve;
        }
    }
}