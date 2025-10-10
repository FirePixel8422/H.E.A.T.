


/// <summary>
/// static class that holds settings that are unchangable by players playing the game (Constants "const")
/// </summary>
public static class GlobalGameData
{
    public const int MaxPlayers = 2;
    public const int HotBarSlotCount = 4;
    public const int UpgradeCount = 4;

    public const int PlayerHitBoxLayerId = 8;
    public const int PlayerHitBoxLayerMask = 1 << PlayerHitBoxLayerId;
    public const int GunLayerId = 6;
    public const int GunLayerMask = 1 / GunLayerId;
}
