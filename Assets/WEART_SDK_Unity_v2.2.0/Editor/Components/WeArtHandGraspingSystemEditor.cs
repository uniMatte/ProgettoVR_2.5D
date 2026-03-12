using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using WeArt.Components;

namespace WeArt.UnityEditor
{
    /// <summary>
    /// A custom inspector for components of type <see cref="WeArtHandGraspingSysyem"/>.
    /// </summary>
    [CustomEditor(typeof(WeArtHandGraspingSystem), true), CanEditMultipleObjects]
    public class WeArtHandGraspingSystemEditor : WeArtComponentEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var editor = base.CreateInspectorGUI();
            editor.Bind(serializedObject);

            // Grasper
            {
                var property = serializedObject.FindProperty(nameof(WeArtHandGraspingSystem._grasper));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Trasform position Grasper"
                };
                editor.Add(propertyField);
            }

            /*// Hand Visibility
            {
                var property = serializedObject.FindProperty(nameof(WeArtHandGraspingSystem._hideDuringGrasp));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Handles hand visibility during grasp"
                };
                editor.Add(propertyField);
            }*/

            // Hand Visibility
            {
                var property = serializedObject.FindProperty(nameof(WeArtHandGraspingSystem._notPhysicalDuringGrasp));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Handles if the hand is physical during grasp"
                };
                editor.Add(propertyField);
            }

            // Label
            {
                var header = new Label("Snap grasping mode");
                header.AddToClassList("header");
                editor.Add(header);
            }

            // Grasper
            {
                var property = serializedObject.FindProperty(nameof(WeArtHandGraspingSystem._snapGraspProximityTransform));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Trasform for snap grasp proximity checking"
                };
                editor.Add(propertyField);
            }

            // Label
            {
                var header = new Label("Proximity grasping mode");
                header.AddToClassList("header");
                editor.Add(header);
            }

            {
                var property = serializedObject.FindProperty(nameof(WeArtHandGraspingSystem._fingerClosureGraspThreshold));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Threshold value above which the finger can grasp"
                };
                editor.Add(propertyField);
            }

            {
                var property = serializedObject.FindProperty(nameof(WeArtHandGraspingSystem._fingerClosureReleaseThreshold));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Threshold value below which the finger can release"
                };
                editor.Add(propertyField);
            }

            // Label
            {
                var header = new Label("Fingers proximity colliders");
                header.AddToClassList("header");
                editor.Add(header);
            }

            //  Proximity coliders
            {
                var property = serializedObject.FindProperty(nameof(WeArtHandGraspingSystem._thumbProximityColider));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Select here the proximity collider for the thumb"
                };
                editor.Add(propertyField);
            }
            {
                var property = serializedObject.FindProperty(nameof(WeArtHandGraspingSystem._indexProximityColider));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Select here the proximity collider for the index"
                };
                editor.Add(propertyField);
            }
            {
                var property = serializedObject.FindProperty(nameof(WeArtHandGraspingSystem._middleProximityColider));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Select here the proximity collider for the middle"
                };
                editor.Add(propertyField);
            }
            {
                var property = serializedObject.FindProperty(nameof(WeArtHandGraspingSystem._annularProximityColider));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Select here the proximity collider for the annular"
                };
                editor.Add(propertyField);
            }
            {
                var property = serializedObject.FindProperty(nameof(WeArtHandGraspingSystem._pinkyProximityColider));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Select here the proximity collider for the pinky"
                };
                editor.Add(propertyField);
            }

            return editor;
        }
    }
}