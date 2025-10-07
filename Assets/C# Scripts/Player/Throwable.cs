using System.Collections.Generic;
using UnityEngine;



public class Throwable : MonoBehaviour
{
    [SerializeField] private ThrowableStats stats;
    private Collider coll;


    public void Init(ThrowableStats stats, float lagCompensation)
    {
        coll = GetComponent<Collider>();

        this.stats = stats;
        Invoke(nameof(Activate), stats.activationDelay - lagCompensation);
    }

    public void Activate()
    {
        Collider[] affectedPlayerColliders = Physics.OverlapSphere(transform.position, stats.outerRadius, GlobalGameData.PlayerHitBoxLayerMask, QueryTriggerInteraction.Collide);
        int affectedPlayerCount = affectedPlayerColliders.Length;

        if (affectedPlayerCount == 0) return;

        HashSet<IDamagable> damagedTargets = new HashSet<IDamagable>(affectedPlayerCount);

        hit = GrenadeRaycaster.CheckHit(coll, stats.outerRadius, affectedPlayerColliders, rayCount);

        for (int i = 0; i < affectedPlayerColliders.Length; i++)
        {
            if (hit[i] && affectedPlayerColliders[i].TryGetComponent(out SmartHitBox smartHitBox))
            {
                if (damagedTargets.Contains(smartHitBox.Target) == false)
                {
                    Vector3 playerPos = smartHitBox.transform.position;

                    float distance = Vector3.Distance(playerPos, transform.position);
                    Vector3 hitPoint = playerPos - Vector3.up;
                    Vector3 hitDir = (playerPos - transform.position).normalized;

                    smartHitBox.DealDamageToTargetObject(stats.GetDamageOutput(distance), false, hitPoint, hitDir, out _);
                    damagedTargets.Add(smartHitBox.Target);
                }
            }
        }
    }


    private void OnCollisionEnter(Collision collision)
    {

    }


#if UNITY_EDITOR
    [Range(1, 10)]
    [SerializeField] private int rayCount = 1;
    [SerializeField] private bool[] hit;


    [ContextMenu("KABOOM")]
    private void ForceExplode()
    {
        Init(stats, 0);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.innerRadius);
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, stats.outerRadius);


        if (TryGetComponent(out Collider coll))
        {
            Collider[] affectedPlayerColliders = Physics.OverlapSphere(transform.position, stats.outerRadius, GlobalGameData.PlayerHitBoxLayerMask, QueryTriggerInteraction.Collide);

            if (affectedPlayerColliders.Length > 0)
            {
                hit = GrenadeRaycaster.CheckHit(coll, stats.outerRadius, affectedPlayerColliders, rayCount);
            }
        }
    }
#endif
}