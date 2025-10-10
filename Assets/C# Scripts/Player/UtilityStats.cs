using UnityEngine;



[System.Serializable]
public class UtilityStats
{
    public GameObject utilityPrefab;

    [SerializeField] private int maxCharges = 2;
    private int chargesLeft;

    public bool IsUsable => chargesLeft > 0;

    public virtual void Use()
    {
        chargesLeft -= 1;
    }
}