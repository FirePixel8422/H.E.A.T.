using UnityEngine;



[System.Serializable]
public struct ThrowableStats
{
    [Header("Time before throwable activates")]
    public float activationDelay;

    [Header("Activate when hitting a collider")]
    public bool activateOnImpact;

    [Header("Range at which throwable is max effective")]
    public float innerRadius;
    [Header("Range at which the throwable will barely be min effective, .1 meter further will miss")]
    public float outerRadius;


    [Header("Max Damage at inner radius hit")]
    [SerializeField] private float damage;

    [Header("Damage faloff curve AFTER the 'innerRadius'")]
    [SerializeField] private NativeSampledAnimationCurve damageFallOffCurve;

    public float GetDamageOutput(float distance)
    {
        return damage * GetEffectivenessPercentage(damageFallOffCurve, distance);
    }

    //[Header()]
    public float flashPower;




    /// <summary>
    /// Get effectiveness of throwable based on distance of the target and internal <see cref="innerRadius"/>, <see cref="outerRadius"/> and <paramref name="curve"/>
    /// </summary>
    private float GetEffectivenessPercentage(NativeSampledAnimationCurve curve, float distance)
    {
        if (distance < innerRadius) return 1;

        return curve.Evaluate(distance / outerRadius);
    }



    /// <summary>
    /// Bake all curves from the AnimationCurves in this struct into their internal float arrays.
    /// </summary>
    public void BakeAllCurves()
    {
        damageFallOffCurve.Bake();
    }
    public void Dispose()
    {
        damageFallOffCurve.Dispose();
    }

    public ThrowableStats Default => new ThrowableStats()
    {
        activationDelay = 1,
        activateOnImpact = false,

        innerRadius = 0.5f,
        outerRadius = 1.5f,

        damage = 120,
        damageFallOffCurve = NativeSampledAnimationCurve.Default,
    };
}