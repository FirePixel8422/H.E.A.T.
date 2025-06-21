using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class HeatSink : MonoBehaviour
{
    [SerializeField] private Image heatBar;

    [SerializeField] private HeatSinkStatsSO heatSinkStatsSO;
    private HeatSinkStats stats;


    private float timeSinceLastShot;
    [Tooltip("Whether the gun is cooling down and decaying heat. true when gun has not been shot for stats.heatDecayDelay amount of time")]
    private bool CoolingDown => timeSinceLastShot >= stats.heatDecayDelay;

    [Tooltip("The amount of heat the gun has accumulated, from 0 to 1. 1 means the gun is overheated.")]
    private float heatAmount = 0f;

    private bool overheated;
    private bool overHeatedAnimationActive;

    private Animator anim;


    private void Start()
    {
        anim = GetComponent<Animator>();
        stats = heatSinkStatsSO.data;
    }

    private void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    private void OnDisable() => UpdateScheduler.UnregisterUpdate(OnUpdate);


    private void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnShoot();
        }
        else
        {
            timeSinceLastShot += Time.deltaTime;
        }

        if (CoolingDown || (overHeatedAnimationActive == false && overheated))
        {
            DecayHeat();
        }

        heatBar.fillAmount = heatAmount;
    }


    /// <summary>
    /// Called when the gun shoots during this frame
    /// </summary>
    private void OnShoot()
    {
        //reset time since last shot value
        timeSinceLastShot = 0;

        //add stats.heatPerShot of heat to the heatSink
        AddHeat();
    }

    
    private void AddHeat()
    {
        StopAllCoroutines();

        heatAmount += stats.heatPerShot;

        if (heatAmount >= 1)
        {
            heatAmount = 1;

            overHeatedAnimationActive = true;
            overheated = true;

            StartCoroutine(OverheatedCooldown());
            anim.SetBool("Overheated", true);
        }
    }


    /// <summary>
    /// Decay the heat of the heatsink over time (Called every frame if gun has not been shot for long enough).
    /// </summary>
    private void DecayHeat()
    {
        float decaySpeedMultiplier = 1 + (1 - heatAmount) * stats.decayMultiplierAtMaxHeat;

        if (overheated)
        {
            heatAmount -= Time.deltaTime * stats.overheatDecayPower * decaySpeedMultiplier;

            if (heatAmount < 0)
            {
                heatAmount = 0;

                anim.SetBool("Overheated", false);
                overheated = false;
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

        overHeatedAnimationActive = false;
    }
}
