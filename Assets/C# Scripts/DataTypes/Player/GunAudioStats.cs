using UnityEngine;



[System.Serializable]
public struct GunAudioStats
{
    [Header("Audio clip to play when shooting")]
    public AudioClip shootAudioClip;
    public MinMaxFloat minMaxPitch;
    public MinMaxFloat minMaxPitchAtMaxHeat;

    [Header("Audio clip to play when gun overheats")]
    public AudioClip overHeatAudioClip;
    public MinMaxFloat overHeatMinMaxPitch;

    [Header("Audio clip to play when you hit opponent")]
    public AudioClip onHitAudioClip;
    public MinMaxFloat onHitMinMaxPitch;



    public static GunAudioStats Default => new GunAudioStats()
    {
        shootAudioClip = null,
        minMaxPitch = new MinMaxFloat(0.95f, 1.05f),
        minMaxPitchAtMaxHeat = new MinMaxFloat(0.95f, 1.05f),

        overHeatAudioClip = null,
        overHeatMinMaxPitch = new MinMaxFloat(0.95f, 1.05f),

        onHitAudioClip = null,
        onHitMinMaxPitch = new MinMaxFloat(0.95f, 1.05f),
    };
}