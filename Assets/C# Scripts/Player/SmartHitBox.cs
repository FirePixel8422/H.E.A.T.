using UnityEngine;



public class SmartHitBox : SurfaceTypeIdentifier
{
    [Header("IDamagable Object that this collider is part of\nLeave empty to auto find in parent")]
    [SerializeField] private IDamagable target;
    public IDamagable Target => target;

    [SerializeField] private bool isHeadHitBox;
    public bool IsHeadHitBox => isHeadHitBox;


    private void Awake()
    {
        if (target == null)
        {
            target = transform.GetComponentInParent<IDamagable>(true);

            DebugLogger.LogError(gameObject + " has no targetObject for IDamagable", target == null);
        }
    }


    public void DealDamageToTargetObject(float damage, bool headShot, Vector3 hitPoint, Vector3 hitDir, out HitTypeResult hitTypeResult)
    {
        target.DealDamage(damage, headShot, hitPoint, hitDir, out hitTypeResult);
    }
}