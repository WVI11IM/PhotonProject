using System;
using Logitech;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Logitech {

    public class LogitechUtil : MonoBehaviour {

        #region Singleton Logic
        private static LogitechUtil _instance;
        public static LogitechUtil Instance {
            get {
                if (!_instance) {
                    _instance = new GameObject("LogitechUtil Singleton", typeof(LogitechUtil)).GetComponent<LogitechUtil>();
                    // _instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
                }
                return _instance;
            }
        }
        #endregion
        
        // Config
        [Tooltip("Whether to call LogiSteeringInitialize on update when a wheel isn't present. This allows a wheel to connect and be detected without having to restart the game.")]
        [SerializeField] private bool attemptWheelInitAtRuntime = true;
        
        #region InputSystem

        private static float _wheelBackingField;
        private static float _acceleratorBackingField;
        private static float _brakeBackingField;
        private static float _clutchBackingField;
        private static InputAction _isActionWheel;
        private static InputAction _isActionAccelerator;
        private static InputAction _isActionBrake;
        private static InputAction _isActionClutch;

        #endregion
        
        #region Properties
        /// <summary>
        /// Returns the wheel's rotation relative to its physical range of movement (-1.0 to 1.0)
        /// </summary>
        public static float WheelAxis               => 
            // Steering wheel value
            (Instance == null ? 0 : Instance._joyStatus.lX / (float)Int16.MaxValue) +
            // Keyboard emulation value (if enabled, else zero)
            (Config.useKeyboardEmulation ? _wheelBackingField : 0);
        /// <summary>
        /// 
        /// Returns the number of revolutions the wheel has made (-1.5 to 1.5)
        /// </summary>
        public static float WheelAxisRevolutions    => 
            (Instance == null ? 0 : Instance._joyStatus.lX / (float)Int16.MaxValue / 1.25f);
        /// <summary>
        /// Returns the wheel's rotation in degrees (-540.0 to 540.0)
        /// </summary>
        public static float WheelAxisDegrees        => 
            (Instance == null ? 0 : Instance._joyStatus.lX / (float)Int16.MaxValue * 450);
        /// <summary>
        /// Returns how far the accelerator has been depressed, from 0 (resting position) to 1 (fully pressed).
        /// </summary>
        public static float AxisPedalAccelerator    => 
            AbsoluteIntToPercent(Instance == null ? 0 : Instance._joyStatus.lY);
        /// <summary>
        /// Returns how far the brake has been depressed, from 0 (resting position) to 1 (fully pressed).
        /// </summary>
        public static float AxisPedalBrake          => 
            AbsoluteIntToPercent(Instance == null ? 0 : Instance._joyStatus.lRz);
        /// <summary>
        /// Returns how far the clutch has been depressed, from 0 (resting position) to 1 (fully pressed).
        /// </summary>
        public static float AxisPedalClutch         => 
            AbsoluteIntToPercent(Instance._joyStatus.rglSlider == null ? 0 : Instance._joyStatus.rglSlider[0]);
        #endregion
        
        // Logitech SDK
        private LogitechGSDK.DIJOYSTATE2ENGINES _joyStatus;
        private LogitechGSDK.LogiControllerPropertiesData _joyProps;

        private Exception _lastPrintedException;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            
            Debug.Log("Initializing LogiSteering...");

            try {
                if (LogitechGSDK.LogiSteeringInitialize(true))
                    Debug.Log("Successfully initialized LogiSteering.");
                else
                    Debug.LogError("Failed to initialize LogiSteering!");
            } catch (DllNotFoundException e) {
                // ignored
                Debug.LogError("LogiSDK DLL missing!");
                _lastPrintedException = e;
            } catch (Exception e) {
                // ignored
                Debug.LogError($"Unknown exception while initializing LogiSteering:\n{e}");
            }

            _isActionWheel          = Config.keyboardActions.FindAction("Wheel");
            _isActionAccelerator    = Config.keyboardActions.FindAction("Accelerator");
            _isActionBrake          = Config.keyboardActions.FindAction("Brake");
            _isActionClutch         = Config.keyboardActions.FindAction("Clutch");

        }

        // Update is called once per frame
        void Update() {
            if (_config.useKeyboardEmulation) {
                //TODO: Implement keyboard emulation using InputSystem actions.
            } else {
                // If the previous exception was a DLL Not Found, halt Logi input detection outright
                // For the sake of simplicity, we'll only check if the first device is a steering wheel. A more robust system should detect if *any* connected devices is a steering wheel.
                if (_lastPrintedException is not DllNotFoundException &&
                    LogitechGSDK.LogiIsDeviceConnected(0, LogitechGSDK.LOGI_DEVICE_TYPE_WHEEL)) {
                    // Update Logi API
                    LogitechGSDK.LogiUpdate();

                    // Get wheel status, stored in a JOYSTATES2ENGINES struct
                    _joyStatus = LogitechGSDK.LogiGetStateUnity(0); // Gets the joystick status (e.g. wheel angle, etc.)
                } else {

                    // Attempt to initialize logi steering
                    if (attemptWheelInitAtRuntime) {
                        try {
                            if (LogitechGSDK.LogiSteeringInitialize(true))
                                Debug.Log("Successfully initialized LogiSteering.");
                            else if (_lastPrintedException != null) {
                                Debug.LogError("Failed to initialize LogiSteering!");
                                _lastPrintedException = null;
                            }
                        }
                        catch (DllNotFoundException e) {
                            if (_lastPrintedException is not DllNotFoundException) {
                                Debug.LogError("LogiSDK DLL missing!");
                                _lastPrintedException = e;
                            }
                        }
                        catch (Exception e) {
                            if (_lastPrintedException != e) {
                                Debug.LogWarning("No steering wheel connected.");
                                _lastPrintedException = e;
                            }
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

        #region Config
        private static LogitechUtilSettingsScriptableObject _config;

        private static LogitechUtilSettingsScriptableObject Config {
            get {
                if (!_config)
                    LoadConfig();

                return _config;
            }
        }

        private static void LoadConfig() {
            _config = Resources.Load<LogitechUtilSettingsScriptableObject>("LogitechUtilConfig");

            if (!_config)
                throw new NullReferenceException(
                    "LogitechUtilConfig asset could not be located. Make sure a LogitechUtilSettingsScriptableObject named \"LogitechUtilConfig\" exists in a Resources folder.");
        }
        #if UNITY_EDITOR
        [MenuItem("Logitech Utility/Config/Enable Keyboard Emulation")]
        #endif
        private static void ConfigEnableKeyboard() {
            try {
                LoadConfig();
            } catch (Exception e) {
                Debug.LogError(e);
                return;
            }
            _config.useKeyboardEmulation = true;
            Debug.Log("Steering wheel emulation via keyboard input has been enabled.");
        }
        #if UNITY_EDITOR
        [MenuItem("Logitech Utility/Config/Disable Keyboard Emulation")]
        #endif
        private static void ConfigDisableKeyboard() {
            try {
                LoadConfig();
            } catch (Exception e) {
                Debug.LogError(e);
                return;
            }
            _config.useKeyboardEmulation = true;
            Debug.Log("Steering wheel emulation via keyboard input has been disabled.");
        }
        #endregion

    }

}