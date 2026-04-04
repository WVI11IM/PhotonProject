using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Logitech {

    public class LogitechUtilWindow : EditorWindow {

        private bool _showConfig;
        private bool _showValues;
        private bool _showEmulatedValues;
        private LogitechUtilConfig Config => LogitechUtil.Config;

        [MenuItem("Window/Logitech Util")]
        public static void ShowMyEditor() {
            
            // This method is called when the user selects the menu item in the Editor.
            EditorWindow wnd = GetWindow<LogitechUtilWindow>();
            wnd.titleContent = new GUIContent("Logitech Utils");

            // Limit size of the window.
            wnd.minSize = new Vector2(450, 200);
            // wnd.maxSize = new Vector2(1920, 720);
            
        }

        private void Update() {
            Repaint();
        }

        public void OnGUI() {
            
            // Config foldout group
            _showConfig = EditorGUILayout.Foldout(_showConfig, "Config");

            if (_showConfig) {
                Config.attemptWheelInitAtRuntime = EditorGUILayout.Toggle(
                    new GUIContent("Re-init wheel at runtime",
                        "Whether to constantly re-attempt wheel initialization at runtime if initialization failed on start."),
                    Config.attemptWheelInitAtRuntime);

                Config.useKeyboardEmulation = EditorGUILayout.Toggle(
                    new GUIContent("Emulate wheel with keyboard",
                        "Whether to use the below InputActions to control the game instead of a Logitech steering device."),
                    Config.useKeyboardEmulation);

                EditorGUI.BeginDisabledGroup(!Config.useKeyboardEmulation);
                
                Config.keyboardActions = (InputActionAsset)EditorGUILayout.ObjectField(
                    new GUIContent("Wheel emulation actions",
                        "InputActions asset containing the standard input actions used to emulate a Logitech steering device."),
                    Config.keyboardActions, typeof(InputActionAsset), false);
                
                EditorGUI.EndDisabledGroup();
                
                if (GUILayout.Button("Select Config File"))
                    Selection.activeObject = Config;
                
                if (GUILayout.Button("Save")) {
                    EditorUtility.SetDirty(Config);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                
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

            // Visualize the current Logitech Wheel input values using sliders.
            _showEmulatedValues = EditorGUILayout.Foldout(_showEmulatedValues, "Emulated Values");

            if (_showEmulatedValues) {
                EditorGUILayout.LabelField("Steering Wheel Axis:");
                EditorGUILayout.Slider(LogitechUtil.EmulatedWheel, -1, 1);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Clutch");
                EditorGUILayout.Slider(LogitechUtil.EmulatedClutch, 0, 1);
                EditorGUILayout.LabelField("Brake");
                EditorGUILayout.Slider(LogitechUtil.EmulatedBrake, 0, 1);
                EditorGUILayout.LabelField("Accelerator");
                EditorGUILayout.Slider(LogitechUtil.EmulatedAccelerator, 0, 1);
            }
                
            if (GUILayout.Button("Select Singleton"))
                Selection.activeObject = LogitechUtil.Instance;

        }

    }

}