using Unity.Burst;
using UnityEngine;



[BurstCompile]
public static class GrenadeRaycaster
{

    [BurstCompile]
    public static bool[] CheckHit(Collider grenade, float effectRadius, Collider[] targets, int rayCount = 1)
    {
#if UNITY_EDITOR
        if (targets == null || targets.Length == 0)
        {
            DebugLogger.LogError("targets is uninitialized or has 0 entries...");
            return new bool[0];
        }
#endif

        bool[] results = new bool[targets.Length];

        // Grenade bounds and center
        Bounds grenadeBounds = grenade.bounds;
        Vector3 grenadeCenter = grenadeBounds.center;

        // Variables moved outside loops
        Collider target = null;
        Bounds targetBounds = new Bounds();
        Vector3 targetCenter = Vector3.zero;
        Quaternion lookAtRotation = Quaternion.identity;
        Vector3[] axes = new Vector3[5];
        Vector3 delta = Vector3.zero;
        Vector3 rayDir = Vector3.zero;
        float distance = 0f;
        Vector3 grenadePoint = Vector3.zero;
        Vector3 targetPoint = Vector3.zero;
        bool hitTarget;
        float offsetPercentage = 0f;

        // Precompute offsets
        float[] offsets = new float[rayCount];
        for (int i = 0; i < rayCount; i++)
            offsets[i] = 1f / rayCount * (i + 1);

        for (int t = 0; t < targets.Length; t++)
        {
            target = targets[t];
            targetBounds = target.bounds;
            targetCenter = targetBounds.center;

            lookAtRotation = Quaternion.LookRotation(targetCenter - grenadeCenter, grenade.transform.up);

            axes[0] = lookAtRotation * Vector3.forward;
            axes[1] = lookAtRotation * Vector3.right;
            axes[2] = lookAtRotation * Vector3.up;
            axes[3] = -axes[1];
            axes[4] = -axes[2];

            hitTarget = false;

            // Center Raycast
            delta = targetCenter - grenadeCenter;
            rayDir = delta.normalized;
            distance = delta.magnitude;

            if (Physics.Raycast(grenadeCenter, rayDir, out RaycastHit hit, distance))
            {
                if (hit.collider == target)
                {
                    hitTarget = true;
#if !UNITY_EDITOR
                break; // no need to check other axes for this target
#else
                    Debug.DrawRay(grenadeCenter, rayDir * distance, Color.blue);
#endif
                }
            }

            // Directional Offset Raycasts
            foreach (Vector3 dir in axes)
            {
                for (int i = 0; i < rayCount; i++)
                {
                    offsetPercentage = offsets[i];

                    grenadePoint = grenadeCenter + Vector3.Scale(dir, grenadeBounds.extents) * offsetPercentage;
                    targetPoint = targetCenter + Vector3.Scale(dir, targetBounds.extents) * offsetPercentage;

                    delta = targetPoint - grenadePoint;
                    rayDir = delta.normalized;
                    distance = delta.magnitude;

                    if (Physics.Raycast(grenadePoint, rayDir, out hit, distance))
                    {
                        if (hit.collider == target)
                        {
                            hitTarget = true;
#if !UNITY_EDITOR
                            break; // no need to check other axes for this target
#else
                            Debug.DrawRay(grenadePoint, rayDir * distance, Color.blue);
                        }
                        else
                        {
                            Debug.DrawRay(grenadePoint, rayDir * distance, Color.grey);
                        }
                    }
                    else
                    {
                        Debug.DrawRay(grenadePoint, rayDir * distance, Color.grey);
#endif
                    }
                }
            }

            results[t] = hitTarget;
        }

        return results;
    }

}