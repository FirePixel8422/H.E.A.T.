using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Helper to set Matrix data into transform.
/// </summary>
public static class TransformUtility
{
    // Set Transform from a world-space matrix
    public static void SetTransformFromMatrix(Transform target, Matrix4x4 m)
    {
        // position
        Vector3 position = m.GetColumn(3);

        // scale (lengths of basis vectors)
        Vector3 scale = new Vector3(
            m.GetColumn(0).magnitude,
            m.GetColumn(1).magnitude,
            m.GetColumn(2).magnitude
        );

        // rotation (extract quaternion from 3x3)
        Quaternion rotation = QuaternionFromMatrix(m);

        target.position = position;
        target.rotation = rotation;
        target.localScale = scale;
    }

    // If the matrix is intended as LOCAL transform, use this instead:
    public static void SetLocalTransformFromMatrix(Transform target, Matrix4x4 localMatrix)
    {
        Vector3 localPosition = localMatrix.GetColumn(3);
        Vector3 localScale = new Vector3(
            localMatrix.GetColumn(0).magnitude,
            localMatrix.GetColumn(1).magnitude,
            localMatrix.GetColumn(2).magnitude
        );
        Quaternion localRotation = QuaternionFromMatrix(localMatrix);

        target.localPosition = localPosition;
        target.localRotation = localRotation;
        target.localScale = localScale;
    }

    // ----- utility to get quaternion out of a 4x4 matrix -----
    private static Quaternion QuaternionFromMatrix(Matrix4x4 m)
    {
        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(Mathf.Max(0f, 1f + m.m00 + m.m11 + m.m22)) * 0.5f;
        q.x = Mathf.Sqrt(Mathf.Max(0f, 1f + m.m00 - m.m11 - m.m22)) * 0.5f;
        q.y = Mathf.Sqrt(Mathf.Max(0f, 1f - m.m00 + m.m11 - m.m22)) * 0.5f;
        q.z = Mathf.Sqrt(Mathf.Max(0f, 1f - m.m00 - m.m11 + m.m22)) * 0.5f;

        q.x *= Mathf.Sign(q.x * (m.m21 - m.m12));
        q.y *= Mathf.Sign(q.y * (m.m02 - m.m20));
        q.z *= Mathf.Sign(q.z * (m.m10 - m.m01));

        return q;
    }


    public static void DestroyAllChildren(this Transform parent)
    {
        foreach (Transform child in parent)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Get all children recursively
    /// </summary>
    public static List<Transform> GetAllChildren(this Transform parent)
    {
        List<Transform> children = new List<Transform>();
        CollectChildren(parent, children);
        return children;
    }

    private static void CollectChildren(Transform parent, List<Transform> list)
    {
        foreach (Transform child in parent)
        {
            list.Add(child);
            CollectChildren(child, list); // recurse deeper
        }
    }
}
