using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using WeArt.TouchUI;

namespace WeArt.UnityEditor
{
    /// <summary>
    /// A custom inspector for components of type <see cref="SensorsCalibrationPanel"/>.
    /// </summary>
    [CustomEditor(typeof(SensorsCalibrationPanel), true), CanEditMultipleObjects]
    public class SensorsCalibrationPanelEditor : WeArtComponentEditor
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
                var property = serializedObject.FindProperty(nameof(SensorsCalibrationPanel.bleConnectionPanel));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Select here the game object containing the ble connection panel"
                };
                editor.Add(propertyField);
            }

            {
                var property = serializedObject.FindProperty(nameof(SensorsCalibrationPanel.startPanel));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Select here the game object containing the start panel"
                };
                editor.Add(propertyField);
            }

            {
                var property = serializedObject.FindProperty(nameof(SensorsCalibrationPanel.warningPanel));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Select here the game object containing the warning panel"
                };
                editor.Add(propertyField);
            }

            {
                var property = serializedObject.FindProperty(nameof(SensorsCalibrationPanel.calibrationPanel));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Select here the game object containing the calibration panel"
                };
                editor.Add(propertyField);
            }

            {
                var property = serializedObject.FindProperty(nameof(SensorsCalibrationPanel.completePanel));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Select here the game object containing the complete panel"
                };
                editor.Add(propertyField);
            }

            {
                var property = serializedObject.FindProperty(nameof(SensorsCalibrationPanel.startCalibButton));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Button stars sensors calibration process"
                };
                editor.Add(propertyField);
            }

            {
                var property = serializedObject.FindProperty(nameof(SensorsCalibrationPanel.cancelPanelButton));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Button cancel calibration process"
                };
                editor.Add(propertyField);
            }

            {
                var property = serializedObject.FindProperty(nameof(SensorsCalibrationPanel.closePanelButton));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Button closes calibration panel"
                };
                editor.Add(propertyField);
            }

            {
                var property = serializedObject.FindProperty(nameof(SensorsCalibrationPanel.calibrationBar));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Image that rapresents the progress bar"
                };
                editor.Add(propertyField);
            }

            {
                var property = serializedObject.FindProperty(nameof(SensorsCalibrationPanel.startCalibText));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Text that informs whether the devices are connected"
                };
                editor.Add(propertyField);
            }

            {
                var property = serializedObject.FindProperty(nameof(SensorsCalibrationPanel.finalCalibText));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Text that informs whether the process was successful or not"
                };
                editor.Add(propertyField);
            }

            {
                var property = serializedObject.FindProperty(nameof(SensorsCalibrationPanel.successIcon));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Image that rapresents the success of calibration process"
                };
                editor.Add(propertyField);
            }

            {
                var property = serializedObject.FindProperty(nameof(SensorsCalibrationPanel.failIcon));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Image that rapresents the fail of calibration process"
                };
                editor.Add(propertyField);
            }

            {
                var property = serializedObject.FindProperty(nameof(SensorsCalibrationPanel.resultIcon));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Image that rapresents the icon for calibration result"
                };
                editor.Add(propertyField);
            }

            {
                var property = serializedObject.FindProperty(nameof(SensorsCalibrationPanel.failColor));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Color that rapresents failure"
                };
                editor.Add(propertyField);
            }

            {
                var property = serializedObject.FindProperty(nameof(SensorsCalibrationPanel.successColor));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Image that rapresents success"
                };
                editor.Add(propertyField);
            }

            return editor;
        }
    }
}
