using FirePixel.Networking;
using Unity.Netcode;
using UnityEngine;


public class PlayerHealthHandler : NetworkBehaviour, IDamagable
{
    [SerializeField] private float maxHealth = 250;
    [SerializeField] private float cHealth = 250;

    public float MaxHealth
    {
        get => maxHealth;
        set => maxHealth = value;
    }
    public float CurrentHealth
    {
        get => cHealth;
        set => cHealth = value;
    }

    private NetworkStateMachine stateMachine;
    /// <summary>
    /// The Local players hudHandler
    /// </summary>
    private PlayerHUDHandler hudHandler;


    private void Awake()
    {
        stateMachine = GetComponent<NetworkStateMachine>();
        hudHandler = GetComponent<PlayerHUDHandler>();
    }

    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
    }


    #region Take Damage, Update Health

    /// <summary>
    /// Make this player take damage locally and send to other clients. if health hits 0, die and send to other clients.
    /// </summary>
    public void DealDamage(float damage, bool headShot, Vector3 hitPoint, Vector3 hitDir, out HitTypeResult hitTypeResult)
    {
        bool dead = RecieveDamage(damage);

        hitTypeResult = IDamagable.CalculateHitType(headShot, dead);

        if (dead)
        {
            OnDeath(hitPoint, hitDir);
            return;
        }

        SendDamage_ServerRPC(GameIdRPCTargets.SendToOppositeOfLocalClient(), damage, hitTypeResult);
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

        hudHandler.OnDamageRecieved(damage / maxHealth, hitType);
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

    #endregion


    #region Death

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
}