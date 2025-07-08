using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


public class PoolingSystem
{
    [SerializeField] private GameObject pooledPrefab;

    [SerializeField] private int startCapacity;
    [SerializeField] private bool dynamicRefill;

    private List<GameObject> pooledList = new List<GameObject>();


    public PoolingSystem(int startCapacity, bool dynamicRefill)
    {
        this.startCapacity = startCapacity;
        this.dynamicRefill = dynamicRefill;
    }


    public void Initialize()
    {
        pooledList = new List<GameObject>();
        for (int i = 0; i < startCapacity; i++)
        {
            GameObject pooledObject = GameObject.Instantiate(pooledPrefab, Vector3.zero, Quaternion.identity);
            pooledObject.SetActive(false);

            pooledList.Add(pooledObject);
        }
    }

    public GameObject GetPulledObj(int index, Vector3 pos, Quaternion rot)
    {
        foreach (GameObject obj in pooledList)
        {
            if (obj.activeInHierarchy == false)
            {
                obj.SetActive(true);
                obj.transform.SetPositionAndRotation(pos, rot);
                return obj;
            }
        }

        if (dynamicRefill)
        {
            GameObject newPooledObject = GameObject.Instantiate(pooledPrefab, Vector3.zero, Quaternion.identity);
            pooledList.Add(newPooledObject);

            return newPooledObject;
        }

        return null;
    }
}
