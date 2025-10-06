using UnityEngine;



/// <summary>
/// static class that holds settings that are unchangable by players playing the game (Constants "const")
/// </summary>
public static class GlobalGameData
{
    public const int MaxPlayers = 2;
    public const int HotBarSlotCount = 4;

    public static readonly int PlayerLayer = LayerMask.NameToLayer("PlayerHitBox");
    public static readonly int GunLayer = LayerMask.NameToLayer("Gun");
}
