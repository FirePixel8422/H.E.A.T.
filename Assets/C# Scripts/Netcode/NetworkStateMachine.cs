using System.Collections;
using Unity.Netcode;
using UnityEngine;


namespace FirePixel.Networking
{
    public class NetworkStateMachine : NetworkBehaviour
    {
        private Animator anim;
        private RagDollController ragDollController;


        #region animation data

        [Header("Start Animation")]
        [SerializeField] private string currentAnimation = "Idle";


        [Header("Animation Names")]
        [SerializeField] private string idleAnimation = "Idle";

        [SerializeField] private string crouchAnimation = "Crouch";
        [SerializeField] private string crouchWalkAnimation = "CrouchWalk";

        [SerializeField] private string walkAnimation = "Walk";
        [SerializeField] private string sprint = "SDebugLogger.Log";

        [SerializeField] private string hurtAnimation = "Hurt";


        private int currentAnimationHash;
        private int idleAnimationHash;
        private int crouchAnimationHash;
        private int crouchWalkAnimationHash;
        private int walkAnimationHash;
        private int sprintHash;
        private int hurtAnimationHash;

        #endregion


        [SerializeField] private bool dead;




        private void Start()
        {
            anim = GetComponent<Animator>();
            ragDollController = GetComponent<RagDollController>();

            // Cache animation hashes for performance
            currentAnimationHash = Animator.StringToHash(currentAnimation);
            idleAnimationHash = Animator.StringToHash(idleAnimation);
            crouchAnimationHash = Animator.StringToHash(crouchAnimation);
            crouchWalkAnimationHash = Animator.StringToHash(crouchWalkAnimation);
            walkAnimationHash = Animator.StringToHash(walkAnimation);
            sprintHash = Animator.StringToHash(sprint);
            hurtAnimationHash = Animator.StringToHash(hurtAnimation);
        }




        #region Change/Transition Animation + Server Sync Functions

        /// <returns>true if the animation has changed, false otherwise</returns>
        private bool TryTransitionAnimation(int animationHash, float transitionDuration = 0.25f, float speed = 1, int layer = 0)
        {
            //if the new animation is the same as current, return false
            if (currentAnimationHash == animationHash) return false;


            SyncAnimation_ServerRPC(ClientManager.LocalClientGameId, animationHash, transitionDuration, speed, layer);

            currentAnimationHash = animationHash;

            anim.speed = speed;
            anim.CrossFade(animationHash, transitionDuration, layer);

            return true;
        }

        /// <summary>
        /// Sent Animation Data trough server, back to all clients except sender.
        /// </summary>
        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void SyncAnimation_ServerRPC(int fromClientGameId, int animationHash, float transitionDuration, float speed = 1, int layer = 0)
        {
            SyncAnimation_ClientRPC(animationHash, transitionDuration, speed, layer, GameIdRPCTargets.SendToOppositeClient(fromClientGameId));
        }

        [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void SyncAnimation_ClientRPC(int animationHash, float transitionDuration, float speed = 1, int layer = 0, GameIdRPCTargets rpcTargets = default)
        {
            if (rpcTargets.IsTarget == false) return;

            anim.speed = speed;
            anim.CrossFade(animationHash, transitionDuration, layer);
        }

        #endregion


        public void UpdateMovementState(bool moving, bool crouching, bool sprinting, float transitionDuration = 0.25f)
        {
            if (dead) return;

            if (moving)
            {
                if (crouching)
                    CrouchWalk(transitionDuration);
                else if (sprinting)
                    Sprint(transitionDuration);
                else
                    Walk(transitionDuration);
            }
            else
            {
                if (crouching)
                    Crouch(transitionDuration);
                else
                    Idle(transitionDuration);
            }
        }

        private void Idle(float transitionDuration = 0.25f)
        {
            TryTransitionAnimation(idleAnimationHash, transitionDuration);
        }

        private void Crouch(float transitionDuration = 0.25f)
        {
            TryTransitionAnimation(crouchAnimationHash, transitionDuration);
        }
        private void CrouchWalk(float transitionDuration = 0.25f)
        {
            TryTransitionAnimation(crouchWalkAnimationHash, transitionDuration);
        }

        private void Walk(float transitionDuration = 0.25f)
        {
            TryTransitionAnimation(walkAnimationHash, transitionDuration);
        }
        private void Sprint(float transitionDuration = 0.25f)
        {
            TryTransitionAnimation(sprintHash, transitionDuration);
        }


        public void GetHurt(float transitionDuration = 0.25f)
        {
            if (dead) return;

            TryTransitionAnimation(hurtAnimationHash, transitionDuration);

            AutoTransition(idleAnimationHash, transitionDuration);
        }
        public void Die(Vector3 ragdollDirection, Vector3 ragdollImpactPoint, float transitionDuration = 0.25f)
        {
            dead = true;

            ragDollController.StartRagdoll(ragdollDirection, ragdollImpactPoint);
        }


        /// <summary>
        /// Create an auto transition to target animation after current animation finishes playing.
        /// </summary>
        private void AutoTransition(int animationHash, float transitionDuration, int layer = 0)
        {
            StopAllCoroutines();
            StartCoroutine(AutoTransitionCoroutine(animationHash, transitionDuration, layer));
        }
        private IEnumerator AutoTransitionCoroutine(int animationHash, float transitionDuration, int layer = 0)
        {
            float clipTime = anim.GetCurrentAnimatorStateInfo(layer).length;

            yield return new WaitForSeconds(clipTime);

            TryTransitionAnimation(animationHash, transitionDuration);
        }
    }
}