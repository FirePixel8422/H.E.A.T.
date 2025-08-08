using Unity.Netcode;
using UnityEngine;



[System.Serializable]
public struct BulletHoleMessage : INetworkSerializable
{
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale;
    public SurfaceType hitSurfaceType;
    public float lifetime;


    public BulletHoleMessage(Vector3 pos, Quaternion rot, Vector3 scale, SurfaceType hitSurfaceType, float lifetime)
    {
        this.pos = pos;
        this.rot = rot;
        this.scale = scale;
        this.hitSurfaceType = hitSurfaceType;
        this.lifetime = lifetime;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref pos);
        serializer.SerializeValue(ref rot);
        serializer.SerializeValue(ref scale);
        serializer.SerializeValue(ref hitSurfaceType);
        serializer.SerializeValue(ref lifetime);
    }
}