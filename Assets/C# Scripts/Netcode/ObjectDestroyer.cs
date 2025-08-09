using Unity.Netcode;
using UnityEngine;




public class ObjectDestroyer : NetworkBehaviour
{
    [SerializeField] private Component[] selfComponentList;
    [SerializeField] private GameObject[] selfObjList;

    [SerializeField] private Component[] otherComponentList;
    [SerializeField] private GameObject[] otherObjList;


    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            DestroyForOwned();
        }
        else
        {
            DestroyForNonOwned();
        }

        Destroy(this);
    }

    private void DestroyForOwned()
    {
        for (int i = 0; i < selfComponentList.Length; i++)
        {
            Destroy(selfComponentList[i]);
            
            if (selfComponentList[i] is NetworkBehaviour)
            {
                DebugLogger.LogWarning("ObjectDestroyer: Destroying NetworkBehaviour component " + selfComponentList[i].GetType().Name + " on " + gameObject.name + "This is likely wrong");
            }
        }

        for (int i = 0; i < selfObjList.Length; i++)
        {
            Destroy(selfObjList[i]);
        }
    }

    private void DestroyForNonOwned()
    {
        for (int i = 0; i < otherComponentList.Length; i++)
        {
            Destroy(otherComponentList[i]);
        }

        for (int i = 0; i < otherObjList.Length; i++)
        {
            Destroy(otherObjList[i]);

            if (otherComponentList[i] is NetworkBehaviour)
            {
                DebugLogger.LogWarning("ObjectDestroyer: Destroying NetworkBehaviour component " + otherObjList[i].GetType().Name + " on " + gameObject.name + "This is likely wrong");
            }
        }
    }
}
