using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;


namespace FirePixel.Networking
{
    public class NetworkStateMachine : NetworkBehaviour
    {
        #region Animation Data

        [Header("Start Animation")]
        [SerializeField] private string[] currentAnimation = { "Idle", "ShakeGooglyEyes" };


        [Header("Animation Names")]
        [SerializeField] private string idleAnimation = "Idle";

        [SerializeField] private string crouchAnimation = "Crouch";
        [SerializeField] private string crouchWalkAnimation = "CrouchWalk";

        [SerializeField] private string walkAnimation = "Walk";
        [SerializeField] private string sprintAnimation = "Sprint";

        [SerializeField] private string jumpAnimation = "Jump";
        [SerializeField] private string fallAnimation = "Falling";

        [SerializeField] private string shakeGooglyEyesAnimation = "ShakeGooglyEyes";
        [SerializeField] private string eyesCuriousAnimation = "EyesCurious";


        private int[] currentAnimationHashes;

        private int idleAnimationHash;
        private int crouchAnimationHash;
        private int crouchWalkAnimationHash;
        private int walkAnimationHash;
        private int sprintAnimationHash;
        
        private int jumpAnimationHash;
        private int fallAnimationHash;
        
        private int shakeGooglyEyesAnimationHash;
        private int eyesCuriousAnimationHash;
        
        #endregion


        private Animator anim;
        private RagDollController ragDollController;
        private Rigidbody rb;

        private int animationLayerCount;

        private Coroutine[] autoTransitiosCOs;

        private bool IsJumping => currentAnimationHashes[0] == jumpAnimationHash;
        [SerializeField] private bool dead;




        private void Awake()
        {
            anim = GetComponent<Animator>();
            ragDollController = GetComponent<RagDollController>();
            rb = GetComponent<Rigidbody>();

            animationLayerCount = anim.layerCount;

            currentAnimationHashes = new int[animationLayerCount];
            autoTransitiosCOs = new Coroutine[animationLayerCount];

            idleAnimationHash = Animator.StringToHash(idleAnimation);
            crouchAnimationHash = Animator.StringToHash(crouchAnimation);
            crouchWalkAnimationHash = Animator.StringToHash(crouchWalkAnimation);
            walkAnimationHash = Animator.StringToHash(walkAnimation);
            sprintAnimationHash = Animator.StringToHash(sprintAnimation);

            jumpAnimationHash = Animator.StringToHash(jumpAnimation);
            fallAnimationHash = Animator.StringToHash(fallAnimation);

            shakeGooglyEyesAnimationHash = Animator.StringToHash(shakeGooglyEyesAnimation);
            eyesCuriousAnimationHash = Animator.StringToHash(eyesCuriousAnimation);

            // Get and set the start animation hashes
            for (int i = 0; i < animationLayerCount; i++)
            {
                currentAnimationHashes[i] = Animator.StringToHash(currentAnimation[i]);

                anim.speed = 1;
                anim.CrossFadeInFixedTime(currentAnimationHashes[i], 0, i);
            }
        }




        #region Change/Transition Animation + Server Sync Functions

        /// <returns>true if the animation has changed, false otherwise</returns>
        private bool TryTransitionAnimation(int animationHash, float transitionDuration = 0.25f, float speed = 1, int layer = 0)
        {
            //if the new animation is the same as current, return false
            if (currentAnimationHashes[layer] == animationHash) return false;

            //DebugLogger.Log($"Transitioning to animation: {animationHash} with duration: {transitionDuration}, speed: {speed}, layer: {layer}");

            SyncAnimation_ServerRPC(ClientManager.LocalClientGameId, animationHash, transitionDuration, speed, layer);

            TransitionAnimation(animationHash, transitionDuration, speed, layer);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TransitionAnimation(int animationHash, float transitionDuration, float speed, int layer)
        {
            currentAnimationHashes[layer] = animationHash;

            anim.speed = speed;
            anim.CrossFadeInFixedTime(animationHash, transitionDuration, layer);
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

            TransitionAnimation(animationHash, transitionDuration, speed, layer);
        }

        #endregion


        #region Movement State Functions

        public void UpdateMovementState(bool moving, bool crouching, bool sprinting, float transitionDuration = 0.25f)
        {
            if (dead || IsJumping) return;

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
            TryTransitionAnimation(sprintAnimationHash, transitionDuration);
        }

        #endregion


        public void Jump(float transitionDuration = 0.25f)
        {
            TryTransitionAnimation(jumpAnimationHash, transitionDuration);

            AutoTransition(idleAnimationHash, transitionDuration);
        }


        public void ShakeGooglyEyes(float transitionDuration = 0.25f)
        {
            TryTransitionAnimation(shakeGooglyEyesAnimationHash, transitionDuration, 1, 1);

            AutoTransition(eyesCuriousAnimationHash, transitionDuration, 1, 1);
        }


        public void Die(Vector3 ragdollDirection, Vector3 ragdollImpactPoint, float transitionDuration = 0.25f)
        {
            dead = true;

            ragDollController.StartRagdoll(ragdollDirection, ragdollImpactPoint);
        }


        /// <summary>
        /// Create an auto transition to target animation after current animation finishes playing.
        /// </summary>
        private void AutoTransition(int animationHash, float transitionDuration, float speed = 1, int layer = 0)
        {
            if (autoTransitiosCOs[layer] != null)
            {
                StopCoroutine(autoTransitiosCOs[layer]);
            }
            autoTransitiosCOs[layer] = StartCoroutine(AutoTransitionCoroutine(animationHash, transitionDuration, speed, layer));
        }
        private IEnumerator AutoTransitionCoroutine(int animationHash, float transitionDuration, float speed = 1, int layer = 0)
        {
            yield return null; // Wait 1 frame so animator updates to the new state

            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(layer);
            float remainingTime = (1f - state.normalizedTime) * state.length;

            yield return new WaitForSeconds(remainingTime);

            TryTransitionAnimation(animationHash, transitionDuration, speed, layer);
        }
    }
}