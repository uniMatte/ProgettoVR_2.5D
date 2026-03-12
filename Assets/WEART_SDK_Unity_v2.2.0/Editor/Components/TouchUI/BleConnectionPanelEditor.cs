using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using WeArt.TouchUI;

namespace WeArt.UnityEditor
{
    /// <summary>
    /// A custom inspector for components of type <see cref="BleConnectionPanel"/>.
    /// </summary>
    [CustomEditor(typeof(BleConnectionPanel), true), CanEditMultipleObjects]
    public class BleConnectionPanelEditor : WeArtComponentEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var editor = base.CreateInspectorGUI();

            // External tracking objects
            {
                var property = serializedObject.FindProperty(nameof(BleConnectionPanel.tracker));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Select here the game object containing the weart-app status tracker"
                };
                editor.Add(propertyField);
            }
            {
                var property = serializedObject.FindProperty(nameof(BleConnectionPanel.calibrationManager));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Select here the game object containing the calibration manager"
                };
                editor.Add(propertyField);
            }
            {
                var property = serializedObject.FindProperty(nameof(BleConnectionPanel.sensorsCalibrationPanel));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Select here the game object containing the sensors calibration panel"
                };
                editor.Add(propertyField);
            }

            // Canvas elements 
            {
                var property = serializedObject.FindProperty(nameof(BleConnectionPanel.touchDiverInfoPrefab));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "Prefab for Touch Diver Info panels."
                });
            }

            {
                var property = serializedObject.FindProperty(nameof(BleConnectionPanel.deviceContainer));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "Container where will be instantiated the info panels for each device."
                });
            }

            {
                var property = serializedObject.FindProperty(nameof(BleConnectionPanel.scrollUpButton));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "Button scrolls the list up."
                });
            }

            {
                var property = serializedObject.FindProperty(nameof(BleConnectionPanel.scrollDownButton));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "Button scrolls the list down."
                });
            }

            {
                var property = serializedObject.FindProperty(nameof(BleConnectionPanel.stopRunButton));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "Button stops devices and changing status to IDLE"
                });
            }

            {
                var property = serializedObject.FindProperty(nameof(BleConnectionPanel.sensorsCalibButton));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "Button opens sensors calibration panel"
                });
            }

            {
                var property = serializedObject.FindProperty(nameof(BleConnectionPanel.scrollBar));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "Side scroll bar."
                });
            }

            {
                var property = serializedObject.FindProperty(nameof(BleConnectionPanel.slidingArea));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "Sliding area."
                });
            }

            {
                var property = serializedObject.FindProperty(nameof(BleConnectionPanel.autoConnectionText));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "Auto Connection Text."
                });
            }

            return editor;
        }
    }
    
}