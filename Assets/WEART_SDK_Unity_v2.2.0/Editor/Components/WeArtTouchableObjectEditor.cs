using System.ComponentModel;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using WeArt.Components;
using WeArt.Core;
using WeArt.Utils;

namespace WeArt.UnityEditor
{
    /// <summary>
    /// A custom inspector for components of type <see cref="WeArtTouchableObjectEditor"/>.
    /// </summary>
    [CustomEditor(typeof(WeArtTouchableObject), true), CanEditMultipleObjects]
    public class WeArtTouchableObjectEditor : WeArtComponentEditor
    {
        private WeArtTouchableObject Touchable => serializedObject.targetObject as WeArtTouchableObject;

        public override VisualElement CreateInspectorGUI()
        {
            var editor = base.CreateInspectorGUI();
            editor.Bind(serializedObject);

            // Label
            {
                var header = new Label("Haptic feeling");
                header.AddToClassList("header");
                editor.Add(header);
            }

            // Temperature
            {
                var property = serializedObject.FindProperty(nameof(WeArtTouchableObject._temperature));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "It will trigger a thermic effect on touching haptic objects"
                };
                propertyField.RegisterCallback<ChangeEvent<Temperature>>(evt =>
                {
                    Touchable._temperature = evt.previousValue;
                    Touchable.Temperature = evt.newValue;
                });
                editor.Add(propertyField);
            }

            // Stiffness
            {
                var property = serializedObject.FindProperty(nameof(WeArtTouchableObject._stiffness));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "It will trigger a pressure force effect on touching haptic objects"
                };
                propertyField.RegisterCallback<ChangeEvent<Force>>(evt =>
                {
                    Touchable._stiffness = evt.previousValue;
                    Touchable.Stiffness = evt.newValue;
                });
                editor.Add(propertyField);
            }

            // Texture
            {
                var property = serializedObject.FindProperty(nameof(WeArtTouchableObject._texture));

                var propertyField = new PropertyField(property)
                {
                    tooltip = "It will trigger a haptic texture effect on touching haptic objects"
                };

                propertyField.RegisterCallback<ChangeEvent<Texture>>(evt =>
                {
                    Touchable._texture = evt.previousValue;
                    Touchable.Texture = evt.newValue;
                });
                editor.Add(propertyField);
            }

            // Label
            {
                var header = new Label("Interaction");
                header.AddToClassList("header");
                editor.Add(header);
            }

            

            // Surface Exploration
            {
                var property = serializedObject.FindProperty(nameof(WeArtTouchableObject._surfaceExploration));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Flag the object for the surface exploration"
                };

                editor.Add(propertyField);
            }

            // Ignore Force computation
            {
                var property = serializedObject.FindProperty(nameof(WeArtTouchableObject._disableDynamicForce));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Ignores the force computation while touching this touchable object"
                };

                editor.Add(propertyField);
            }

            // Graspable
            {
                var property = serializedObject.FindProperty(nameof(WeArtTouchableObject._graspable));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Enable interaction with WEART Hands to manage grasping"
                };

                editor.Add(propertyField);

                var graspContainer = new VisualElement();
                graspContainer.style.display = DisplayStyle.None;
                editor.Add(graspContainer);
                BuildGraspSection(graspContainer);

                propertyField.RegisterValueChangeCallback(evt =>
                {
                    UpdateGraspSectionVisibility(graspContainer, property.boolValue);
                });
            }

            if (EditorApplication.isPlaying)
            {
                // Label
                {
                    var header = new Label("Runtime");
                    header.AddToClassList("header");
                    editor.Add(header);
                }

                // Affected haptic objectss
                {
                    var affectedHapticsContainer = new VisualElement();
                    affectedHapticsContainer.AddToClassList("propertyRows");
                    editor.Add(affectedHapticsContainer);

                    updateHaptics();
                    Touchable.OnAffectedHapticObjectsUpdate += updateHaptics;

                    void updateHaptics()
                    {
                        const string hapticsLabel = "Affected haptic objects";
                        affectedHapticsContainer.Clear();
                        if (Touchable.AffectedHapticObjects.Count > 0)
                        {
                            foreach (var haptic in Touchable.AffectedHapticObjects)
                            {
                                string label = affectedHapticsContainer.childCount == 0 ? hapticsLabel : " ";
                                var hapticField = createHapticField(label, haptic);
                                affectedHapticsContainer.Add(hapticField);
                            }
                        }
                        else
                        {
                            var objectField = createHapticField(hapticsLabel, null);
                            affectedHapticsContainer.Add(objectField);
                        }
                    }

                    VisualElement createHapticField(string label, WeArtHapticObject haptic)
                    {
                        var objectField = new ObjectField(label);
                        objectField.AddToClassList("propertyRow");
                        objectField.objectType = typeof(WeArtHapticObject);
                        objectField.Q(className: ObjectField.inputUssClassName).SetEnabled(false);
                        objectField.SetValueWithoutNotify(haptic);
                        return objectField;
                    }
                }
            }

            return editor;
        }

        private void UpdatePropertyVisibility(PropertyField property, bool condition)
        {
            property.style.display = condition ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void UpdateHelpBoxVisibility(HelpBox property, bool condition)
        {
            property.style.display = condition ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void UpdateGraspSectionVisibility(VisualElement property, bool condition)
        {
            property.style.display = condition ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void BuildGraspSection(VisualElement editor)
        {
            // Label
            {
                var header = new Label("Grasping settings");
                header.AddToClassList("header");
                editor.Add(header);
            }

            // Grasping type
            {
                var propertyGT = serializedObject.FindProperty(nameof(WeArtTouchableObject._graspingType));
                var propertyField = new PropertyField(propertyGT)
                {
                    tooltip = "Defines which system will be used for grasping this object"
                };

                editor.Add(propertyField);

                var errorBox = new HelpBox("Physical and Snap mode are available only with WeArt grasping system. Proximity mode is for external grasping system. Check which type of grasping system is selected in WeArtController", HelpBoxMessageType.Error);
                errorBox.style.display = DisplayStyle.None;
                editor.Add(errorBox);

                propertyField.RegisterValueChangeCallback(evt =>
                {
                    serializedObject.ApplyModifiedProperties();

                    if (WeArtController.Instance.UseExternalGraspSystem)
                    {
                        UpdateHelpBoxVisibility(errorBox, propertyGT.enumValueIndex != (int)GraspingType.Proximity);
                    }
                    else
                    {
                        UpdateHelpBoxVisibility(errorBox, propertyGT.enumValueIndex == (int)GraspingType.Proximity);
                    }
                });
            }

            // Label
            {
                var header = new Label("Grasping events");
                header.AddToClassList("header");
                editor.Add(header);
            }

            // OnGrasp Ready/Not Ready
            {
                var property = serializedObject.FindProperty(nameof(WeArtTouchableObject._graspingType));
                var propertyField = new PropertyField(property);

                var propertyReady = serializedObject.FindProperty(nameof(WeArtTouchableObject.OnGraspReady));
                var propertyReadyField = new PropertyField(propertyReady)
                {
                    tooltip = "Event invoked when the object is ready to be grasped"
                };
                propertyReadyField.style.display = DisplayStyle.None;
                editor.Add(propertyReadyField);

                var propertyNotReady = serializedObject.FindProperty(nameof(WeArtTouchableObject.OnGraspNotReady));
                var propertyNotReadyField = new PropertyField(propertyNotReady)
                {
                    tooltip = "Event invoked when the object is not ready to be grasped"
                };
                propertyNotReadyField.style.display = DisplayStyle.None;
                editor.Add(propertyNotReadyField);

                editor.Bind(serializedObject);

                UpdatePropertyVisibility(propertyReadyField, WeArtController.Instance.UseExternalGraspSystem && property.enumValueIndex == (int)GraspingType.Proximity);
                UpdatePropertyVisibility(propertyNotReadyField, WeArtController.Instance.UseExternalGraspSystem && property.enumValueIndex == (int)GraspingType.Proximity);

                editor.TrackPropertyValue(property, p =>
                {
                    UpdatePropertyVisibility(propertyReadyField, WeArtController.Instance.UseExternalGraspSystem && property.enumValueIndex == (int)GraspingType.Proximity);
                    UpdatePropertyVisibility(propertyNotReadyField, WeArtController.Instance.UseExternalGraspSystem && property.enumValueIndex == (int)GraspingType.Proximity);
                });
            }

            // OnGrasp
            {
                var property = serializedObject.FindProperty(nameof(WeArtTouchableObject.OnGrasp));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "Event invoked when the object is grasped"
                });
            }

            // OnRelease
            {
                var property = serializedObject.FindProperty(nameof(WeArtTouchableObject.OnRelease));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "Event invoked when the object is released"
                });
            }
        }
    }
}