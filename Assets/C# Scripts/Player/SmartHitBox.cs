using UnityEngine;



public class SmartHitBox : MonoBehaviour
{
    [Header("IDamagable Object that this collider is part of\nLeave empty to auto find in parent")]
    [SerializeField] private IDamagable targetObject;
    [SerializeField] private bool isHeadHitBox;
    public bool IsHeadHitBox => isHeadHitBox;


    private void Awake()
    {
        if (targetObject == null)
        {
            targetObject = transform.GetComponentInParent<IDamagable>();

            DebugLogger.LogError(gameObject.name = " has no targetObject for IDamagable");
        }
    }


    public void DealDamageToTargetObject(float damage, bool headShot, Vector3 hitPoint, Vector3 hitDir)
    {
        targetObject.DealDamage(damage, headShot, hitPoint, hitDir);
    }
}