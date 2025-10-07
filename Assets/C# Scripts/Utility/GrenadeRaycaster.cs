using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;



[BurstCompile]
public static class GrenadeRaycaster
{

    [BurstCompile]
    public static bool[] CheckHits(Collider grenade, float effectRadius, Collider[] targets, int rayCount = 1)
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
        Vector3 horizontalAxis = Vector3.zero;
        Vector3 verticalAxis = Vector3.zero;
        Vector3 delta = Vector3.zero;
        Vector3 rayDir = Vector3.zero;
        float distance = 0f;
        Vector3 grenadePoint = Vector3.zero;
        Vector3 targetPoint = Vector3.zero;
        bool hitTarget;
        float offsetXPercentage, offsetYPercentage = 0f;
        float len;

        // For every target
        for (int t = 0; t < targets.Length; t++)
        {
            target = targets[t];
            targetBounds = target.bounds;
            targetCenter = targetBounds.center;

            lookAtRotation = Quaternion.LookRotation(targetCenter - grenadeCenter, grenade.transform.up);

            horizontalAxis = lookAtRotation * Vector3.left;
            verticalAxis = lookAtRotation * Vector3.down;

            hitTarget = false;

            // Center Raycast
            delta = targetCenter - grenadeCenter;
            rayDir = delta.normalized;
            distance = delta.magnitude;


            // For Horizontal offset
            for (int x = 0; x < rayCount; x++)
            {
                // For Vertical offset
                for (int y = 0; y < rayCount; y++)
                {
                    offsetXPercentage = SpreadValue(x, rayCount);
                    offsetYPercentage = SpreadValue(y, rayCount);

                    // Clamp to circle radius
                    len = math.sqrt(offsetXPercentage * offsetXPercentage + offsetYPercentage * offsetYPercentage);
                    if (len > 1f)
                    {
                        offsetXPercentage /= len;
                        offsetYPercentage /= len;
                    }

                    grenadePoint = grenadeCenter +
                        Vector3.Scale(horizontalAxis, grenadeBounds.extents) * offsetXPercentage +
                        Vector3.Scale(verticalAxis, grenadeBounds.extents) * offsetYPercentage;

                    targetPoint = targetCenter +
                        Vector3.Scale(horizontalAxis, targetBounds.extents) * offsetXPercentage +
                        Vector3.Scale(verticalAxis, targetBounds.extents) * offsetYPercentage;

                    delta = targetPoint - grenadePoint;
                    rayDir = delta.normalized;
                    distance = delta.magnitude;

                    Debug.DrawRay(grenadePoint, rayDir * distance, Color.grey);

                    if (Physics.Raycast(grenadePoint, rayDir, out RaycastHit hit, distance * 1.1f))
                    {
                        if (hit.collider == target)
                        {
                            hitTarget = true;
#if !UNITY_EDITOR
                break; // stop early
#else
                            Debug.DrawRay(grenadePoint, rayDir * distance, Color.blue);
#endif
                        }
                    }
                }
            }

            results[t] = hitTarget;
        }

        return results;
    }

    private static float SpreadValue(int index, int count)
    {
        if (count <= 1)
            return 0f;

        return Mathf.Lerp(-1f, 1f, (float)index / (count - 1));
    }
}