using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using WeArt.Components;

namespace WeArt.UnityEditor
{
    /// <summary>
    /// A custom inspector for components of type <see cref="WeArtSensorsCalibration"/>.
    /// </summary>
    [CustomEditor(typeof(WeArtSensorsCalibration), true), CanEditMultipleObjects]
    public class WeArtSensorsCalibrationEditor : WeArtComponentEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var editor = base.CreateInspectorGUI();

            // OnSensorCalibrationEnter
            {
                var property = serializedObject.FindProperty(nameof(WeArtSensorsCalibration._OnSensorsCalibrationEnter));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "This event is raised when sensors calibration state is entered"
                });
            }

            // OnSensorCalibrationExit
            {
                var property = serializedObject.FindProperty(nameof(WeArtSensorsCalibration._OnSensorsCalibrationExit));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "This event is raised when sensors calibration state is exited"
                });
            }


            // OnSensorsCalibrationStartSuccess
            {
                var property = serializedObject.FindProperty(nameof(WeArtSensorsCalibration._OnSensorsCalibrationStartSuccess));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "This event is raised when sensors calibration successfully starts"
                });
            }

            // OnSensorsCalibrationStartFail
            {
                var property = serializedObject.FindProperty(nameof(WeArtSensorsCalibration._OnSensorsCalibrationStartFail));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "This event is raised when sensors calibration fails to start"
                });
            }

            // OnSensorsCalibrationResultSuccess
            {
                var property = serializedObject.FindProperty(nameof(WeArtSensorsCalibration._OnSensorsCalibrationResultSuccess));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "This event is raised when sensors calibration successfully finishes"
                });
            }

            // OnSensorsCalibrationStartFail
            {
                var property = serializedObject.FindProperty(nameof(WeArtSensorsCalibration._OnSensorsCalibrationResultFail));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "This event is raised when sensors calibration fails to finish"
                });
            }

            return editor;
        }
    }
}
