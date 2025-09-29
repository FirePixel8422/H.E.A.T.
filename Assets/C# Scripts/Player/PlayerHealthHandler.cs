using FirePixel.Networking;
using System;
using Unity.Netcode;
using UnityEngine;


public class PlayerHealthHandler : NetworkBehaviour, IDamagable
{
    [SerializeField] private float maxHealth = 250;
    [SerializeField] private float cHealth = 250;

    private NetworkStateMachine stateMachine;

    public Action OnDeathEvent { get; private set; }
    public static Action<float, HitTypeResult> OnDamageRecieved { get; private set; }
    public static Action<float, HitTypeResult> OnDamageDealt { get; private set; }


    public override void OnNetworkSpawn()
    {
        stateMachine = GetComponent<NetworkStateMachine>();
    }


    #region Take Damage, Update Health and Death

    /// <summary>
    /// Make this player take damage locally and send to other clients. if health hits 0, die and send to other clients.
    /// </summary>
    public void DealDamage(float damage, bool headShot, Vector3 hitPoint, Vector3 hitDir)
    {
        bool dead = RecieveDamage(damage);

        HitTypeResult hitType = CalculateHitType(headShot, dead);
        
        OnDamageDealt?.Invoke(damage, hitType);

        if (dead)
        {
            OnDeath(hitPoint, hitDir);
            return;
        }

        SendDamage_ServerRPC(GameIdRPCTargets.SendToOppositeOfLocalClient(), damage, hitType);
    }

    private HitTypeResult CalculateHitType(bool headShot, bool dead)
    {
        if (headShot)
        {
            return dead ? HitTypeResult.HeadShotKill : HitTypeResult.HeadShot;
        }
        else
        {
            return dead ? HitTypeResult.NormalKill : HitTypeResult.Normal;
        }
    }

    [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void SendDamage_ServerRPC(GameIdRPCTargets rpcTargets, float damage, HitTypeResult hitType)
    {
        RecieveDamage_ClientRPC(rpcTargets, damage, hitType);
    }
    [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void RecieveDamage_ClientRPC(GameIdRPCTargets rpcTargets, float damage, HitTypeResult hitType)
    {
        if (rpcTargets.IsTarget == false) return;

        RecieveDamage(damage);

        OnDamageRecieved?.Invoke(damage, hitType);
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
        OnDeath_ServerRPC(GameIdRPCTargets.SendToOppositeOfLocalClient(), OwnerClientId);
    }

    /// <summary>
    /// Notify Server client has died and update game state on server
    /// </summary>
    [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void OnDeath_ServerRPC(GameIdRPCTargets rpcTargets, ulong deadClientNetworkId)
    {
        OnDeathEvent?.Invoke();

        OnDeath_ClientRPC(rpcTargets);

        NetworkObject.Despawn(gameObject);

        this.FindObjectOfType<PlayerManager>().SpawnPlayer_ServerRPC(deadClientNetworkId);
    }
    [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void OnDeath_ClientRPC(GameIdRPCTargets rpcTargets)
    {
        if (rpcTargets.IsTarget == false) return;

        Die();
    }

    private void Die()
    {
        
    }

    #endregion


    public override void OnDestroy()
    {
        base.OnDestroy();

        OnDamageRecieved = null;
        OnDamageDealt = null;
    }
}