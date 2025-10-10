using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;



namespace FirePixel.Networking
{
    public class NetworkVFXPool : NetworkBehaviour
    {
        public static NetworkVFXPool Instance { get; set; }

        
        [SerializeField] private PoolingSystem<VisualEffect> muzzleFlashPool;
        [SerializeField] private float muzzleFlashMaxDuration;
        private WaitForSeconds waitTime;


        private void Awake()
        {
            Instance = this;
            muzzleFlashPool.Initialize();
            waitTime = new WaitForSeconds(muzzleFlashMaxDuration);
        }


        #region Muzzle Flash Get and Release

        public VisualEffect GetMuzzleFlashObj(Vector3 pos)
        {
            return GetMuzzleFlashObj(pos, Quaternion.identity, Vector3.one);
        }

        public VisualEffect GetMuzzleFlashObj(Vector3 pos, Quaternion rot)
        {
            return GetMuzzleFlashObj(pos, rot, Vector3.one);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VisualEffect GetMuzzleFlashObj(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            VisualEffect vfxObj = muzzleFlashPool.GetPulledObject(pos, rot, scale);
            vfxObj.Play();

            // Release VFX Object after max duration, clear parent only if parent was set ^
            StartCoroutine(ReleaseObjectFromTargetPool(muzzleFlashPool, waitTime, vfxObj));

            return vfxObj;

        }

        #endregion


        /// <summary>
        /// Release target Pooled Object from target Pool after X seconds stored in WaitForSeconds <paramref name="waitTime"/>
        /// </summary>
        private IEnumerator ReleaseObjectFromTargetPool<T>(PoolingSystem<T> targetPool, WaitForSeconds waitTime, T toReleaseObj, bool clearParent = false) where T : Component
        {
            yield return waitTime;

            targetPool.ReleasePooledObject(toReleaseObj, clearParent);
        }
    }
}