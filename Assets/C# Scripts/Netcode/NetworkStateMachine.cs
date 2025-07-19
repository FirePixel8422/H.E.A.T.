using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class NetworkStateMachine : NetworkBehaviour
{
    private Animator anim;
    private RagDollController ragDollController;


    #region animationStrings

    [Header("Start Animation")]
    [SerializeField] private string currentAnimation = "Idle";


    [Header("Animation Names")]

    [SerializeField] private string idleAnimation = "Idle";

    [SerializeField] private string crouchAnimation = "Crouch";
    [SerializeField] private string crouchWalkAnimation = "CrouchWalk";

    [SerializeField] private string walkAnimation = "Walk";
    [SerializeField] private string sprintAnimation = "Sprint";

    [SerializeField] private string hurtAnimation = "Hurt";

    #endregion


    [SerializeField] private bool dead;




    private void Start()
    {
        anim = GetComponent<Animator>();
        ragDollController = GetComponent<RagDollController>();
    }




    #region Change/Transition Animation + Server Sync Functions

    /// <returns>true if the animation has changed, false otherwise</returns>
    private bool TryTransitionAnimation(string animationString, float transitionDuration = 0.25f, float speed = 1, int layer = 0)
    {
        //if the new animation is the same as current, return false
        if (currentAnimation == animationString) return false;


        SyncAnimation_ServerRPC(ClientManager.LocalClientGameId, animationString, transitionDuration, speed, layer);

        currentAnimation = animationString;

        anim.speed = speed;
        anim.CrossFade(animationString, transitionDuration, layer);

        return true;
    }

    /// <summary>
    /// Sent Animation Data trough server, back to all clients except sender.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void SyncAnimation_ServerRPC(int fromClientGameId, string animationString, float transitionDuration, float speed = 1, int layer = 0)
    {
        SyncAnimation_ClientRPC(animationString, transitionDuration, speed, layer, GameIdRPCTargets.SendToOppositeClient(fromClientGameId));
    }

    [ClientRpc(RequireOwnership = false)]
    private void SyncAnimation_ClientRPC(string animationString, float transitionDuration, float speed = 1, int layer = 0, GameIdRPCTargets rpcTargets = default)
    {
        if (rpcTargets.IsTarget == false) return;

        anim.speed = speed;
        anim.CrossFade(animationString, transitionDuration, layer);
    }

    #endregion



    public void Idle(float transitionDuration = 0.25f)
    {
        if (dead) return;

        TryTransitionAnimation(idleAnimation, transitionDuration);
    }


    public void Crouch(float transitionDuration = 0.25f)
    {
        if (dead) return;

        TryTransitionAnimation(crouchAnimation, transitionDuration);
    }
    public void CrouchWalk(float transitionDuration = 0.25f)
    {
        if (dead) return;

        TryTransitionAnimation(crouchWalkAnimation, transitionDuration);
    }


    public void Walk(float transitionDuration = 0.25f)
    {
        if (dead) return;

        TryTransitionAnimation(walkAnimation, transitionDuration);
    }
    public void Sprint(float transitionDuration = 0.25f)
    {
        if (dead) return;

        TryTransitionAnimation(sprintAnimation, transitionDuration);
    }


    public void GetHurt(float transitionDuration = 0.25f)
    {
        if (dead) return;

        TryTransitionAnimation(hurtAnimation, transitionDuration);

        StopAllCoroutines();
        StartCoroutine(AutoTransition(idleAnimation, transitionDuration));
    }
    public void Die(Vector3 ragdollDirection, Vector3 ragdollImpactPoint, float transitionDuration = 0.25f)
    {
        dead = true;

        ragDollController.StartRagdoll(ragdollDirection, ragdollImpactPoint);
    }



    private IEnumerator AutoTransition(string animationString, float transitionDuration)
    {
        float clipTime = anim.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(clipTime);

        TryTransitionAnimation(animationString, transitionDuration);
    }
}