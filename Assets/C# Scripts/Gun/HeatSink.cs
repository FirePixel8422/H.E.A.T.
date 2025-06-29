using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class HeatSink : MonoBehaviour
{
    [SerializeField] private Image heatBar;
    [SerializeField] private HeatSinkStats stats;


    [Tooltip("The amount of heat the gun has accumulated, from 0 to stats.heatSinkSize. stats.heatSinkSize means the gun is overheated.")]
    [SerializeField] private float heatAmount = 0f;

    public bool Overheated { get; private set; }
    private bool recoveringFromOverheat;

    private Animator anim;


    public void Init(HeatSinkStats _stats)
    {
        anim = GetComponentInChildren<Animator>(true);
        stats = _stats;
    }


    /// <summary>
    /// Called by GunCore when gun is not currently firing
    /// </summary
    public void OnGunIdle(float timeSinceLastShot)
    {
        //Whether the gun is cooling down and decaying heat. true when gun has not been shot for stats.heatDecayDelay amount of time
        bool coolingDown = timeSinceLastShot >= stats.heatDecayDelay;

        if (coolingDown || (Overheated && recoveringFromOverheat == false))
        {
            DecayHeat();
        }

        heatBar.fillAmount = heatAmount / stats.heatSinkSize;
    }


    /// <summary>
    /// called by GunCore when a shot is fired. Adds heat equivelent to coreStats.addedHeat.
    /// </summary>
    public void AddHeat(float addedHeat)
    {
        StopAllCoroutines();

        heatAmount += addedHeat;

        heatBar.fillAmount = heatAmount / stats.heatSinkSize;

        if (heatAmount >= stats.heatSinkSize)
        {
            heatAmount = stats.heatSinkSize;

            recoveringFromOverheat = true;
            Overheated = true;

            StartCoroutine(OverheatedCooldown());
            anim.SetBool("Overheated", true);
        }
    }


    /// <summary>
    /// Decay the heat of the heatsink over time (Called every frame if gun has not been shot for long enough).
    /// </summary>
    private void DecayHeat()
    {
        float decaySpeedMultiplier = stats.heatSinkSize + (stats.heatSinkSize - heatAmount) * stats.decayMultiplierAtMaxHeat;

        if (Overheated)
        {
            //while heatSink is recovering from overheat for stats.overheatDecayDelay amount of time, decay NO heat
            if (recoveringFromOverheat) return;

            heatAmount -= Time.deltaTime * stats.overheatDecayPower * decaySpeedMultiplier;

            if (heatAmount < 0)
            {
                heatAmount = 0;

                anim.SetBool("Overheated", false);
                Overheated = false;
            }
        }
        else
        {
            heatAmount -= Time.deltaTime * stats.heatDecayPower * decaySpeedMultiplier;

            if (heatAmount < 0)
            {
                heatAmount = 0;
            }
        }
    }

    
    private IEnumerator OverheatedCooldown()
    {
        yield return new WaitForSeconds(stats.overheatDecayDelay);

        recoveringFromOverheat = false;
    }
}
