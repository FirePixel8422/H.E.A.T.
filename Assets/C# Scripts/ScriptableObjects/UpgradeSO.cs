using UnityEngine;



[CreateAssetMenu(fileName = "temp", menuName = "ScriptableObjects/TEMPTESTING MENYNAME")]
public class UpgradeSO : ScriptableObject
{
    [Header("How rare is upgrade")]
    public UpgradeRarity rarity = UpgradeRarity.Common;

    [Header("Can you purchase this upgrade multiple times")]
    public bool stackable;

    public Sprite upgradeSprite;
    public string upgradeName;


    [HideInInspector] public int upgradeId;
    [HideInInspector] public int upgradesLeftId;


    public virtual void ApplyUpgrade(GunManager gunManager, PlayerDataLibrary playerDataLibrary)
    {

    }
}