using Unity.Netcode;
using UnityEngine;


public class RagDollController : NetworkBehaviour
{
    [SerializeField] private Rigidbody[] bones;

    [SerializeField] private float ragDollStrength = 10f;
    [SerializeField] private float ragDollImpactStrength = 0.5f;
    [SerializeField] private float ragDollImpactRadius = 2;
    [SerializeField] private float ragDollImpactUpMod = 1;

    [SerializeField] private Collider[] mainColliders;
    private Rigidbody mainRigidbody;


    private void Start()
    {
        mainRigidbody = GetComponent<Rigidbody>();

        int boneCount = bones.Length;
        for (int i = 0; i < boneCount; i++)
        {
            Rigidbody targetBoneRb = bones[i];
            targetBoneRb.isKinematic = true;

            if (targetBoneRb.TryGetComponent(out Collider coll))
            {
                coll.enabled = false;
            }
        }
    }


    #region Start Ragdoll Netcode Logic

    public void StartRagdoll(Vector3 ragdollDirection, Vector3 ragdollImpactPoint)
    {
        Ragdoll(ragdollDirection, ragdollImpactPoint);

        ActivateRagdoll_ServerRPC(ClientManager.LocalClientGameId, ragdollDirection, ragdollImpactPoint);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivateRagdoll_ServerRPC(int fromClientGameId, Vector3 ragdollDirection, Vector3 ragdollImpactPoint)
    {
        ActivateRagdoll_ClientRPC(ragdollDirection, ragdollImpactPoint, GameIdRPCTargets.SendToOppositeClient(fromClientGameId));
    }

    [ClientRpc(RequireOwnership = false)]
    private void ActivateRagdoll_ClientRPC(Vector3 ragdollDirection, Vector3 ragdollImpactPoint, GameIdRPCTargets rpcTargets)
    {
        if (rpcTargets.IsTarget == false) return;

        RecreateRagdollTransforms();
        Ragdoll(ragdollDirection, ragdollImpactPoint);
    }

    #endregion


    private void RecreateRagdollTransforms()
    {

    }

    private void Ragdoll(Vector3 ragdollDirection, Vector3 ragdollImpactPoint)
    {
        int mainColliderCount = mainColliders.Length;
        for (int i = 0; i < mainColliderCount; i++)
        {
            mainColliders[i].enabled = false;
        }
        mainRigidbody.isKinematic = true;


        int boneCount = bones.Length;
        for (int i = 0; i < boneCount; i++)
        {
            Rigidbody targetBoneRb = bones[i];
            targetBoneRb.isKinematic = false;

            if (targetBoneRb.TryGetComponent(out Collider coll))
            {
                coll.enabled = true;
            }

            targetBoneRb.AddForce(ragdollDirection * ragDollStrength, ForceMode.Impulse);
            targetBoneRb.AddExplosionForce(ragDollImpactStrength, ragdollImpactPoint, ragDollImpactRadius, ragDollImpactUpMod, ForceMode.Impulse);
        }
    }


#if UNITY_EDITOR

    [SerializeField] private Transform ragDollImpactPoint;
    [SerializeField] private Vector3 ragDollDirection;


    [ContextMenu("DEBUG Test RagDoll")]
    private void DEBUG_TestRagDoll()
    {
        Ragdoll(ragDollDirection, ragDollImpactPoint.position);
    }

#endif
}
