using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;


/// <summary>
/// Class to keep track of and destroy spawned bullet holes after a certain time when not in view
/// </summary>
public class BulletHoleManager : MonoBehaviour
{
    public static BulletHoleManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }



    [Header("MAX allowed bullet holes in the scene")]
    [SerializeField] private int bulletHoleCap = 150;

    [Header("How many bullets to instantly destroy when hitting cap, to make space for new ones")]
    [SerializeField] private int onCapReachCleanupCount = 10;

    [Header(">>DEBUG<<, List of all bullet holes in the scene, and lifeTimers")]
    [SerializeField] private List<BulletHoleEntry> bulletHoleEntries = new List<BulletHoleEntry>();


    private void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    private void OnDisable() => UpdateScheduler.UnregisterUpdate(OnUpdate);


    private void Start()
    {
        // Set capacity
        bulletHoleEntries = new List<BulletHoleEntry>(bulletHoleCap);
    }

    public void RegisterBulletHole(GameObject bulletHole, float lifetime)
    {
        DecalProjector bulletRenderer = bulletHole.GetComponent<DecalProjector>();

        // If list is at max capacity, remove the oldest bullet hole
        if (bulletHoleEntries.Count >= bulletHoleCap)
        {
            // Kill all bullet GameObjects
            for (int i = 0; i < onCapReachCleanupCount; i++)
            {
                Destroy(bulletHoleEntries[i].decalProjector.gameObject);
            }

            // Remove all destroyed bullet entries at once
            bulletHoleEntries.RemoveRange(0, onCapReachCleanupCount);
        }

        bulletHoleEntries.Add(new BulletHoleEntry(bulletRenderer, Time.time + lifetime));

#if UNITY_EDITOR
        bulletHole.transform.SetParent(transform);
#endif
    }

    private void OnUpdate()
    {
        int bulletHoleCount = bulletHoleEntries.Count;

        float time = Time.time;
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        Bounds bounds;

        for (int i = 0; i < bulletHoleCount; i++)
        {
            BulletHoleEntry targetBulletHole = bulletHoleEntries[i];

            // Check if bulletHole may expire according to deathTime and check if it is not visible
            if (time >= targetBulletHole.deathTime)
            {
                Vector3 center = targetBulletHole.decalProjector.transform.position;
                bounds = new Bounds(center, Vector3.one * 0.5f);

                // If the bulletHole is not visible, destroy it
                if (GeometryUtility.TestPlanesAABB(planes, bounds) == false)
                {
                    Destroy(targetBulletHole.decalProjector.gameObject);

                    bulletHoleEntries.RemoveAtSwapBack(i);
                    i--;
                    bulletHoleCount--;
                }
            }
        }
    }
}