using UnityEngine.Rendering.Universal;



[System.Serializable]
public class BulletHoleEntry
{
    public DecalProjector decalProjector;
    public float deathTime;

    public BulletHoleEntry(DecalProjector decalProjector, float deathTime)
    {
        this.decalProjector = decalProjector;
        this.deathTime = deathTime;
    }
}
