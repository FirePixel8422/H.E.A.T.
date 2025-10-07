using UnityEngine;



public interface IDamagable
{
    public float MaxHealth { get; set; }
    public float CurrentHealth { get; set; }


    public void DealDamage(float damage, bool headShot, Vector3 hitPoint, Vector3 hitDir, out HitTypeResult hitTypeResult);



    /// <summary>
    /// Get <see cref="HitTypeResult"/> based on <paramref name="headShot"/> and <paramref name="dead"/> state
    /// </summary>
    public static HitTypeResult CalculateHitType(bool headShot, bool dead)
    {
        if (headShot)
        {
            return dead ? HitTypeResult.HeadShotKill : HitTypeResult.HeadShot;
        }
        else
        {
            return dead ? HitTypeResult.NormalKill : HitTypeResult.Normal;
        }
    }
}