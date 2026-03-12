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
        
        #region InputSystem

        private InputAction _isActionWheel;
        private InputAction _isActionAccelerator;
        private InputAction _isActionBrake;
        private InputAction _isActionClutch;

        #endregion
        
        #region Properties
        /// <summary>
        /// Returns the wheel's rotation relative to its physical range of movement (-1.0 to 1.0)
        /// </summary>
        public static float WheelAxis               => 
            // Steering wheel value
            (Instance == null ? 0 : Instance._joyStatus.lX / (float)Int16.MaxValue) +
            // Keyboard emulation value (if enabled, else zero)
            (Config.allowKeyboardEmulation ? Config.keyboardActions["Wheel"].GetControlMagnitude() : 0);
        /// <summary>
        /// Returns the number of revolutions the wheel has made (-1.5 to 1.5)
        /// </summary>
        public static float WheelAxisRevolutions    => 
            (Instance == null ? 0 : Instance._joyStatus.lX / (float)Int16.MaxValue / 1.5f); //TODO: Test with wheel, ensure this value is accurate
        /// <summary>
        /// Returns the wheel's rotation in degrees (-540.0 to 540.0)
        /// </summary>
        public static float WheelAxisDegrees        => 
            (Instance == null ? 0 : Instance._joyStatus.lX / (float)Int16.MaxValue / 540); //TODO: Test with wheel, ensure this value is accurate
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

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            
            Debug.Log("Initializing LogiSteering...");
            
            if (LogitechGSDK.LogiSteeringInitialize(true))
                Debug.Log("Successfully initialized LogiSteering.");
            else
                Debug.LogError("Failed to initialize LogiSteering!");

            _isActionWheel          = Config.keyboardActions.FindAction("Wheel");
            _isActionAccelerator    = Config.keyboardActions.FindAction("Accelerator");
            _isActionBrake          = Config.keyboardActions.FindAction("Brake");
            _isActionClutch         = Config.keyboardActions.FindAction("Clutch");

            _isActionWheel.started += (InputAction.CallbackContext context) => Debug.Log("Wheel actions started");
            _isActionWheel.canceled += (InputAction.CallbackContext context) => Debug.Log("Wheel actions canceled");
            _isActionWheel.performed += (InputAction.CallbackContext context) => Debug.Log("Wheel actions performed");
            
            Debug.Log(_isActionWheel);

        }

        // Update is called once per frame
        void Update() {
            Debug.Log(_isActionWheel.ReadValue<double>());
            // Update Logi API
            LogitechGSDK.LogiUpdate();
            // For the sake of simplicity, we'll only check if the first device is a steering wheel. A more robust system should detect if *any* connected devices is a steering wheel.
            if (LogitechGSDK.LogiIsDeviceConnected(0, LogitechGSDK.LOGI_DEVICE_TYPE_WHEEL))
                // Get wheel status, stored in a JOYSTATES2ENGINES struct
                _joyStatus = LogitechGSDK.LogiGetStateUnity(0); // Gets the joystick status (e.g. wheel angle, etc.)
            else
                Debug.LogWarning("No steering wheel connected.");
        }
        
        private void OnDestroy() => Shutdown();
        private void OnApplicationQuit() => Shutdown();
        private void Shutdown(){
            LogitechGSDK.LogiStopSpringForce(0);
            LogitechGSDK.LogiSteeringShutdown();
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
        [MenuItem("Logitech Utility/Config/Enable Keyboard Emulation")]
        private static void ConfigEnableKeyboard() {
            try {
                LoadConfig();
            } catch (Exception e) {
                Debug.LogError(e);
                return;
            }
            _config.allowKeyboardEmulation = true;
            Debug.Log("Steering wheel emulation via keyboard input has been enabled.");
        }
        [MenuItem("Logitech Utility/Config/Disable Keyboard Emulation")]
        private static void ConfigDisableKeyboard() {
            try {
                LoadConfig();
            } catch (Exception e) {
                Debug.LogError(e);
                return;
            }
            _config.allowKeyboardEmulation = true;
            Debug.Log("Steering wheel emulation via keyboard input has been disabled.");
        }
        #endregion

    }

}