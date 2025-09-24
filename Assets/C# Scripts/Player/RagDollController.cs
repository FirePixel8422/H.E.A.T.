using Unity.Netcode;
using UnityEngine;
using FirePixel.Networking;
using System.Collections.Generic;


public class RagDollController : NetworkBehaviour
{
    [SerializeField] private Transform playerBonesRoot;

    [SerializeField] private float ragDollStrength = 10f;
    [SerializeField] private float ragDollImpactStrength = 0.5f;
    [SerializeField] private float ragDollImpactRadius = 2;
    [SerializeField] private float ragDollImpactUpMod = 1;

    [SerializeField] private float ragdollLifeTime = 15;

    private Rigidbody mainRigidbody;
    private Rigidbody[] bones;
    private Collider[] mainColliders;


    private void Start()
    {
        // Detach regdoll from player
        transform.parent = null;

        mainRigidbody = GetComponent<Rigidbody>();

        bones = GetComponentsInChildren<Rigidbody>(true);

        int boneCount = bones.Length;

        List<Collider> mainColls = new List<Collider>(boneCount);

        for (int i = 0; i < boneCount; i++)
        {
            Rigidbody targetBoneRb = bones[i];
            targetBoneRb.isKinematic = true;

            foreach(Collider coll in targetBoneRb.GetComponents<Collider>())
            {
                mainColls.Add(coll);
                coll.enabled = false;
            }
        }

        mainColliders = mainColls.ToArray();
    }


    #region Start Ragdoll Netcode Logic

    public void StartRagdoll(Vector3 ragdollDirection, Vector3 ragdollImpactPoint)
    {
        return;

        RecreateRagdollTransforms();
        Ragdoll(ragdollDirection, ragdollImpactPoint);

        ActivateRagdoll_ServerRPC(ClientManager.LocalClientGameId, ragdollDirection, ragdollImpactPoint);
    }

    [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void ActivateRagdoll_ServerRPC(int fromClientGameId, Vector3 ragdollDirection, Vector3 ragdollImpactPoint)
    {
        ActivateRagdoll_ClientRPC(ragdollDirection, ragdollImpactPoint, GameIdRPCTargets.SendToOppositeClient(fromClientGameId));
    }

    [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void ActivateRagdoll_ClientRPC(Vector3 ragdollDirection, Vector3 ragdollImpactPoint, GameIdRPCTargets rpcTargets)
    {
        if (rpcTargets.IsTarget == false) return;

        RecreateRagdollTransforms();
        Ragdoll(ragdollDirection, ragdollImpactPoint);
    }

    #endregion


    private void RecreateRagdollTransforms()
    {
        List<Transform> children = playerBonesRoot.GetAllChildren();
        int childCount = children.Count;

        List<Matrix4x4> matrices = new List<Matrix4x4>(childCount);

        for (int i = 0; i < childCount; i++)
        {
            matrices.Add(children[i].localToWorldMatrix);
        }
        //Destroy(playerBonesRoot.gameObject);


        children = transform.GetAllChildren();
        childCount = children.Count;

        for (int i = 0; i < childCount; i++)
        {
            TransformUtility.SetTransformFromMatrix(children[i], matrices[i]);
        }
    }

    private void Ragdoll(Vector3 ragdollDirection, Vector3 ragdollImpactPoint)
    {
        gameObject.SetActive(true);

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

        Destroy(gameObject, ragdollLifeTime);
    }


#if UNITY_EDITOR

    [Header("DEBUG")]
    [SerializeField] private Transform ragDollImpactPoint;
    [SerializeField] private Vector3 ragDollDirection;


    [ContextMenu("DEBUG Test RagDoll")]
    private void DEBUG_TestRagDoll()
    {
        if (Application.isPlaying == false) return;

        RecreateRagdollTransforms();
        Ragdoll(ragDollDirection, ragDollImpactPoint.position);
    }

#endif
}
