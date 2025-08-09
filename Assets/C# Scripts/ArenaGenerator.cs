using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;



public class ArenaGenerator : NetworkBehaviour
{
    [SerializeField] private ArenaPart[] parts;
    [SerializeField] private int spawnAmount;

    [SerializeField] private Vector3 bounds;


    [SerializeField] private int playerReadyCount = 0;

    public override void OnNetworkSpawn()
    {
        PlayerReady_ServerRPC();
    }

    [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void PlayerReady_ServerRPC()
    {
        playerReadyCount += 1;

        if (playerReadyCount == GlobalGameSettings.MaxPlayers)
        {
            GenerateArena_ClientRPC(EzRandom.Seed());
        }
    }

    [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void GenerateArena_ClientRPC(uint randomSeed)
    {
        Unity.Mathematics.Random random = new Unity.Mathematics.Random(randomSeed);

        int partCount = parts.Length;
        float totalWeight = 0;
        for (int i = 0; i < partCount; i++)
        {
            totalWeight += parts[i].weight;
        }

        float r = 0;
        for (int i = 0; i < spawnAmount; i++)
        {
            r = random.NextFloat(0, totalWeight);

            for (int i2 = 0; i2 < partCount; i2++)
            {
                if (r > parts[i2].weight)
                {
                    r -= parts[i2].weight;

                    continue;
                }
                else
                {
                    Vector3 randomOffset = new Vector3(random.NextFloat(-bounds.x * 0.5f, bounds.x * 0.5f), 0, random.NextFloat(-bounds.z * 0.5f, bounds.z * 0.5f));

                    Instantiate(parts[i2].prefab, transform.position + randomOffset, Quaternion.Euler(0, random.NextFloat(-90, 270), 0));

                    break;
                }
            }
        }
    }



    [System.Serializable]
    private class ArenaPart
    {
        public GameObject prefab;
        public float weight;
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.position + Vector3.up * bounds.y * 0.5f, bounds);
    }


    [ContextMenu("DEBUG Generate")]
    private void DEBUG_Generate()
    {
        GenerateArena_ClientRPC(EzRandom.Seed());
    }
#endif
}
