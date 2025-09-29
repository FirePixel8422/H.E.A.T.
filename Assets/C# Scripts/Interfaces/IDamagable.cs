using UnityEngine;



public interface IDamagable
{
    public void DealDamage(float damage, bool headShot, Vector3 hitPoint, Vector3 hitDir);
}