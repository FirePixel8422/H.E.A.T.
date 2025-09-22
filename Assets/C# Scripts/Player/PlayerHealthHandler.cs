using FirePixel.Networking;
using System;
using Unity.Netcode;
using UnityEngine;


public class PlayerHealthHandler : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 250;
    [SerializeField] private float cHealth = 250;

    private NetworkStateMachine stateMachine;

    public Action OnDeathEvent { get; private set; }


    private void Start()
    {
        stateMachine = GetComponent<NetworkStateMachine>();
    }


    #region Take Damage, Update Health and Death

    /// <summary>
    /// Take damage locally and send to other clients. of health hits 0, die and send to other clients.
    /// </summary>
    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitDir)
    {
        bool dead = RecieveDamage(damage);

        if (dead)
        {
            OnDeath(hitPoint, hitDir);
            return;
        }

        SendDamage_ServerRPC(GameIdRPCTargets.SendToOppositeOfLocalClient(), damage);
    }

    [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void SendDamage_ServerRPC(GameIdRPCTargets rpcTargets, float damage)
    {
        RecieveDamage_ClientRPC(rpcTargets, damage);
    }    
    [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void RecieveDamage_ClientRPC(GameIdRPCTargets rpcTargets, float damage)
    {
        if (rpcTargets.IsTarget) return;

        RecieveDamage(damage);
    }

    private bool RecieveDamage(float damage)
    {
        cHealth -= damage;

        // If player health falls below 0, Call OnDeath
        if (cHealth <= 0)
        {
            return true;
        }

        return false;
    }

    // Flag state machine as dead and notify other clients
    private void OnDeath(Vector3 hitPoint, Vector3 hitDir)
    {
        stateMachine.Die(hitDir, hitPoint, 0.25f);

        Die();
        OnDeath_ServerRPC(GameIdRPCTargets.SendToOppositeOfLocalClient());
    }

    /// <summary>
    /// Notify Server client has died and update game state on server
    /// </summary>
    [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void OnDeath_ServerRPC(GameIdRPCTargets rpcTargets)
    {
        OnDeathEvent?.Invoke();

        OnDeath_ClientRPC(rpcTargets);
    }
    [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void OnDeath_ClientRPC(GameIdRPCTargets rpcTargets)
    {
        if (rpcTargets.IsTarget) return;

        Die();
    }

    private void Die()
    {

    }

    #endregion
}