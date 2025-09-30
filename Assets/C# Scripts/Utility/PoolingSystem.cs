using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


[System.Serializable]
public class PoolingSystem<T> where T : Component
{
    [SerializeField] private T pooledPrefab;

    [SerializeField] private int startCapacity;
    [SerializeField] private bool dynamicRefill;

    private List<T> allObjects;
    private Queue<T> availableObjects;

#if UNITY_EDITOR
    private Transform objHolderParent;
#endif


    public PoolingSystem(T pooledPrefab, int startCapacity, bool dynamicRefill)
    {
        this.pooledPrefab = pooledPrefab;
        this.startCapacity = startCapacity;
        this.dynamicRefill = dynamicRefill;

        if (pooledPrefab == null)
        {
            DebugLogger.LogError("Pooling System of object: '" + pooledPrefab.name + "' pooledPrefab is null!!");
            return;
        }

        Initialize();
    }
    public void Initialize()
    {
        allObjects = new List<T>(startCapacity);
        availableObjects = new Queue<T>(startCapacity);

#if UNITY_EDITOR
        objHolderParent = new GameObject("DEBUG_PoolingSystemHolder: " + pooledPrefab.name).transform;
#endif

        for (int i = 0; i < startCapacity; i++)
        {
            T obj = CreateNewObject();

            allObjects.Add(obj);
            availableObjects.Enqueue(obj);
        }
    }

    public T GetPulledObject(Vector3 pos, Transform parent = null)
    {
        return GetPulledObject(pos, Quaternion.identity, Vector3.one, parent);
    }

    public T GetPulledObject(Vector3 pos, Quaternion rot, Transform parent = null)
    {
        return GetPulledObject(pos, rot, Vector3.one, parent);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetPulledObject(Vector3 pos, Quaternion rot, Vector3 scale, Transform parent = null)
    {
        if (availableObjects.Count > 0)
        {
            T obj = availableObjects.Dequeue();

            return SetupObject(obj, pos, rot, scale, parent);
        }

        if (dynamicRefill)
        {
            T obj = CreateNewObject();

            allObjects.Add(obj);

            return SetupObject(obj, pos, rot, scale, parent);
        }
        else
        {
            DebugLogger.LogError("Pooling System of object: '" + pooledPrefab.name + "' exceeded limits, resizing is disabled, FIX THIS");
        }

        return null;
    }

    public void ReleasePooledObject(T obj, bool clearParent = false)
    {
        obj.gameObject.SetActive(false);
        if (clearParent)
        {
#if UNITY_EDITOR
            obj.transform.SetParent(objHolderParent);
#else
            obj.transform.parent = null;
#endif
        }

        availableObjects.Enqueue(obj);
    }



    /// <summary>
    /// Create (Instantiate) a new Object <typeparamref name="T"/> and set it inactive
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T CreateNewObject()
    {
        T obj = Object.Instantiate(pooledPrefab);
        obj.gameObject.SetActive(false);

#if UNITY_EDITOR
        obj.transform.SetParent(objHolderParent);
#endif

        return obj;
    }

    /// <summary>
    /// Set object <typeparamref name="T"/> active set position rotation and optional parent
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T SetupObject(T obj, Vector3 pos, Quaternion rot, Vector3 scale, Transform parent = null)
    {
        obj.gameObject.SetActive(true);
        obj.transform.SetPositionAndRotation(pos, rot);
        obj.transform.localScale = scale;
        if (parent != null)
        {
            obj.transform.SetParent(parent, true);
        }
        return obj;
    }
}
