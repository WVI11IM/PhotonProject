using UnityEngine;

namespace Enemies.Strider.FSM {

    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(StriderCore))]
    public class StriderFSM : MonoBehaviour {

        public StriderCore Core { get; private set; }
        
        public Animator fsmAnimator { get; private set; }
        
        private void Awake() {

            fsmAnimator = GetComponent<Animator>();
            Core = GetComponent<StriderCore>();
            
            StateBase[] behaviours = fsmAnimator.GetBehaviours<StateBase>();
            
            foreach (var behaviour in behaviours)
                behaviour.Init(this);

        }

    }

}