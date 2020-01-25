using UnityEditor;

namespace E7.Timeline
{
    public abstract class DrawThingsInTemplate : Editor
    {
        public override void OnInspectorGUI()
        {
            var template = serializedObject.FindProperty("template");
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            var hasNext = template.NextVisible(enterChildren: true);
            while (hasNext)
            {
                EditorGUILayout.PropertyField(template);
                hasNext = template.NextVisible(enterChildren: false);
            }
            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }
    }
}