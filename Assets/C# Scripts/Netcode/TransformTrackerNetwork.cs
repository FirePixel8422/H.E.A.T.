using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


namespace FirePixel.Networking
{
    /// <summary>
    /// Component to track transforms of objects in the scene by their index (synced over network).
    /// </summary>
    public class TransformTrackerNetwork : NetworkBehaviour
    {
        [SerializeField] private Transform[] trackedObjects;
        private Dictionary<int, Transform> idToTransform = new Dictionary<int, Transform>();


        private void Start()
        {
            for (int i = 0; i < trackedObjects.Length; i++)
            {
                idToTransform[i] = trackedObjects[i];
            }

            trackedObjects = null; // Clear the array to free up memory, we only need the dictionary.
        }

        public bool TryGetById(int id, out Transform trans)
        {
            return idToTransform.TryGetValue(id, out trans);
        }
        public Transform GetById(int id)
        {
            return idToTransform.TryGetValue(id, out var t) ? t : null;
        }

        public int GetIdByTransform(Transform t)
        {
            for (int i = 0; i < trackedObjects.Length; i++)
            {
                if (trackedObjects[i] == t) return i;
            }
            return -1;
        }
    }
}