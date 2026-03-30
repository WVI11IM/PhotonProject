using UnityEngine;
using UnityEngine.InputSystem;

namespace Logitech {

    [CreateAssetMenu(fileName = "LogitechUtilConfig", menuName = "Logitech Utility/Config File", order = 1)]
    public class LogitechUtilSettingsScriptableObject : ScriptableObject {

        public bool useKeyboardEmulation;
        public InputActionAsset keyboardActions;

    }

}