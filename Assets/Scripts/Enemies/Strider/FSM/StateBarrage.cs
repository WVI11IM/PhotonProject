using UnityEngine;
using UnityEngine.Animations;

namespace Enemies.Strider.FSM {

    public class StateBarrage : StateBase {

        private static readonly int Flee = Animator.StringToHash("Flee");
        private static readonly int Seek = Animator.StringToHash("Seek");

        [Tooltip("The minimum distance the Strider has to be from the player to stay in barrage mode.")]
        [SerializeField] private float minDistance;
        [Tooltip("The maximum distance the Strider has to be from the player to stay in barrage mode.")]
        [SerializeField] private float maxDistance;
        [Tooltip("How many bullets to shoot.")]
        [SerializeField] private int bulletCount;
        [Tooltip("Range of angle to apply to bullets")]
        [SerializeField] private float bulletSpread;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller) {
            
            base.OnStateUpdate(animator, stateInfo, layerIndex, controller);
            
            // Shoot volley of bullets at the player
            FiniteStateMachine.Core.ShootVolley(bulletCount, bulletSpread);

            // If too close, enter Flee state
            if (Vector2.Distance(FiniteStateMachine.Core.transform.position, FiniteStateMachine.Core.Ship.position) < minDistance)
                FiniteStateMachine.fsmAnimator.SetTrigger(Flee);
            // If too far, enter Seek state
            if (Vector2.Distance(FiniteStateMachine.Core.transform.position, FiniteStateMachine.Core.Ship.position) > maxDistance)
                FiniteStateMachine.fsmAnimator.SetTrigger(Seek);
            
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) { base.OnStateUpdate(animator, stateInfo, layerIndex);
            // Turn to face the player
            FiniteStateMachine.Core.TorqueToFace(FiniteStateMachine.Core.Ship.position - FiniteStateMachine.Core.transform.position);
        }

    }

}