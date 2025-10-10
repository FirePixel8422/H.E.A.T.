using UnityEngine;


public class UtilityManager : MonoBehaviour
{
    public static UtilityManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private UtilitySO[] baseUtiliies;

    public int UtilityCount => baseUtiliies.Length;

    private int currentUtilityId;



    /// <summary>
    /// Swap gun and get baseGunstats by gunId.
    /// </summary>
    public void SwapUtility(Transform handTransform, int utilityId)
    {
        currentUtilityId = utilityId;

        Destroy(handTransform.GetChild(0).gameObject);

        Instantiate(baseUtiliies[utilityId].UtilityPrefab, handTransform);
    }


    private void OnDestroy()
    {

    }
}
