using Pilot.Ship;
using UnityEngine;
using UnityEngine.Animations;

namespace Enemies.Strider.FSM {

    public class StateSeek : StateBase {

        private static readonly int Barrage = Animator.StringToHash("Barrage");

        [SerializeField] private float force;
        [Tooltip("The minimum distance the Strider has to be from the player to be in seek mode.")]
        [SerializeField] private float minDistance;

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) {
            
            base.OnStateUpdate(animator, stateInfo, layerIndex, controller);
            
            // Seek the ship
            FiniteStateMachine.Core.Rb.AddForce((ShipCore.Instance.transform.position - FiniteStateMachine.Core.transform.position).normalized * force * Time.deltaTime);

            // If within range, enter Barrage state
            if (Vector2.Distance(FiniteStateMachine.Core.transform.position, ShipCore.Instance.transform.position) < minDistance)
                FiniteStateMachine.fsmAnimator.SetTrigger(Barrage);
            
            // Turn to face movement direction
            FiniteStateMachine.Core.TorqueToFace(FiniteStateMachine.Core.Rb.linearVelocity);
            
        }

    }

}