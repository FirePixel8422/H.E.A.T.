using Unity.Netcode;
using UnityEngine;



[System.Serializable]
public struct HeatSinkStats : INetworkSerializable
{
    [Header("Size of the heatsink")]
    public float heatSinkSize;

    [Header("Time before decaying heat and how fast")]
    public float heatDecayDelay;
    public float heatDecayPower;
    public float heatDecayPowerMultiplier;

    [Header("Time before decaying heat when overheated and how fast")]
    public float overheatDecayDelay;
    public float overheatDecayPower;
    public float overheatDecayPowerMultiplier;

    [Header("The fuller the bar the slower heat decays multiplier")]
    public float decayMultiplierAtMaxHeat;


    /// <summary>
    /// Deafault values for the heatsink.
    /// </summary>
    public static HeatSinkStats Default => new HeatSinkStats()
    {
        heatSinkSize = 1,
        heatDecayDelay = 0.25f,
        heatDecayPower = 0.1f,
        heatDecayPowerMultiplier = 1f,
        overheatDecayDelay = 1f,
        overheatDecayPower = 0.075f,
        overheatDecayPowerMultiplier = 1f,
        decayMultiplierAtMaxHeat = 10f
    };


    /// <summary>
    /// Make all values syncable with Multiplayer Netcode
    /// </summary>
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref heatSinkSize);
        serializer.SerializeValue(ref heatDecayDelay);
        serializer.SerializeValue(ref heatDecayPower);
        serializer.SerializeValue(ref heatDecayPowerMultiplier);
        serializer.SerializeValue(ref overheatDecayDelay);
        serializer.SerializeValue(ref overheatDecayPower);
        serializer.SerializeValue(ref overheatDecayPowerMultiplier);
        serializer.SerializeValue(ref decayMultiplierAtMaxHeat);
    }
}
