using System;
using Logitech;
using UnityEngine;
using UnityEngine.UI;

namespace Logitech {

    public class LogitechUtil : MonoBehaviour {

        private static LogitechUtil _instance;

        public static LogitechUtil Instance {
            get {
                if (!_instance) {
                    _instance = new GameObject("LogitechUtil Singleton", typeof(LogitechUtil)).GetComponent<LogitechUtil>();
                    _instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
                }
                return _instance;
            }
        }

        public static float AxisWheel              => Instance._joyStatus.lX / (float)Int16.MaxValue;
        public static float AxisPedalAccelerator   => AbsoluteIntToPercent(Instance._joyStatus.lY);
        public static float AxisPedalBrake         => AbsoluteIntToPercent(Instance._joyStatus.lRz);
        public static float AxisPedalClutch        => AbsoluteIntToPercent(Instance._joyStatus.rglSlider[0]);

        private LogitechGSDK.DIJOYSTATE2ENGINES _joyStatus;
        private LogitechGSDK.LogiControllerPropertiesData _joyProps;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            Debug.Log("Initializing LogiSteering...");
            if (LogitechGSDK.LogiSteeringInitialize(true))
                Debug.Log("Successfully initialized LogiSteering.");
            else
                Debug.LogError("Failed to initialize LogiSteering!");
        }

        // Update is called once per frame
        void Update() {
            // Update Logi API
            LogitechGSDK.LogiUpdate();
            // Get wheel status, stored in a JOYSTATES2ENGINES struct
            _joyStatus = LogitechGSDK.LogiGetStateUnity(0); // Gets the joystick status (e.g. wheel angle, etc.)
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

    }

}