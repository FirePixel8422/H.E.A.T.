using UnityEngine;




public class ObjectDestroyer : MonoBehaviour
{
    [SerializeField] private Component[] componentList;
    [SerializeField] private GameObject[] ObjList;

    public void DestroyAll()
    {
        for (int i = 0; i < componentList.Length; i++)
        {
            Destroy(componentList[i]);
        }

        for (int i = 0; i < ObjList.Length; i++)
        {
            Destroy(ObjList[i]);
        }

        Destroy(this);
    }
}
