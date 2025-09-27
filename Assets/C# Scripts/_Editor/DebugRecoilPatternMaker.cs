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

    [SerializeField] GunSO toSaveLoadGunDataObject;

    [SerializeField] private bool updateRecoilVisualsLive;
    [SerializeField] private float shootingSequenceRPM;

    private AudioSource source;

    private const string RecoilSavesPath = "Editor/RecoilPatterns/";



    private void Start()
    {
        source = GetComponent<AudioSource>();
    }

    private void Update()
    {
        int childCount = transform.childCount;

        if (childCount == 0) return;

        Vector3 previousPos = transform.GetChild(0).position;


        for (int i = 1; i < childCount; i++)
        {
            Vector3 newPos = transform.GetChild(i).position;
            Debug.DrawLine(previousPos, newPos, new Color(1, 1, 1, 0.5f));

            previousPos = newPos;
        }
    }

    public void LoadRecoilPatternFromScriptableObject()
    {
        recoilPatternData.recoilPoints = toSaveLoadGunDataObject.GetGunCoreStats().adsRecoilPattern;
    }

    public void SaveRecoilPatternToScriptableObject()
    {
        toSaveLoadGunDataObject.SetGunStatsADSRecoilPattern(recoilPatternData.recoilPoints);
    }


    public async Task LoadRecoilPatternFromFile()
    {
        (bool succes, DebugRecoilPatternData loadedRecoilPattern) = await FileManager.LoadInfoFromEditorAsync<DebugRecoilPatternData>(RecoilSavesPath + recoilPatternData.weaponName + "'s Recoilpattern");

        if (succes)
        {
            recoilPatternData = loadedRecoilPattern;
            DebugLogger.Log("Weapon recoilpattern data loaded from file");
        }
    }

    public async Task SaveRecoilPatternToFile()
    {
        await FileManager.SaveInfoToEditorAsync(recoilPatternData, RecoilSavesPath + recoilPatternData.weaponName + "'s Recoilpattern");
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
        float elapsed = 0;

        float2 cRecoil = float2.zero;

        int recoilPointCount = recoilPatternData.recoilPoints.Length;
        for (int i = 0; i < recoilPointCount; i++)
        {
            yield return new WaitForSeconds(60 / shootingSequenceRPM);

            source.PlayWithPitch(EzRandom.Range(0.95f, 1.05f));

            cRecoil += recoilPatternData.recoilPoints[i];

            GameObject obj = Instantiate(recoilPartPrefab, transform);
            obj.transform.localPosition = new Vector3(cRecoil.x, cRecoil.y, 0);
            obj.transform.localScale = Vector3.one * recoilPartScale;

            elapsed += 60 / shootingSequenceRPM;
        }

        DebugLogger.Log("Shooting sequence took: " + elapsed);
    }


    private void OnValidate()
    {
        if (updateRecoilVisualsLive == false) return;

        UpdateVisualRecoilPattern();
    }
}
#endif