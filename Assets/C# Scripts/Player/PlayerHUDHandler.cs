using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public class PlayerHUDHandler : NetworkBehaviour
{

    #region CrossHair and HitMarkers

    [Header("Crosshair")]
    [SerializeField] private Image crossHairPlus;

    [SerializeField] private Image[] crossHairLines = new Image[4];
    [SerializeField] private int2[] crossHairLineDirs = new int2[4];
    [SerializeField] private MinMaxFloat crossHairOffsetMinMax;
    [SerializeField] private float crossHairAnimationPower = 0.1f;

    [SerializeField] private float crossHairStabilizeSpeed = 5;
    private float crossHairOffset01;

    [Header("Hitmarkers")]
    [SerializeField] private Image[] hitMarkers = new Image[4];
    [SerializeField] private int2[] hitMarkerDirs = new int2[4];
    [SerializeField] private MinMaxFloat hitMarkerOffsetMinMax;
    [SerializeField] private float hitMarkerAnimationPower = 0.1f;

    [SerializeField] private float hitMarkerDuration;
    [SerializeField] private float hitMarkerFadeTime;

    [SerializeField] private float hitMarkerStabilizeSpeed = 5;
    private float hitMarkerOffset01;

    [Space(10)]
    [SerializeField] private Color crossHairColor;
    [SerializeField] private Color hitMarkerColor;
    [SerializeField] private Color hitMarkerHitColor, hitMarkerCritColor, hitMarkerKillColor;

    #endregion


    #region HotBar

    [Header("Hotbar:")]
    [SerializeField] private HotBarSlot[] hotBarSlots = new HotBarSlot[GlobalGameSettings.HotBarSlotCount];
    [SerializeField] private HotBarItem[] hotBarItems = new HotBarItem[GlobalGameSettings.HotBarSlotCount];
    [SerializeField] private Sprite[] hotBarItemsSprites = new Sprite[GlobalGameSettings.HotBarSlotCount];

    [SerializeField] private Color slotActiveColor, slotInactiveColor, slotOnCooldownColor; 

    [SerializeField] private int selectedHotbarSlotId;
    [SerializeField] private bool secondaryWeaponEquipped;

    #endregion


    private float timeSinceLastHitMarker;
    private bool damageDealtThisFrame;


    public void SetCrossHairAlpha(float alpha)
    {
        crossHairColor.a = alpha;

        crossHairPlus.color = crossHairColor;
        for (int i = 0; i < 4; i++)
        {
            crossHairLines[i].color = crossHairColor;
        }
    }


    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            crossHairColor = crossHairPlus.color;

            UpdateScheduler.RegisterUpdate(OnUpdate);

            // Mark hitmarkers for invisible
            timeSinceLastHitMarker = hitMarkerDuration + hitMarkerFadeTime;
            UpdateHitMarkers();

#pragma warning disable UDR0004
            PlayerHealthHandler.OnDamageDealt += OnDamageDealt;
#pragma warning restore UDR0004
        }
        else
        {
#pragma warning disable UDR0004
            PlayerHealthHandler.OnDamageRecieved += OnDamageRecieved;
#pragma warning restore UDR0004
        }
    }


    #region OnDamageDealt + OnDamageRecieved

    public void OnDamageDealt(float damagePercentage, HitTypeResult hitType)
    {
        damageDealtThisFrame = true;
        timeSinceLastHitMarker = 0;

        hitMarkerOffset01 += damagePercentage * hitMarkerAnimationPower;

        SetHitMarkersColor(hitType);
    }
    public void OnDamageRecieved(float damagePercentage, HitTypeResult hitType)
    {
        
    }

    #endregion


    public void SwapToHotBarSlot(int slotId)
    {
        selectedHotbarSlotId = slotId;

        if (slotId == 2)
        {
            secondaryWeaponEquipped = true;
        }
        else
        {
            secondaryWeaponEquipped = false;
        }

        UpdateHotBarUI();
    }

    private void UpdateHotBarUI()
    {
        int primaryWeaponSlotId = secondaryWeaponEquipped ? 1 : 0;
        int secondaryWeaponSlotId = secondaryWeaponEquipped ? 0 : 1;

        hotBarSlots[0].image.sprite = hotBarItemsSprites[primaryWeaponSlotId];
        hotBarSlots[1].image.sprite = hotBarItemsSprites[secondaryWeaponSlotId];
        hotBarSlots[2].image.sprite = hotBarItemsSprites[2];
        hotBarSlots[3].image.sprite = hotBarItemsSprites[3];

        for (int i = 0; i < GlobalGameSettings.HotBarSlotCount; i++)
        {
            if (hotBarItems[i].OnCooldown)
            {
                hotBarSlots[i].image.fillAmount = hotBarItems[i].CoolDownCompletionPercent01;
                hotBarSlots[i].image.color = slotOnCooldownColor;
            }
            else
            {
                Color targetColor = selectedHotbarSlotId == i ? slotActiveColor : slotInactiveColor;
                hotBarSlots[i].image.color = targetColor;
            }
        }
    }


    public void AddCrossHairInstability(float amount)
    {
        crossHairOffset01 += amount * crossHairAnimationPower;
    }

    public void OnUpdate()
    {
        float deltaTime = Time.deltaTime;

        crossHairOffset01 = math.lerp(crossHairOffset01, 0, crossHairStabilizeSpeed * deltaTime);
        hitMarkerOffset01 = math.lerp(hitMarkerOffset01, 0, hitMarkerStabilizeSpeed * deltaTime);

        UpdateCrossHair();
        UpdateHitMarkers();

        if (damageDealtThisFrame == false)
        {
            timeSinceLastHitMarker += deltaTime;
        }
        damageDealtThisFrame = false;

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1))
        {
            OnDamageDealt(debugToHitDamage, HitTypeResult.Normal);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            OnDamageDealt(debugToHitDamage, HitTypeResult.HeadShot);
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            OnDamageDealt(debugToHitDamage, HitTypeResult.HeadShotKill);
        }
#endif
    }


    #region Update CrossHair and HitMarkers

    private void UpdateCrossHair()
    {
        float crossHairOffset = math.lerp(crossHairOffsetMinMax.min, crossHairOffsetMinMax.max, crossHairOffset01);
        for (int i = 0; i < crossHairLines.Length; i++)
        {
            crossHairLines[i].transform.localPosition = new Vector3(crossHairLineDirs[i].x, crossHairLineDirs[i].y, 0) * crossHairOffset;
        }
    }
    private void UpdateHitMarkers()
    {
        float hitMarkerOffset = math.lerp(hitMarkerOffsetMinMax.min, hitMarkerOffsetMinMax.max, hitMarkerOffset01);

        hitMarkerColor.a = math.lerp(1, 0, math.clamp(timeSinceLastHitMarker - hitMarkerDuration, 0, 1) / hitMarkerDuration);

        for (int i = 0; i < 4; i++)
        {
            hitMarkers[i].color = hitMarkerColor;
            hitMarkers[i].transform.localPosition = new Vector3(hitMarkerDirs[i].x, hitMarkerDirs[i].y, 0) * hitMarkerOffset;
        }
    }
    private void SetHitMarkersColor(HitTypeResult hitTypeResult)
    {
        bool kill = hitTypeResult.HasFlag(HitTypeResult.NormalKill) || hitTypeResult.HasFlag(HitTypeResult.HeadShotKill);
        bool headShot = hitTypeResult == HitTypeResult.HeadShot;

        float alpha = hitMarkerColor.a;

        // check for kill, then headshot then normal color
        hitMarkerColor = kill ? hitMarkerKillColor : headShot ? hitMarkerCritColor : hitMarkerHitColor;
        hitMarkerColor.a = alpha;
    }

    #endregion


    public override void OnDestroy()
    {
        UpdateScheduler.UnRegisterUpdate(OnUpdate);

        if (IsOwner)
        {
#pragma warning disable UDR0004
            PlayerHealthHandler.OnDamageDealt -= OnDamageDealt;
#pragma warning restore UDR0004
        }
        else
        {
#pragma warning disable UDR0004
            PlayerHealthHandler.OnDamageRecieved -= OnDamageRecieved;
#pragma warning restore UDR0004
        }

        base.OnDestroy();
    }


#if UNITY_EDITOR

    private void OnValidate()
    {
        if (crossHairPlus != null)
        {
            crossHairPlus.color = crossHairColor;
        }


        if (crossHairLines[0] != null)
        {
            UpdateCrossHair();
        }
        if (hitMarkers[0] != null)
        {
            UpdateHitMarkers();
        }
    }

    [Header("DEBUG")]
    [SerializeField] private float debugToHitDamage;
#endif
}
