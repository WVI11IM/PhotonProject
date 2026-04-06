using UnityEngine;
using UnityEngine.Animations;

namespace Enemies.Strider.FSM {

    public class StateFlee : StateBase {

        private static readonly int Barrage = Animator.StringToHash("Barrage");

        [SerializeField] private float force;
        [Tooltip("The maximum distance the Strider has to be from the player to be in flee mode.")]
        [SerializeField] private float maxDistance;

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) {
            
            base.OnStateUpdate(animator, stateInfo, layerIndex, controller);
            
            // Flee the ship
            FiniteStateMachine.Core.Rb.AddForce( (FiniteStateMachine.Core.transform.position - FiniteStateMachine.Core.Ship.position).normalized * force * Time.deltaTime);

            // If outside range, enter Barrage state
            if (Vector2.Distance(FiniteStateMachine.Core.transform.position, FiniteStateMachine.Core.Ship.position) > maxDistance)
                FiniteStateMachine.fsmAnimator.SetTrigger(Barrage);
            
            // Turn to face movement direction
            FiniteStateMachine.Core.TorqueToFace(FiniteStateMachine.Core.Rb.linearVelocity);
            
        }

    }

}