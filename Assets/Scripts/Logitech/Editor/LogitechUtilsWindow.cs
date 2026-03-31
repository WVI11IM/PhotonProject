using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Logitech.Editor {

    public class LogitechUtilsWindow : EditorWindow {

        private const string ConfigPath = "Assets/Scripts/Logitech/Resources/LogitechUtilsConfig.asset";

        private bool _showConfig;
        private bool _showValues;

        [MenuItem("Window/Logitech Util")]
        public static void ShowMyEditor() {
            
            // This method is called when the user selects the menu item in the Editor.
            EditorWindow wnd = GetWindow<LogitechUtilsWindow>();
            wnd.titleContent = new GUIContent("Logitech Utils");

            // Limit size of the window.
            wnd.minSize = new Vector2(450, 200);
            // wnd.maxSize = new Vector2(1920, 720);
            
        }

        public void OnGUI() {

            // If no config is present in the static 
            if (LogitechUtil.Config == null) {
                if (Resources.Load(ConfigPath) is LogitechUtilConfig) {
                    LogitechUtil.Config = (LogitechUtilConfig)Resources.Load(ConfigPath);
                } else {
                    CreateConfigFile();
                }
            }
            
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;
            
            // Config foldout group
            _showConfig = EditorGUILayout.Foldout(_showConfig, "Config");

            if (_showConfig) {
                LogitechUtil.AttemptWheelInitAtRuntime = EditorGUILayout.Toggle(
                    new GUIContent("Re-init wheel at runtime",
                        "Whether to constantly re-attempt wheel initialization at runtime if initialization failed on start."),
                    LogitechUtil.AttemptWheelInitAtRuntime);

                LogitechUtil.UseKeyboardEmulation = EditorGUILayout.Toggle(
                    new GUIContent("Emulate wheel with keyboard",
                        "Whether to use the below InputActions to control the game instead of a Logitech steering device."),
                    LogitechUtil.UseKeyboardEmulation);

                EditorGUI.BeginDisabledGroup(!LogitechUtil.UseKeyboardEmulation);

                LogitechUtil.KeyboardActions = (InputActionAsset)EditorGUILayout.ObjectField(
                    new GUIContent("Wheel emulation actions",
                        "InputActions asset containing the standard input actions used to emulate a Logitech steering device."),
                    LogitechUtil.KeyboardActions, typeof(InputActionAsset), false);

                EditorGUI.EndDisabledGroup();
            }

            // Visualize the current Logitech Wheel input values using sliders.
            _showValues = EditorGUILayout.Foldout(_showValues, "Values");

            if (_showValues) {
                EditorGUILayout.LabelField("Steering Wheel Axis:");
                EditorGUILayout.Slider(LogitechUtil.WheelAxis, -1, 1);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Clutch");
                EditorGUILayout.Slider(LogitechUtil.AxisPedalClutch, 0, 1);
                EditorGUILayout.LabelField("Brake");
                EditorGUILayout.Slider(LogitechUtil.AxisPedalBrake, 0, 1);
                EditorGUILayout.LabelField("Accelerator");
                EditorGUILayout.Slider(LogitechUtil.AxisPedalAccelerator, 0, 1);
            }

        }

        public static void CreateConfigFile() {
            // MyClass inherits from ScriptableObject base class
            var newConfig = CreateInstance<LogitechUtilConfig>();

            // Path has to start at "Assets"
            AssetDatabase.CreateAsset(newConfig, ConfigPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            LogitechUtil.Config = newConfig;

            // Unneeded, but will keep here for future reference as it's likely to be useful elsewhere in the future
            // Selection.activeObject = newConfig;
        }

    }

}