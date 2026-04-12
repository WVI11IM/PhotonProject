using UnityEngine;

namespace Enemies.Strider.FSM {

    public abstract class StateBase : StateMachineBehaviour {

        protected StriderFSM FiniteStateMachine { get; private set; }

        public virtual void Init(StriderFSM finiteStateMachine) {
            this.FiniteStateMachine = finiteStateMachine;
        }
        
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
        }

    }

}