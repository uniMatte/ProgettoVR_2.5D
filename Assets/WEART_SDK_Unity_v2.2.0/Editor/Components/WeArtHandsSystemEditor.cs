using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using WeArt.Components;

namespace WeArt.UnityEditor
{
    /// <summary>
    /// A custom inspector for components of type <see cref="WeArtHandsSystem"/>.
    /// </summary>
    [CustomEditor(typeof(WeArtHandsSystem), true), CanEditMultipleObjects]
    public class WeArtHandsSystemEditor : WeArtComponentEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var editor = base.CreateInspectorGUI();

            // HandsRenderType
            {
                var property = serializedObject.FindProperty(nameof(WeArtHandsSystem._handsRenderType));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "Handles hands rendering type. Always Visible: for always show the hands. Invisible During Grasp: for hide the hands only during grasp. Always Invisible: for always hide the hands"
                });
            }

            // HandsPhysicsType
            {
                var property = serializedObject.FindProperty(nameof(WeArtHandsSystem._handsPhysicsType));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "Handles hands physics simulation type. High: for better quality. Low: for better performance"
                });
            }

            return editor;
        }
    }
}
