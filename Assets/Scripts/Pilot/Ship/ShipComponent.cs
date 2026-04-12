using UnityEngine;

namespace Pilot.Ship {

    [RequireComponent(typeof(ShipCore))]
    public abstract class ShipComponent : MonoBehaviour {

        private ShipCore _core;

        protected ShipCore Core {
            get {
                if (_core == null)
                    _core = GetComponent<ShipCore>();
                return _core;
            }
        }

    }

}