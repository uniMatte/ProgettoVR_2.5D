using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using WeArt.Components;

namespace WeArt.UnityEditor
{
    /// <summary>
    /// A custom inspector for components of type <see cref="WeArtController"/>.
    /// </summary>
    [CustomEditor(typeof(WeArtController), true), CanEditMultipleObjects]
    public class WeArtControllerEditor : WeArtComponentEditor
    {
        private WeArtController Controller => serializedObject.targetObject as WeArtController;

        public override VisualElement CreateInspectorGUI()
        {
            var editor = base.CreateInspectorGUI();

            // Label
            {
                var header = new Label("Common Settings");
                header.AddToClassList("header");
                editor.Add(header);
            }

            // Type of tracking data to receive from Middleware 
            {
                var property = serializedObject.FindProperty(nameof(WeArtController._deviceGeneration));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "Device generation used in the project"
                });
            }

            // Allows to work with gestures: at this moment only teleportation is available.
            {
                var property = serializedObject.FindProperty(nameof(WeArtController._allowGestures));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "If true, the features that work with gesture recognition will be enabled: Teleportation"
                });
            }

            // Allows to grasp objects with external grasping systems.
            {
                var property = serializedObject.FindProperty(nameof(WeArtController._useExternalGraspSystem));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "If true, the Weart grasp system is disabled, allowing the use of external systems"
                });
            }

            // Start calibration automatically after run Middleware
            {
                var property = serializedObject.FindProperty(nameof(WeArtController._startCalibrationAutomatically));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "If true, the middleware will start the finger tracking calibration process after be started"
                });
            }

            // Start raw data automatically after run Middleware
            {
                var property = serializedObject.FindProperty(nameof(WeArtController._startRawDataAutomatically));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "If true, the middleware will start sending raw data to the client after starting"
                });
            }

            // PC BASED ONLY COMPONENTS
            {
                var mainHeader = new Label("PC BASED ONLY COMPONENTS");
                mainHeader.AddToClassList("main-header");
                mainHeader.style.color = Color.black; 
                mainHeader.style.fontSize = 12; 
                mainHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
                mainHeader.style.unityTextAlign = TextAnchor.MiddleCenter;

                mainHeader.style.marginTop = 15;

                if (ColorUtility.TryParseHtmlString("#CBFF00", out Color headerColor))
                {
                    mainHeader.style.backgroundColor = new StyleColor(headerColor);
                }
                else
                {
                    Debug.LogError("Failed to parse color.");
                }

                editor.Add(mainHeader);

                // Label
                {
                    var header = new Label("Settings");
                    header.AddToClassList("header");
                    editor.Add(header);
                }

                // Client port
                {
                    var property = serializedObject.FindProperty(nameof(WeArtController._clientPort));
                    var field = new PropertyField(property)
                    {
                        tooltip = "The local network port used by the middleware",
                    };

                    // Make the property read-only
                    field.SetEnabled(false);

                    editor.Add(field);
                }



                // Start automatically
                {
                    var property = serializedObject.FindProperty(nameof(WeArtController._startAutomatically));
                    editor.Add(new PropertyField(property)
                    {
                        tooltip = "If true, the middleware will be started automatically on Start()"
                    });
                }
            }


            // Standalone Only Components
            {
                var mainHeader = new Label("STANDALONE ONLY COMPONENTS");
                mainHeader.AddToClassList("main-header");
                mainHeader.style.color = Color.black; 
                mainHeader.style.fontSize = 12; 
                mainHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
                mainHeader.style.unityTextAlign = TextAnchor.MiddleCenter;

                mainHeader.style.marginTop = 15;

                if (ColorUtility.TryParseHtmlString("#CBFF00", out Color headerColor))
                {
                    mainHeader.style.backgroundColor = new StyleColor(headerColor);
                }
                else
                {
                    Debug.LogError("Failed to parse color.");
                }

                editor.Add(mainHeader);

                // Create the subheader label
                var subHeader = new Label("TouchDIVERs");
                subHeader.AddToClassList("header"); // You can customize the class for styling
                editor.Add(subHeader);

                // TouchDIVER Right
                {
                    var property = serializedObject.FindProperty(nameof(WeArtController._deviceFirst));
                    editor.Add(new PropertyField(property)
                    {
                        tooltip = "TouchDIVER device First hand reference"
                    });
                }

                // TouchDIVER Left
                {
                    var property = serializedObject.FindProperty(nameof(WeArtController._deviceSecond));
                    editor.Add(new PropertyField(property)
                    {
                        tooltip = "TouchDIVER device Second hand reference"
                    });
                }
                // Finger tracking performance
                {
                    var property = serializedObject.FindProperty(nameof(WeArtController._trackingPerformance));
                    editor.Add(new PropertyField(property)
                    {
                        tooltip = "Finger tracking performance accordingly with headset BLE capabilities"
                    });
                }
            }

            // Debug Only
            {
                var mainHeader = new Label("DEBUG ONLY");
                mainHeader.AddToClassList("main-header");
                mainHeader.style.color = Color.black;
                mainHeader.style.fontSize = 12;
                mainHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
                mainHeader.style.unityTextAlign = TextAnchor.MiddleCenter;

                mainHeader.style.marginTop = 15;

                if (ColorUtility.TryParseHtmlString("#CBFF00", out Color headerColor))
                {
                    mainHeader.style.backgroundColor = new StyleColor(headerColor);
                }
                else
                {
                    Debug.LogError("Failed to parse color.");
                }

                editor.Add(mainHeader);

                // Create the subheader label
                var subHeader = new Label("Log Messages");
                subHeader.AddToClassList("header"); // You can customize the class for styling
                editor.Add(subHeader);

                // Debug messages
                {
                    var property = serializedObject.FindProperty(nameof(WeArtController._debugMessagesOnConsole));
                    editor.Add(new PropertyField(property)
                    {
                        tooltip = "If true, any messages send or received from/to the middleware will be logged in the console"
                    });
                }

                {
                    var property = serializedObject.FindProperty(nameof(WeArtController._debugMessagesOnFile));
                    editor.Add(new PropertyField(property)
                    {
                        tooltip = "If true, any messages send or received from/to the middleware will be logged in a file (standalone only)"
                    });
                }

                // Level of logs to be written
                {
                    var property = serializedObject.FindProperty(nameof(WeArtController._projectLogLevel));
                    editor.Add(new PropertyField(property)
                    {
                        tooltip = "Level of logs that will be written into console and log file"
                    });
                }
            }


            Controller.Client.OnConnectionStatusChanged += onConnectionStatusChanged;
            void onConnectionStatusChanged(bool connected)
            {
            }

            return editor;
        }

        private void SetMiddleware(bool on)
        {
            if (on)
            {
                Controller.Client.OnConnectionStatusChanged += onClientConnectionChanged;
                Controller.StartFromEditor();
                EditorApplication.playModeStateChanged += onPlayModeStateChanged;
                AssemblyReloadEvents.beforeAssemblyReload += onScriptsReloading;
            }
            else
            {
                Controller.Client.OnConnectionStatusChanged += onClientConnectionChanged;
                Controller.Client.Stop();
                EditorApplication.playModeStateChanged -= onPlayModeStateChanged;
                AssemblyReloadEvents.beforeAssemblyReload -= onScriptsReloading;
            }

            void onClientConnectionChanged(bool _)
            {
                Controller.Client.OnConnectionStatusChanged -= onClientConnectionChanged;
            }

            void onPlayModeStateChanged(PlayModeStateChange change)
            {
                if (change == PlayModeStateChange.ExitingEditMode)
                    SetMiddleware(false);
            }

            void onScriptsReloading() => SetMiddleware(false);
        }

        private void SetCalibration(bool on)
        {

            if (on)
            {
                Controller.Client.StartCalibration();
                EditorApplication.playModeStateChanged += onPlayModeStateChanged;
                AssemblyReloadEvents.beforeAssemblyReload += onScriptsReloading;
            }
            else
            {
                Controller.Client.StopCalibration();
                EditorApplication.playModeStateChanged -= onPlayModeStateChanged;
                AssemblyReloadEvents.beforeAssemblyReload -= onScriptsReloading;
            }

           
            void onPlayModeStateChanged(PlayModeStateChange change)
            {
                if (change == PlayModeStateChange.ExitingEditMode)
                    SetCalibration(false);
            }

            void onScriptsReloading() => SetCalibration(false);
        }
    }

    public static class LocalExtension
    {
        public static void Display(this VisualElement element, bool show)
        {
            element.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}