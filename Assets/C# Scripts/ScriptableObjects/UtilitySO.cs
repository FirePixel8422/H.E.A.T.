using UnityEngine;



[CreateAssetMenu(fileName = "UtilitySO", menuName = "ScriptableObjects/Utility")]
public class UtilitySO : ScriptableObject
{
    [SerializeField] private GameObject utilityPrefab;
    public GameObject UtilityPrefab => utilityPrefab;


    [SerializeField] private UtilityStats stats;
    public UtilityStats Stats;
}