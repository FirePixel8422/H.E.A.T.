using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class HeatBar : MonoBehaviour
{
    [SerializeField] private Image heatBar;

    [Header("How much heat to add per shot")]
    [SerializeField] private float heatPerShot = 0.1f;

    [Header("Time before decaying heat and how fast")]
    [SerializeField] private float heatDecayDelay = 0.25f;
    [SerializeField] private float heatDecayPower = 0.1f;

    [Header("Time before decaying heat when overheated and how fast")]
    [SerializeField] private float overheatDecayDelay = 1f;
    [SerializeField] private float overheatDecayPower = 0.075f;

    [Header("The fuller the bar the slower heat decays multiplier")]
    [SerializeField] private float decayMultiplierAtMaxHeat = 10;

    private float heatAmount = 0f;

    private bool coolingDown;
    private bool overheated;
    private bool overHeatedAnimationActive;

    private Animator anim;


    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    
    private void Update()
    {
        if (overheated == false && Input.GetMouseButtonDown(0))
        {
            OverheatGun();
        }

        if (coolingDown || (overHeatedAnimationActive == false && overheated))
        {
            DecayHeat();
        }

        heatBar.fillAmount = heatAmount;
    }

    
    private void OverheatGun()
    {
        //disable weapon auto cooldown until not fired for heatDecayDelay amount of time
        coolingDown = false;

        StopAllCoroutines();

        heatAmount += heatPerShot;

        if (heatAmount >= 1)
        {
            heatAmount = 1;

            overHeatedAnimationActive = true;
            overheated = true;

            StartCoroutine(OverheatedCooldown());
            anim.SetBool("Overheated", true);
        }
        else
        {
            StartCoroutine(HeatDecayDelay());
        }
    }

    
    private void DecayHeat()
    {
        if (overheated)
        {
            heatAmount -= Time.deltaTime * overheatDecayPower * (1 + (1 - heatAmount) * 10);

            if (heatAmount < 0)
            {
                heatAmount = 0;

                anim.SetBool("Overheated", false);
                overheated = false;
            }
        }
        else
        {
            heatAmount -= Time.deltaTime * heatDecayPower * (1 + (1 - heatAmount) * 10);

            if (heatAmount < 0)
            {
                heatAmount = 0;
                coolingDown = false;
            }
        }
    }


    
    private IEnumerator HeatDecayDelay()
    {
        yield return new WaitForSeconds(heatDecayDelay);

        coolingDown = true;
    }

    
    private IEnumerator OverheatedCooldown()
    {
        yield return new WaitForSeconds(overheatDecayDelay);

        overHeatedAnimationActive = false;
    }
}
