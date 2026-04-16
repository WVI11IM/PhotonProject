using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Logitech {

    #if UNITY_EDITOR
    using UnityEditor;
    public class LogitechEditorUtil : Editor {

        public static LogitechUtilConfig CreateConfigFile() {
            
            // Try to find it in the resources folder 
            if (Resources.Load(LogitechUtil.ConfigPath) is LogitechUtilConfig) {
                Debug.LogWarning($"Attempted to create LogitechUtilConfig when one already existed at {LogitechUtil.ConfigPath}. Returning existing instance instead...");
                return (LogitechUtilConfig)Resources.Load(LogitechUtil.ConfigName);
            }
            
            var newConfig = CreateInstance<LogitechUtilConfig>();
            
            // Path has to start at "Assets"
            AssetDatabase.CreateAsset(newConfig, LogitechUtil.ConfigPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            return newConfig;
            
        }

    }
    #endif

    public class LogitechUtil : MonoBehaviour {

        #region Constants and Readonlys
        
        public static readonly string ConfigName = "LogitechUtilConfig";
        public static readonly string ConfigPath = $"Assets/Scripts/Logitech/Resources/{ConfigName}.asset";
        public const string SingletonGameObjectName = "LogitechUtil Singleton";

        #endregion

        #region Singleton Logic
        private static LogitechUtil _instance;
        public static LogitechUtil Instance {
            get {
                if (!_instance) {
                    // If no instance is present, first attempt to find an existing instance (useful to avoid countless instances being created when reloading scripts)
                    if (GameObject.Find(SingletonGameObjectName) &&
                        GameObject.Find(SingletonGameObjectName).GetComponent<LogitechUtil>())
                        _instance = GameObject.Find(SingletonGameObjectName).GetComponent<LogitechUtil>();
                    // If an instance was not found, create a new one
                    if (!_instance)
                        _instance = new GameObject(SingletonGameObjectName, typeof(LogitechUtil)).GetComponent<LogitechUtil>();
                    // _instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
                }
                return _instance;
            }
        }
        #endregion
        
        #region InputSystem

        public static float EmulatedWheel;
        public static float EmulatedAccelerator;
        public static float EmulatedBrake;
        public static float EmulatedClutch;
        private static InputAction _isActionWheel;
        private static InputAction _isActionAccelerator;
        private static InputAction _isActionBrake;
        private static InputAction _isActionClutch;

        #endregion
        
        #region Properties // TODO: Override default values when wheel is missing, as without the wheel they default to 0.5

        public static bool Wheel => 
            Instance != null && 
            _sdkInitialized && 
            LogitechGSDK.LogiIsDeviceConnected(0, LogitechGSDK.LOGI_DEVICE_TYPE_WHEEL);

        /// <summary>
        /// Returns the wheel's rotation relative to its physical range of movement (-1.0 to 1.0)
        /// </summary>
        public static float WheelAxis               => 
            // Steering wheel value
            (!Wheel ? 0 : Instance._joyStatus.lX / (float)Int16.MaxValue) + EmulatedWheel;
        /// <summary>
        /// 
        /// Returns the number of revolutions the wheel has made (-1.5 to 1.5)
        /// </summary>
        public static float WheelAxisRevolutions    => 
            WheelAxis / 1.25f;
        /// <summary>
        /// Returns the wheel's rotation in degrees (-540.0 to 540.0)
        /// </summary>
        public static float WheelAxisDegrees        => 
            WheelAxis * 450;
        /// <summary>
        /// Returns how far the accelerator has been depressed, from 0 (resting position) to 1 (fully pressed).
        /// </summary>
        public static float AxisPedalAccelerator    => 
            (!Wheel ? 0 : AbsoluteIntToPercent(Instance._joyStatus.lY)) + EmulatedAccelerator;
        /// <summary>
        /// Returns how far the brake has been depressed, from 0 (resting position) to 1 (fully pressed).
        /// </summary>
        public static float AxisPedalBrake          => 
            (!Wheel ? 0 : AbsoluteIntToPercent(Instance._joyStatus.lRz)) + EmulatedBrake;
        /// <summary>
        /// Returns how far the clutch has been depressed, from 0 (resting position) to 1 (fully pressed).
        /// </summary>
        public static float AxisPedalClutch         => 
            (!Wheel || Instance._joyStatus.rglSlider == null ? 0 : AbsoluteIntToPercent(Instance._joyStatus.rglSlider[0])) + EmulatedClutch;
        #endregion

        #region Config
        private static LogitechUtilConfig _config;
        public static LogitechUtilConfig Config {
            get {
                // If a config hasn't been cached
                if (_config == null) {
                    Debug.Log($"No config cached, loading resource with name {ConfigName}.");
                    // Try to load a config from resources
                    if (Resources.Load(ConfigName) is LogitechUtilConfig) {
                        _config = (LogitechUtilConfig)Resources.Load(ConfigName);
                        if (Config.loggingMode == LogitechUtilConfig.LoggingModes.Verbose)
                            Debug.Log("Loaded config.");
                        // If no such resource exists
                    } else {
                        // Create one, if in the editor
                        #if UNITY_EDITOR
                        Debug.LogWarning($"No config could be loaded, creating new config asset at {ConfigPath}");
                        _config = LogitechEditorUtil.CreateConfigFile();
                        // If not in the editor, print an error (code to create a resource is editor-exclusive, and cannot be included in builds)
                        #else
                        Debug.LogError("Could not find a config file. Was this build created without a config?");
                        #endif
                    }
                    
                }
                return _config;
            }
            
        }
        #endregion

        // Logitech SDK
        private static bool _sdkInitialized;
        private LogitechGSDK.DIJOYSTATE2ENGINES _joyStatus;
        private LogitechGSDK.LogiControllerPropertiesData _joyProps;
        
        // Error suppression/management (helps avoid flooding the console with the same error);
        private Exception _lastPrintedException;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {

            if (Config.loggingMode >= LogitechUtilConfig.LoggingModes.Normal)
                Debug.Log("Initializing LogiSteering...");

            // [Attempt to] initialize Logitech Steering
            try {
                if (LogitechGSDK.LogiSteeringInitialize(true)) {
                    if (Config.loggingMode >= LogitechUtilConfig.LoggingModes.Normal)
                        Debug.Log("Successfully initialized LogiSteering.");
                    _sdkInitialized = true;
                } else if (Config.loggingMode >= LogitechUtilConfig.LoggingModes.ErrorsOnly)
                    Debug.LogError("Failed to initialize LogiSteering!");
            } catch (DllNotFoundException e) {
                // ignored
                if (Config.loggingMode >= LogitechUtilConfig.LoggingModes.ErrorsOnly)
                    Debug.LogError("LogiSDK DLL missing!");
                _lastPrintedException = e;
            } catch (Exception e) {
                // ignored
                if (Config.loggingMode >= LogitechUtilConfig.LoggingModes.ErrorsOnly)
                    Debug.LogError($"Unknown exception while initializing LogiSteering:\n{e}");
            }

            // Cache actions on start to avoid expensive FindAction method calls during gameplay
            _isActionWheel          = Config.keyboardActions.FindAction("Wheel");
            _isActionAccelerator    = Config.keyboardActions.FindAction("Accelerator");
            _isActionBrake          = Config.keyboardActions.FindAction("Brake");
            _isActionClutch         = Config.keyboardActions.FindAction("Clutch");

        }

        // Update is called once per frame
        void Update() {

            if (Config.loggingMode == LogitechUtilConfig.LoggingModes.Verbose) {

                Debug.Log(
                    $"Status: SDK {_sdkInitialized}, Wheel Connected {LogitechGSDK.LogiIsDeviceConnected(0, LogitechGSDK.LOGI_DEVICE_TYPE_WHEEL)}, Wheel: {Wheel}");

                if (_joyStatus.rglSlider != null)
                    for (int i = 0; i < _joyStatus.rglSlider.Length; i++)
                        Debug.Log($"RGL Slider {i} {Instance._joyStatus.rglSlider[i]}");

                Debug.Log($"lX {Instance._joyStatus.lX}");
                Debug.Log($"lY {Instance._joyStatus.lY}");
                Debug.Log($"lZ {Instance._joyStatus.lZ}");
                Debug.Log($"lRX {Instance._joyStatus.lRx}");
                Debug.Log($"lRY {Instance._joyStatus.lRy}");
                Debug.Log($"lRZ {Instance._joyStatus.lRz}");

            }

            if (Config.useKeyboardEmulation) {
                
                EmulatedWheel = Mathf.Clamp(EmulatedWheel + _isActionWheel.ReadValue<float>() * Time.deltaTime, -1, 1);
                
                EmulatedAccelerator -= Time.deltaTime;
                EmulatedAccelerator = Mathf.Clamp01(EmulatedAccelerator + _isActionAccelerator.ReadValue<float>() * Time.deltaTime * 2);

                EmulatedBrake -= Time.deltaTime;
                EmulatedBrake = Mathf.Clamp01(EmulatedBrake + _isActionBrake.ReadValue<float>() * Time.deltaTime * 2);
                
                EmulatedClutch -= Time.deltaTime;
                EmulatedClutch = Mathf.Clamp01(EmulatedClutch + _isActionClutch.ReadValue<float>() * Time.deltaTime * 2);
            
            } else {
                
                EmulatedWheel = 0;
                EmulatedAccelerator = 0;
                EmulatedBrake = 0;
                EmulatedClutch = 0;
                
            }

            // For the sake of simplicity, we'll only check if the first device is a steering wheel. A more robust system should detect if *any* connected devices is a steering wheel.
            if (Wheel) {
                
                // Update Logi API
                LogitechGSDK.LogiUpdate();

                // Get wheel status, stored in a JOYSTATES2ENGINES struct
                _joyStatus = LogitechGSDK.LogiGetStateUnity(0); // Gets the joystick status (e.g. wheel angle, etc.)

                if (Config.loggingMode >= LogitechUtilConfig.LoggingModes.Verbose)
                    Debug.Log("Updated LogiSDK JoyStatus");

            } else if (!_sdkInitialized) {

                // Attempt to initialize logi steering
                if (Config.attemptWheelInitAtRuntime) {
                    try {
                        if (LogitechGSDK.LogiSteeringInitialize(true)) {
                            if (Config.loggingMode >= LogitechUtilConfig.LoggingModes.Normal)
                                Debug.Log("Successfully initialized LogiSteering.");
                            _sdkInitialized = true;
                        } else if (_lastPrintedException != null) {
                            if (Config.loggingMode >= LogitechUtilConfig.LoggingModes.ErrorsOnly)
                                Debug.LogError("Failed to initialize LogiSteering!");
                            _lastPrintedException = null;
                        }
                    }
                    catch (DllNotFoundException e) {
                        if (_lastPrintedException is not DllNotFoundException) {
                            if (Config.loggingMode >= LogitechUtilConfig.LoggingModes.ErrorsOnly)
                                Debug.LogError("LogiSDK DLL missing!");
                            _lastPrintedException = e;
                        }
                    }
                    catch (Exception e) {
                        if (_lastPrintedException != e) {
                            if (Config.loggingMode >= LogitechUtilConfig.LoggingModes.Normal)
                            Debug.LogWarning("No steering wheel connected.");
                            _lastPrintedException = e;
                        }
                    }
                }

            }
        }

        private void OnDestroy() => Shutdown();
        private void OnApplicationQuit() => Shutdown();
        private void Shutdown(){
            try {
                LogitechGSDK.LogiStopSpringForce(0);
                LogitechGSDK.LogiSteeringShutdown();
            } catch{
                //Ignore
            }
        }

        /// <summary>
        /// Returns a float between 0-1 corresponding to the value relative to integer range (-32768, 32767)
        /// </summary>
        /// <param name="value">Int32 value to convert to float between 0 and 1</param>
        /// <returns>Float between 0 and 1</returns>
        public static float AbsoluteIntToPercent(int value) {
            // Logi API returns values between the min and max values for a 16-bit integer
            return (float)value / Int16.MaxValue / -2 + 0.5f;
        }

        /// <summary>
        /// Sets the steering wheel's spring force intensity and angle
        /// </summary>
        /// <param name="angle">The angle the wheel should spring to</param>
        /// <param name="saturation">The saturation of the spring force (refer to LogiSDK documentation)</param>
        /// <param name="coefficient">The coefficient of the spring force (refer to LogiSDK documentation)</param>
        public static bool SetSpringForce(float angle, float saturation, float coefficient) {
            if (!Wheel) {
                if (Config.loggingMode >= LogitechUtilConfig.LoggingModes.Normal)
                    Debug.LogWarning("Could not set spring force; no wheel available.");
                return false;
            }
            LogitechGSDK.LogiPlaySpringForce(
                0, 
                Mathf.RoundToInt(angle * 100), 
                Mathf.RoundToInt(saturation * 100), 
                Mathf.RoundToInt(coefficient * 100)
            );

            return true;
        }

        public static bool StopSpringForce() {
            
            if (!Wheel) {
                if (Config.loggingMode >= LogitechUtilConfig.LoggingModes.Normal)
                    Debug.LogWarning("Could not set spring force; no wheel available.");
                return false;
            }

            LogitechGSDK.LogiStopSpringForce(0);

            return true;
            
        }

    }

}