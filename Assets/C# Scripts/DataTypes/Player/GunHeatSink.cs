using UnityEngine;
using UnityEngine.InputSystem.iOS;
using UnityEngine.UI;



[System.Serializable]
public struct GunHeatSink
{
    [SerializeField] private Image heatBar;
    [SerializeField] private Animator anim;

    public HeatSinkStats stats;

    [Tooltip("The amount of heat the gun has accumulated, from 0 to stats.heatSinkSize. stats.heatSinkSize means the gun is overheated.")]
    [SerializeField] private float heatAmount;
    public float HeatPercentage => heatAmount / stats.heatSinkSize;

    [Space(10)]

    private float timeSinceLastShot;

    private float overheatCooldownTimer;
    public bool Overheated { get; private set; }
    private bool recoveringFromOverheat;



    public void Init(Image heatBar, Animator anim)
    {
        this.heatBar = heatBar;
        this.anim = anim;
    }

    /// <summary>
    /// Called by GunCore when a shot is fired. Adds heat equivelent to coreStats.addedHeat.
    /// </summary>
    /// <returns>The heat percentage from 0-1 before adding the new heat</returns>
    public void AddHeat(float addedHeat, out float prevHeatPercentage, out float newHeatPercentage)
    {
        prevHeatPercentage = HeatPercentage;
        heatAmount += addedHeat;
        newHeatPercentage = HeatPercentage;

        heatBar.fillAmount = newHeatPercentage;

        if (newHeatPercentage >= 1)
        {
            heatAmount = stats.heatSinkSize;

            recoveringFromOverheat = true;
            Overheated = true;

            overheatCooldownTimer = stats.overheatDecayDelay; // Set timer
            anim.SetBool("Overheated", true);
        }
    }

    /// <summary>
    /// Called by GunCore, gives timeSinceLastShot as to check if heatsink may cool down.
    /// </summary
    public void UpdateHeatSink(float frameTimeElapsedSinceLastShot, float deltaTime)
    {
        timeSinceLastShot += frameTimeElapsedSinceLastShot;

        //Whether the gun is cooling down and decaying heat. true when gun has not been shot for stats.heatDecayDelay amount of time
        bool coolingDown = timeSinceLastShot >= stats.heatDecayDelay;

        if (recoveringFromOverheat)
        {
            overheatCooldownTimer -= deltaTime;
            if (overheatCooldownTimer <= 0f)
            {
                recoveringFromOverheat = false;
            }
        }
        else if (coolingDown || Overheated)
        {
            DecayHeat(deltaTime);
        }

        heatBar.fillAmount = heatAmount / stats.heatSinkSize;
    }



    /// <summary>
    /// Decay the heat of the heatsink over time (Called every frame if gun has not been shot for long enough).
    /// </summary>
    private void DecayHeat(float deltaTime)
    {
        float decaySpeedMultiplier = stats.heatSinkSize * stats.GetHeatDecayMultiplier(HeatPercentage);

        if (Overheated)
        {
            //while heatSink is recovering from overheat for stats.overheatDecayDelay amount of time, decay NO heat
            if (recoveringFromOverheat) return;

            heatAmount -= deltaTime * stats.overheatDecayPower * decaySpeedMultiplier;

            if (heatAmount < 0)
            {
                heatAmount = 0;

                anim.SetBool("Overheated", false);
                Overheated = false;
            }
        }
        else
        {
            heatAmount -= deltaTime * stats.heatDecayPower * decaySpeedMultiplier;

            if (heatAmount < 0)
            {
                heatAmount = 0;
            }
        }
    }
}