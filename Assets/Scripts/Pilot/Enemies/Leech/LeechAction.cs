using Unity.Behavior;

namespace Enemies.Leech {

    public class LeechAction : Action {

        private LeechCore _leech;
        public LeechCore Leech {
            get {
                if (!_leech)
                    _leech = Parent.GameObject.GetComponent<LeechCore>();
                return _leech;
            }
        }

    }

}