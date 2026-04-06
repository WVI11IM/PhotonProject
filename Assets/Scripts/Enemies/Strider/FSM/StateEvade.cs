using UnityEngine;
using UnityEngine.Animations;

namespace Enemies.Strider.FSM {

    public class StateEvade : StateBase {

        private static readonly int Barrage = Animator.StringToHash("Barrage");

        [SerializeField] private float force;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) {
            
            base.OnStateUpdate(animator, stateInfo, layerIndex, controller);
            
            // Apply force left or right
            FiniteStateMachine.Core.Rb.AddForce((Random.value > 0.5f ? -1 : 1) * FiniteStateMachine.Core.transform.right * force);
            
            // Immediately return to barrage state
            FiniteStateMachine.fsmAnimator.SetTrigger(Barrage);
            
        }
        
    }

}