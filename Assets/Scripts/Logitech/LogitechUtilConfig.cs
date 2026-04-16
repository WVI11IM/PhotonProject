using UnityEngine;
using UnityEngine.InputSystem;

namespace Logitech {

    public class LogitechUtilConfig : ScriptableObject {

        public enum LoggingModes {

            None,
            ErrorsOnly,
            Normal,
            Verbose

        }
        
        [Tooltip("Whether to call LogiSteeringInitialize on update when a wheel isn't present. This allows a wheel to connect and be detected without having to restart the game.")]
        public bool attemptWheelInitAtRuntime = true;
        public bool useKeyboardEmulation = true;
        [SerializeField]
        public LoggingModes loggingMode = LoggingModes.Normal;
        public InputActionAsset keyboardActions;

    }

}