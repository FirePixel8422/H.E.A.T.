using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;



namespace FirePixel.Networking
{
    public class UpgradeManager : NetworkBehaviour
    {
        [SerializeField] private UpgradeSO[] upgrades;

        [SerializeField] private GameObject upgradeUIParent;
        [SerializeField] private UpgradeUISlot[] uiSlots;


        [SerializeField] private List<UpgradeSO> upgradesLeft;
        private int totalWeightLeft;

        [SerializeField] private int2[] upgradesIds;


        private void Awake()
        {
            // Set UpgradeSO ids and calculate totalWeight
            int upgradeCount = upgrades.Length;
            for (int i = 0; i < upgradeCount; i++)
            {
                upgrades[i].upgradeId = i;
                upgrades[i].upgradesLeftId = i;
                totalWeightLeft += (int)upgrades[i].rarity;
            }
            
            upgradesLeft = new List<UpgradeSO>(upgrades);


            //TEMP
            //TEMP
            //TEMP
            //TEMP
            Invoke(nameof(CreateUpgradeUI), 0.5f);
        }


        private void Update()
        {
            upgradesIds = new int2[upgrades.Length];
            for (int i = 0; i < upgrades.Length; i++)
            {
                upgradesIds[i] = new int2(upgrades[i].upgradeId, upgrades[i].upgradesLeftId);
            }
        }

        public void CreateUpgradeUI()
        {
            Cursor.lockState = CursorLockMode.None;
            upgradeUIParent.SetActive(true);

            UpgradeSO[] upgrades = GetRandomUpgrades(GlobalGameData.UpgradeCount);
            
            // Enable and setup up UI slots for found upgrades
            int upgradeCount = upgrades.Length;
            for (int i = 0; i < upgradeCount; i++)
            {
                uiSlots[i].SetActiveAndUpdateUI(upgrades[i].upgradeSprite, upgrades[i].upgradeName);

                int tempIndex = i;
                uiSlots[i].ConfirmButton.onClick.RemoveAllListeners();
                uiSlots[i].ConfirmButton.onClick.AddListener(() => TakeUpgrade(upgrades[tempIndex].upgradeId));
            }

            //If there are too little upgrades, disable unused UI slots
            for (int i = 0; i < GlobalGameData.UpgradeCount - upgradeCount; i++)
            {
                uiSlots[i].SetActive(false);
            }
        }


        /// <summary>
        /// Get up to <paramref name="upgradeCount"/> random upgrades based on upgrades left in <see cref="upgradesLeft"/> based on their <see cref="UpgradeRarity"/>
        /// </summary>
        private UpgradeSO[] GetRandomUpgrades(int upgradeCount)
        {
            int upgradesLeftCount = upgradesLeft.Count;

            // Clamp in case of little upgrades left
            upgradeCount = Mathf.Min(upgradesLeftCount, upgradeCount);

            UpgradeSO[] chosenUpgrades = new UpgradeSO[upgradeCount];


            for (int i = 0; i < upgradeCount; i++)
            {
                int rWeight = EzRandom.Range(0, totalWeightLeft);

                for (int i2 = 0; i2 < upgradesLeftCount; i2++)
                {
                    int rarity = (int)upgradesLeft[i2].rarity;
                    // If rolled random number is still more then current to check upgrade, skip it
                    if (rWeight > rarity)
                    {
                        rWeight -= rarity;
                        continue;
                    }
                    else
                    {
                        // Select Upgrade
                        chosenUpgrades[i] = upgradesLeft[i2];

                        // Remove Upgrade from pool temporarly if its not stackable, also remove weight from totalWeightLeft
                        if (upgradesLeft[i2].stackable == false)
                        {
                            int targetUpgradeLeftId = upgradesLeft[i2].upgradeId;
                            int bottomUpgradeLeftId = upgradesLeft[^1].upgradeId;

                            upgrades[targetUpgradeLeftId].upgradesLeftId = upgradesLeft[^1].upgradesLeftId;
                            upgrades[bottomUpgradeLeftId].upgradesLeftId = upgradesLeft[i2].upgradesLeftId;

                            upgradesLeft.RemoveAtSwapBack(i2);
                            upgradesLeftCount -= 1;
                            totalWeightLeft -= rarity;
                        }
                        break;
                    }
                }
            }

            // Re Add Temporarly removed upgrades if they were stackable
            for (int i = 0; i < upgradeCount; i++)
            {
                DebugLogger.Log("Random Upgrades: " + chosenUpgrades[i].upgradeId);

                if (chosenUpgrades[i].stackable == false)
                {
                    UpgradeSO targetUpgrade = chosenUpgrades[i];

                    upgradesLeft.Add(targetUpgrade);    
                    totalWeightLeft += (int)targetUpgrade.rarity;
                }
            }

            return chosenUpgrades;
        }



        public void TakeUpgrade(int upgradeId)
        {
            // Disable Upgrade Screen
            Cursor.lockState = CursorLockMode.Locked;
            upgradeUIParent.SetActive(false);

            int upgradesLeftId = upgrades[upgradeId].upgradesLeftId;

            DebugLogger.Log("Toook upgrade: " + upgradesLeftId);

            // If Upgrade was non stackable, remove it
            if (upgradesLeft[upgradesLeftId].stackable == false)
            {
                int bottomUpgradeLeftId = upgradesLeft[^1].upgradeId;
                int targetUpgradeLeftId = upgradesLeft[upgradesLeftId].upgradeId;

                upgrades[targetUpgradeLeftId].upgradesLeftId = upgradesLeft[^1].upgradesLeftId;
                upgrades[bottomUpgradeLeftId].upgradesLeftId = upgradesLeft[upgradesLeftId].upgradesLeftId;

                totalWeightLeft -= (int)upgradesLeft[upgradesLeftId].rarity;
                upgradesLeft.RemoveAtSwapBack(upgradesLeftId);
            }

            TakeUpgrade_ServerRPC(upgradeId);



            //TEMP
            //TEMP
            //TEMP
            //TEMP
            Invoke(nameof(CreateUpgradeUI), 0.5f);
        }

        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void TakeUpgrade_ServerRPC(int upgradeId)
        {
            TakeUpgrade_ClientRPC(upgradeId);
        }

        [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void TakeUpgrade_ClientRPC(int upgradeId)
        {

        }
    }
}