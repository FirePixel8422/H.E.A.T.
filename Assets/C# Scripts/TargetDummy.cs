using Unity.Netcode;
using UnityEngine;


namespace FirePixel.Networking
{
    public class TargetDummy : NetworkBehaviour, IDamagable
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

        [Header("Parent GameObject thats holds the dummys colliders and meshes")]
        [SerializeField] private GameObject dummyCoreObj;
        [SerializeField] private float respawnDelay;



        #region Take Damage, Update Health

        public void DealDamage(float damage, bool headShot, Vector3 hitPoint, Vector3 hitDir, out HitTypeResult hitTypeResult)
        {
            bool dead = RecieveDamage(damage);

            hitTypeResult = IDamagable.CalculateHitType(headShot, dead);

            if (dead)
            {
                OnDeath();
                return;
            }

            SendDamage_ServerRPC(GameIdRPCTargets.SendToOppositeOfLocalClient(), damage, hitTypeResult);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendDamage_ServerRPC(GameIdRPCTargets rpcTargets, float damage, HitTypeResult hitType)
        {
            RecieveDamage_ClientRPC(rpcTargets, damage, hitType);
        }
        [ClientRpc(RequireOwnership = false)]
        private void RecieveDamage_ClientRPC(GameIdRPCTargets rpcTargets, float damage, HitTypeResult hitType)
        {
            if (rpcTargets.IsTarget == false) return;

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

        #endregion


        #region Death

        private void OnDeath()
        {
            Die();
            OnDeath_ServerRPC(GameIdRPCTargets.SendToOppositeOfLocalClient());
        }

        /// <summary>
        /// Notify Server client has died and update game state on server
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void OnDeath_ServerRPC(GameIdRPCTargets rpcTargets)
        {
            OnDeath_ClientRPC(rpcTargets);
        }
        [ClientRpc(RequireOwnership = false)]
        private void OnDeath_ClientRPC(GameIdRPCTargets rpcTargets)
        {
            if (rpcTargets.IsTarget == false) return;

            Die();
        }

        private void Die()
        {
            dummyCoreObj.SetActive(false);

            // Respawn after "respawnDelay" seconds
            Invoke(nameof(Respawn), respawnDelay);
        }

        private void Respawn()
        {
            cHealth = maxHealth;
            dummyCoreObj.SetActive(true);
        }

        #endregion
    }
}