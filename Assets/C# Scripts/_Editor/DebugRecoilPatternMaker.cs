#if UNITY_EDITOR
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;


public class DebugRecoilPatternMaker : MonoBehaviour
{
    [SerializeField] private GameObject recoilPartPrefab;
    [SerializeField] private float recoilPartScale = 1;

    [SerializeField] private DebugRecoilPatternData recoilPatternData;

    [SerializeField] GunStatsSO toSaveLoadGunDataObject;

    [SerializeField] private bool updateRecoilVisualsLive;
    [Range(0.05f, 2)]
    [SerializeField] private float shootingSequenceInterval;

    private AudioSource source;

    private const string RecoilSavesPath = "Editor/RecoilPatterns/";
    public const string RecoilPatternParentName = "RecoilPatternVisuals >>DEBUG<<";



    private void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public async Task LoadRecoilPatternFromScriptableObject()
    {

    }

    public async Task SaveRecoilPatternToScriptableObject()
    {

    }


    public async Task LoadRecoilPatternFromFile()
    {
        (bool succes, DebugRecoilPatternData loadedRecoilPattern) = await FileManager.LoadInfo<DebugRecoilPatternData>(RecoilSavesPath + recoilPatternData.weaponName + "'s Recoilpattern");

        if (succes)
        {
            recoilPatternData = loadedRecoilPattern;
            DebugLogger.Log("Weapon recoilpattern data loaded from file");
        }
        else
        {
            DebugLogger.LogWarning("Weapon recoilpattern data not found");
        }
    }

    public async Task SaveRecoilPatternToFile()
    {
        await FileManager.SaveInfo(recoilPatternData, RecoilSavesPath + recoilPatternData.weaponName + "'s Recoilpattern");
    }

    public void LoadRecoilPatternFromVisuals()
    {
        float2 lastBulletPos = float2.zero;

        int childCount = transform.childCount;
        recoilPatternData.recoilPoints = new float2[childCount];

        for (int i = 0; i < childCount; i++)
        {
            recoilPatternData.recoilPoints[i] = new float2(transform.GetChild(i).localPosition.x - lastBulletPos.x, transform.GetChild(i).localPosition.y - lastBulletPos.y);
            lastBulletPos = new float2(transform.GetChild(i).localPosition.x, transform.GetChild(i).localPosition.y);
        }
    }


    public void ClearVisualRecoilPattern()
    {
        StopAllCoroutines();

        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void UpdateVisualRecoilPattern()
    {
        float2 cRecoil = float2.zero;

        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            cRecoil += recoilPatternData.recoilPoints[i];

            transform.GetChild(i).localPosition = new Vector3(cRecoil.x, cRecoil.y, 0);
            transform.GetChild(i).localScale = Vector3.one * recoilPartScale;
        }
    }

    public void StartShootingSequence()
    {
        ClearVisualRecoilPattern();

        StartCoroutine(ShootingSequence());
    }
    private IEnumerator ShootingSequence()
    {
        float2 cRecoil = float2.zero;

        int recoilPointCount = recoilPatternData.recoilPoints.Length;
        for (int i = 0; i < recoilPointCount; i++)
        {
            yield return new WaitForSeconds(shootingSequenceInterval);

            source.PlayWithPitch(EzRandom.Range(0.95f, 1.05f));

            cRecoil += recoilPatternData.recoilPoints[i];

            GameObject obj = Instantiate(recoilPartPrefab, transform);
            obj.transform.localPosition = new Vector3(cRecoil.x, cRecoil.y, 0);
            obj.transform.localScale = Vector3.one * recoilPartScale;
        }
    }


    private void OnValidate()
    {
        if (updateRecoilVisualsLive == false) return;

        UpdateVisualRecoilPattern();
    }
}
#endif