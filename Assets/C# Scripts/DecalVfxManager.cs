using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;


/// <summary>
/// Class to keep track of and destroy spawned decal like bullet holes after a certain time when not in view
/// </summary>
public class DecalVfxManager : MonoBehaviour
{
    public static DecalVfxManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }



    [SerializeField] private DecalProjector decalPrefab;
    [SerializeField] private Material[] decalMaterials;

    private Queue<DecalProjector> decalPool;


    [Header("MAX allowed bullet holes in the scene")]
    [SerializeField] private int decalCap = 150;

    [Header("How many bullets to instantly destroy when hitting cap, to make space for new ones")]
    [SerializeField] private int onCapReachCleanupCount = 10;

    [Header(">>DEBUG<<, List of all bullet holes in the scene, and lifeTimers")]
    [SerializeField] private List<DecalEntry> decalEntries;

    private NativeArray<Plane> cameraPlanes;
    private Camera mainCam;

    public const float MaxExpectedDecalSize = 0.25f;



    public void Init(Camera targetCam)
    {
        // Set capacity and Camera
        decalPool = new Queue<DecalProjector>(decalCap);
        for (int i = 0; i < decalCap; i++)
        {
            DecalProjector decalInstance = Instantiate(decalPrefab);

            decalInstance.gameObject.SetActive(false);
            DontDestroyOnLoad(decalInstance.gameObject);

            decalPool.Enqueue(decalInstance);

#if UNITY_EDITOR
            decalInstance.transform.SetParent(transform);
#endif
        }

        decalEntries = new List<DecalEntry>(decalCap);

        cameraPlanes = new NativeArray<Plane>(6, Allocator.Persistent);
        mainCam = targetCam;

        UpdateScheduler.RegisterUpdate(OnUpdate);
    }

    /// <summary>
    /// Create decal like bullet holes based on SurfaceType with a certain lifetime.
    /// </summary>
    public void RegisterDecal(BulletHoleMessage[] messages)
    {
        int messagesCount = messages.Length;

        for (int i = 0; i < messagesCount; i++)
        {
            BulletHoleMessage cMessage = messages[i];

            // If list is at max capacity, remove the oldest bullet hole
            if (decalEntries.Count >= decalCap)
            {
                // Kill all bullet GameObjects
                for (int i2 = 0; i2 < onCapReachCleanupCount; i2++)
                {
                    // Add decal to pool, in the method the gameObject is disabled
                    AddDecalToPool(decalEntries[i2].decalProjector);
                }

                // Remove all destroyed bullet entries at once
                decalEntries.RemoveRange(0, onCapReachCleanupCount);
            }
            // Add Decal
            DecalProjector decalRenderer = GetDecalFromPool(cMessage.pos, cMessage.rot, Vector3.one * cMessage.scale, (int)cMessage.hitSurfaceType);

            decalEntries.Add(new DecalEntry(decalRenderer, Time.time + cMessage.lifetime));

#if UNITY_EDITOR
            decalRenderer.transform.SetParent(transform);
#endif
        }
    }

    private void OnUpdate()
    {
        int decalCount = decalEntries.Count;

        // If there are no decals, skip 
        if (decalCount == 0 || mainCam == null) return;

        float time = Time.time;

        CullingUtility.ExtractFrustumPlanes(ref cameraPlanes, mainCam.worldToCameraMatrix, mainCam.projectionMatrix);

        DecalProjector targetDecalProjector;

        float3 boundsCenter;
        float3 boundsExtents = new float3(MaxExpectedDecalSize * 0.5f); // Half size to get extents

        for (int i = 0; i < decalCount; i++)
        {
            targetDecalProjector = decalEntries[i].decalProjector;

            boundsCenter = targetDecalProjector.transform.position;

            // If decal is in camera frustum eg, visible
            if (CullingUtility.TestPlanesAABB(cameraPlanes, boundsCenter, boundsExtents))
            {
                // If decal is in frustum and disabled, enable it
                targetDecalProjector.SetActiveStateSmart(true);
            }
            else
            {
                // If decal is allowed to expire, pool it and remove it
                if (time >= decalEntries[i].deathTime)
                {
                    AddDecalToPool(targetDecalProjector);
                    decalEntries.RemoveAtSwapBack(i);
                    i--;
                    decalCount--;
                }
                // If decal is still alive but out of view, disable its renderer
                else
                {
                    targetDecalProjector.SetActiveStateSmart(false);
                }
            }
        }
    }

    private DecalProjector GetDecalFromPool(Vector3 pos, Quaternion rot, Vector3 scale, int materialId)
    {
        if (decalPool.Count > 0)
        {
            DecalProjector pooled = decalPool.Dequeue();

            pooled.transform.SetLocalPositionAndRotation(pos, rot);
            pooled.transform.localScale = scale;

            //set target decal material
            pooled.material = decalMaterials[materialId];

            pooled.gameObject.SetActive(true);

            return pooled;
        }

        return null;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddDecalToPool(DecalProjector decal)
    {
        decal.gameObject.SetActive(false);
        decalPool.Enqueue(decal);
    }


    private void OnDestroy()
    {
        UpdateScheduler.UnregisterUpdate(OnUpdate);
        cameraPlanes.DisposeIfCreated();
    }
}