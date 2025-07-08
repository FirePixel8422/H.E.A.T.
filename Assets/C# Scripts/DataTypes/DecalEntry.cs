using UnityEngine.Rendering.Universal;



[System.Serializable]
public struct DecalEntry
{
    public DecalProjector decalProjector;
    public float deathTime;

    public DecalEntry(DecalProjector decalProjector, float deathTime)
    {
        this.decalProjector = decalProjector;
        this.deathTime = deathTime;
    }
}
