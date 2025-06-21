using Unity.Netcode;
using UnityEngine;



[System.Serializable]
public struct HeatSinkStats : INetworkSerializable
{
    [Header("How much heat to add per shot")]
    public float heatPerShot;

    [Header("Time before decaying heat and how fast")]
    public float heatDecayDelay;
    public float heatDecayPower;

    [Header("Time before decaying heat when overheated and how fast")]
    public float overheatDecayDelay;
    public float overheatDecayPower;

    [Header("The fuller the bar the slower heat decays multiplier")]
    public float decayMultiplierAtMaxHeat;


    /// <summary>
    /// Deafault values for the heatsink.
    /// </summary>
    public static HeatSinkStats Default()
    {
        return new HeatSinkStats
        {
            heatPerShot = 0.1f,
            heatDecayDelay = 0.25f,
            heatDecayPower = 0.1f,
            overheatDecayDelay = 1f,
            overheatDecayPower = 0.075f,
            decayMultiplierAtMaxHeat = 10f
        };
    }


    /// <summary>
    /// Make all values syncable with Multiplayer Netcode
    /// </summary>
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref heatPerShot);
        serializer.SerializeValue(ref heatDecayDelay);
        serializer.SerializeValue(ref heatDecayPower);
        serializer.SerializeValue(ref overheatDecayDelay);
        serializer.SerializeValue(ref overheatDecayPower);
        serializer.SerializeValue(ref decayMultiplierAtMaxHeat);
    }
}
