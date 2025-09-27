using UnityEngine;


[System.Serializable]
public struct LocalOffset
{
    public Vector3 position;
    public Vector3 eulerRotation;
    public Quaternion Rotation => Quaternion.Euler(eulerRotation);

    public LocalOffset(Vector3 position, Vector3 rotation)
    {
        this.position = position;
        this.eulerRotation = rotation;
    }
}