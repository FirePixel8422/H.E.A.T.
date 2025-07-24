using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


namespace FirePixel.Networking
{
    public class NetworkPrefabsHandler : MonoBehaviour
    {
        [SerializeField] private NetworkPrefabsList[] networkPrefabsList;


        private void Start()
        {
            HashSet<GameObject> filterdPrefabSet = GetFilterdPrefabList();
            SetupNetworkPrefabs(filterdPrefabSet);
        }

        private HashSet<GameObject> GetFilterdPrefabList()
        {
            HashSet<GameObject> filterdPrefabSet = new HashSet<GameObject>();

            for (int i = 0; i < networkPrefabsList.Length; i++)
            {
                int prefabCount = networkPrefabsList[i].PrefabList.Count;

                for (int i2 = 0; i2 < prefabCount; i2++)
                {
                    filterdPrefabSet.Add(networkPrefabsList[i].PrefabList[i2].Prefab);
                }
            }

            return filterdPrefabSet;
        }

        private void SetupNetworkPrefabs(HashSet<GameObject> filterdPrefabSet)
        {
            foreach (GameObject prefab in filterdPrefabSet)
            {
                NetworkPrefab networkPrefab = new NetworkPrefab
                {
                    Prefab = prefab
                };

                NetworkManager.Singleton.NetworkConfig.Prefabs.Add(networkPrefab);
            }

            Destroy(this);
        }
    }
}