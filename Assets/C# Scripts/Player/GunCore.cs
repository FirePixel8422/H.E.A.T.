using FirePixel.Networking;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


public class GunCore : NetworkBehaviour
{
    [SerializeField] private Transform gunHolder;

    private GunRefHolder gunRefHolder;
    private PlayerController playerController;
    private int gunLayer;

    [Header("Data Driven Gun Parts")]
    [SerializeField] private GunCoreStats coreStats;

    [SerializeField] private NetworkValue<int> currentGunId = new NetworkValue<int>();

    [SerializeField] private CameraHandler camHandler;
    [SerializeField] private ADSHandler adsHandler;
    [SerializeField] private RecoilHandler recoilHandler;
    [SerializeField] private HeatSinkHandler heatSinkHandler;
    [SerializeField] private GunShakeHandler gunShakeHandler;
    [SerializeField] private GunSwayHandler gunSwayHandler;
    [SerializeField] private GunEmmisionHandler gunEmmisionHandler;

    [Header("Additional Refs")]
    [SerializeField] private AudioSource gunShotSource;
    [SerializeField] private AudioSource gunOverheatSource;

    private Camera mainCam;

    private bool CanShoot => timeSinceLastShot >= coreStats.ShootInterval;

    private float inputBufferTimer;
    private bool ShootInputBuffered => shootButtonHeld || inputBufferTimer > 0f;

    private bool shootButtonHeld;

    private float timeSinceLastShot;
    private float timeSinceShootButtonPress;

    private int burstShotsLeft = 0;
    private float burstShotTimer = 0f;

    [Tooltip("multiplier to counter the offset of the DecalProjector Component's pivot, so the decal still sticks to the surface correctly")]
    public const float DecalProjectorPivotMultiplier = 0.0125f;


    #region Input Callbacks

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        // Get the shoot button state (held true or released false)
        shootButtonHeld = ctx.ReadValueAsButton();

        if (shootButtonHeld)
        {
            inputBufferTimer = coreStats.InputBufferTime;
            timeSinceShootButtonPress = math.min(timeSinceLastShot, coreStats.ShootInterval);
        }
    }
    public void OnADS(InputAction.CallbackContext ctx)
    {
        adsHandler.OnZoomInput(ctx.performed);
    }

    public void OnMouseMovement(InputAction.CallbackContext ctx)
    {
        recoilHandler.OnMouseMovement(ctx.ReadValue<Vector2>());
    }

    #endregion


    #region Initialization

    private void OnEnable() => ManageUpdateCallbacks(true);
    private void OnDisable() => ManageUpdateCallbacks(false);

    public override void OnNetworkSpawn()
    {
        currentGunId = new NetworkValue<int>(0);
        if (IsOwner)
        {
            currentGunId.OnValueChanged += (int newValue) =>
            {
                UpdateGunData_ServerRPC(newValue);
            };
        }
        else
        {
            RequestGunData_ServerRPC();
        }

        Init();
        ManageUpdateCallbacks(true);
    }

#if UNITY_EDITOR
    private void Start()
    {
        if (overrideIsOwner)
        {
            Init();
            ManageUpdateCallbacks(true);
        }
    }
#endif

    private bool registeredForUpdates = false;
    private void ManageUpdateCallbacks(bool register)
    {
#if UNITY_EDITOR
        if ((IsOwner && IsSpawned) || overrideIsOwner)
#else
        if (IsOwner && IsSpawned)
#endif
        {
            if (registeredForUpdates == register) return;

            UpdateScheduler.ManageUpdate(OnUpdate, register);
            UpdateScheduler.ManageFixedUpdate(OnFixedUpdate, register);
            registeredForUpdates = register;
        }
    }

    private void Init()
    {
        ManageUpdateCallbacks(true);

        recoilHandler.Init(camHandler);
        adsHandler.Init(camHandler);
        gunEmmisionHandler.Init();
        gunShakeHandler.Init();

        playerController = GetComponent<PlayerController>();
        playerController.Init(camHandler, gunSwayHandler);


#if UNITY_EDITOR
        if (IsOwner || overrideIsOwner)
        {
#else
        if (IsOwner)
        {
#endif
            gunLayer = LayerMask.NameToLayer("Gun");
            gunHolder.gameObject.layer = gunLayer;

            SwapGun(0);

            mainCam = camHandler.MainCamera;
            DecalVfxManager.Instance.Init(mainCam);
        }
    }

#endregion


    #region Swapping Gun

    private void SwapGun(int gunId)
    {
        currentGunId.Value = gunId;

        SetupNewGunData(gunId);

        // set gun to always in front layer ("Gun")
        gunRefHolder.gameObject.layer = gunLayer;
        foreach (Transform child in gunRefHolder.transform.GetAllChildren())
        {
            child.gameObject.layer = gunLayer;
        }

        timeSinceLastShot = 0;
        burstShotTimer = coreStats.burstShotInterval;

        gunEmmisionHandler.UpdateHeatEmission(0);
        UpdateVisualHeatEmmision_ServerRPC(GameIdRPCTargets.SendToOppositeOfLocalClient(), 0);
    }

    [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void RequestGunData_ServerRPC()
    {
        RequestGunData_ClientRPC();
    }
    [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void RequestGunData_ClientRPC()
    {
        // Let the owner send GunId to the requesting client
        if (IsOwner == false) return;

        UpdateGunData_ServerRPC(currentGunId.Value);
    }

    [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void UpdateGunData_ServerRPC(int gunId)
    {
        RecieveGunData_ClientRPC(gunId);
    }
    [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void RecieveGunData_ClientRPC(int gunId)
    {
        // Skip Owner, they already have the correct gun
        if (IsOwner) return;

        SetupNewGunData(gunId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetupNewGunData(int gunId)
    {
        DisposeGunData();

        GunManager.Instance.SwapGun(gunHolder, gunId, IsOwner,
            ref gunRefHolder,
            out coreStats, 
            out heatSinkHandler.stats,
            out gunShakeHandler.stats,
            out gunSwayHandler.stats,
            out adsHandler.stats);

        adsHandler.SwapGun();
        gunSwayHandler.SwapGun(gunRefHolder.transform, adsHandler);
        gunEmmisionHandler.SwapGun(gunRefHolder.EmissionMatInstance);
    }

    #endregion


    private void OnUpdate()
    {
        // TEMP SWAP TO NEXT GUN FUNCTION
        if (Input.GetKeyDown(KeyCode.V))
        {
            int gunId = GunManager.Instance.GetNextGunId();

            SwapGun(gunId);

            MessageHandler.Instance.SendTextLocal("Swapped to: " + GunManager.Instance.GetCurrentGunName());
        }
        // TEMP




        float deltaTime = Time.deltaTime;
        float adsPercentage = adsHandler.ZoomedInPercent;

        if (heatSinkHandler.Overheated)
        {
            // Reset burst shots immediately when overheated
            burstShotsLeft = 0;
        }
        else
        {
            if (ShootInputBuffered)
            {
                timeSinceShootButtonPress += deltaTime;
            }

            ProcessBurstShots(deltaTime, adsPercentage);
        }

        ProcessShooting(adsPercentage);
        ProcessRecoilSpreadAndHeat(deltaTime, adsPercentage);

        recoilHandler.OnUpdate(coreStats.GetRecoilForce(adsPercentage) * deltaTime);
        gunShakeHandler.OnUpdate(deltaTime, adsPercentage);
        adsHandler.OnUpdate(deltaTime);

        if (coreStats.autoFire == false)
        {
            shootButtonHeld = false;
        }

        timeSinceLastShot += deltaTime;
        inputBufferTimer -= deltaTime;
    }

    private void OnFixedUpdate()
    {
#if UNITY_EDITOR
        if (IsOwner == false && overrideIsOwner == false) return;
#else
        if (IsOwner == false) return;
#endif

        if (gunEmmisionHandler != null)
        {
            float heatPercent = heatSinkHandler.HeatPercentage;

            gunEmmisionHandler.UpdateHeatEmission(heatPercent);
            UpdateVisualHeatEmmision_ServerRPC(GameIdRPCTargets.SendToOppositeOfLocalClient(), heatPercent);
        }
    }


    #region Process Shots, Burst Shots And Recoil + Heat

    private void ProcessBurstShots(float deltaTime, float adsPercentage)
    {
        if (burstShotsLeft <= 0) return;

        burstShotTimer -= deltaTime;

        // Burst shots fire at fixed intervals; this loop handles multiple shots if lagged behind
        while (burstShotTimer <= 0 && burstShotsLeft > 0)
        {
            PrepareShot(coreStats.projectileCount, adsPercentage);
            burstShotsLeft--;
            burstShotTimer += coreStats.burstShotInterval;
        }

        if (burstShotsLeft == 0)
        {
            burstShotTimer = coreStats.burstShotInterval; // Reset timer for next burst
        }
    }

    private void ProcessShooting(float adsPercentage)
    {
        if (ShootInputBuffered && CanShoot && !heatSinkHandler.Overheated)
        {
            // Handles catch-up shots if input was buffered longer than shoot interval
            while (timeSinceShootButtonPress >= coreStats.ShootInterval)
            {
                PrepareShot(coreStats.projectileCount, adsPercentage);
                timeSinceShootButtonPress -= coreStats.ShootInterval;
                timeSinceLastShot = 0;

                burstShotsLeft += coreStats.burstShots; // Queue burst shots for each fired shot
            }
        }
    }

    private void ProcessRecoilSpreadAndHeat(float deltaTime, float adsPercentage)
    {
        // Only stabilize recoil if no shots are currently firing
        if (timeSinceLastShot > coreStats.recoilRecoveryDelay && burstShotsLeft == 0)
        {
            recoilHandler.StabilizeRecoil(coreStats.GetRecoilRecovery(adsPercentage) * deltaTime);

            coreStats.StabilizeRecoil(deltaTime);

            // Stabilize hip fire if not firing shots
            coreStats.StabilizeHipFire(1, deltaTime);
        }
        else
        {
            // Stabilize hip fire if firing, based on adsPercentage
            coreStats.StabilizeHipFire(adsPercentage, deltaTime);
        }


        // Send -1 to heatSinkHandler if burst is ongoing to indicate gun isn't idle yet, otherwise send timeSinceLastShot
        heatSinkHandler.UpdateHeatSink(burstShotsLeft == 0 ? timeSinceLastShot : -1, deltaTime);
    }

    #endregion


    #region Shooting Part

    /// <summary>
    /// Inbetween method for Shoot method for ServerRPC logic
    /// </summary>
    private void PrepareShot(int projectileCount, float adsPercentage)
    {
        gunShakeHandler.AddShake(coreStats.ShootInterval, adsPercentage);

        //float2 recoil = new float2(0, coreStats.GetHipFireRecoil(shootingIntensity));
        float2 recoil = coreStats.GetRecoil(adsPercentage);
        StartCoroutine(recoilHandler.AddRecoil(recoil, coreStats.ShootInterval));

        heatSinkHandler.AddHeat(coreStats.heatPerShot, out float prevHeatPercentage, out float newHeatPercentage);

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        // To Gun Local Space
        Vector3 localOrigin = gunRefHolder.transform.InverseTransformPoint(ray.origin);
        Vector3 localDir = gunRefHolder.transform.InverseTransformDirection(ray.direction);

        // Offset
        Vector3 targetOrigin = localOrigin + gunShakeHandler.ShakeTransform.localPosition + gunRefHolder.transform.localPosition;
        Vector3 targetDir = gunShakeHandler.ShakeTransform.localRotation * gunRefHolder.transform.localRotation * localDir;

        // Ads/Hip Lerp
        localOrigin = Vector3.Lerp(localOrigin, targetOrigin, adsPercentage);
        localDir = Vector3.Lerp(localDir, targetDir.normalized, adsPercentage);

        // Assign
        ray.origin = gunRefHolder.transform.TransformPoint(localOrigin);
        ray.direction = gunRefHolder.transform.TransformDirection(localDir).normalized;

        Vector3 gunRight = gunRefHolder.transform.right;
        Vector3 gunUp = gunRefHolder.transform.up;

        float DEBUG_damageThisShot = 0;

        BulletHoleMessage[] bulletHoleMessages = new BulletHoleMessage[projectileCount];

        for (int i = 0; i < projectileCount; i++)
        {
            float2 spreadOffset = RandomApproxPointInCircle(coreStats.GetSpread(adsPercentage));

            Vector3 rayDirWithSpread = math.normalize(ray.direction + gunRight * spreadOffset.x + gunUp * spreadOffset.y);

            // Shoot an invisble sphere to detect a hit
            if (Physics.SphereCast(ray.origin, coreStats.bulletSize, rayDirWithSpread, out RaycastHit hit))
            {
                if (hit.transform.TryGetComponent(out PlayerHealthHandler player))
                {
                    float damage = coreStats.GetDamageOutput(hit.distance, false);

                    player.DealDamage(damage, hit.point, ray.direction);
                }

                // Deal damage to hit player
                DEBUG_damageThisShot += coreStats.GetDamageOutput(hit.distance, false);


                #region BulletHole FX

                Vector3 hitNormal = hit.normal;

                SurfaceType hitSurfaceType = hit.collider.GetSurfaceType();

                Vector3 bulletHolePos = hit.point;

                // If rayCast can hit a surface, use raycast hitPoint for bulletHole position, otherwise fallback to sphereCast hitPoin
                if (Physics.Raycast(ray.origin, rayDirWithSpread, out hit))
                {
                    hitNormal = hit.normal;
                    bulletHolePos = hit.point;
                }

                // Add spread to bullet hole position
                bulletHolePos += rayDirWithSpread * DecalProjectorPivotMultiplier;

                // Rotation for the bullet hole so it "sticks" to the hit surface, also rotate it randomly around the normal axis
                Quaternion directionRotation = Quaternion.LookRotation(rayDirWithSpread);
                // Rotation based on surface normal
                Quaternion normalRotation = Quaternion.LookRotation(-hitNormal);

                // Blend rotation between direction and normal, and add a random rotation around the Z-axis
                Quaternion bulletHoleRotation = Quaternion.Slerp(directionRotation, normalRotation, 0.5f) * Quaternion.Euler(0, 0, EzRandom.RandomRotationAxis());

                // Set scale
                float bulletHoleScale = EzRandom.Range(coreStats.bulletHoleFXSize);

                bulletHoleMessages[i] = new BulletHoleMessage(bulletHolePos, bulletHoleRotation, bulletHoleScale, hitSurfaceType, coreStats.bulletHoleFXLifetime);

                #endregion
            }
        }

        if (heatSinkHandler.Overheated)
        {
            gunOverheatSource.PlayClipWithPitch(coreStats.overHeatAudioClip, EzRandom.Range(coreStats.overHeatMinMaxPitch));
        }

        // Call shoot method through the server and all clients, except self > call shoot locally
        Shoot_ServerRPC(ClientManager.LocalClientGameId, bulletHoleMessages);
        ShootEffects(bulletHoleMessages);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateVisualHeatEmmision_ServerRPC(GameIdRPCTargets rpcTargets, float percent)
    {
        UpdateVisualHeatEmmision_ClientRPC(rpcTargets, percent);
    }
    [ClientRpc(RequireOwnership = false)]
    private void UpdateVisualHeatEmmision_ClientRPC(GameIdRPCTargets rpcTargets, float percent)
    {
        if (rpcTargets.IsTarget == false) return;

        gunEmmisionHandler.UpdateHeatEmission(percent);
    }

    private float2 RandomApproxPointInCircle(float radius)
    {
        float2 dir = math.normalize(new float2(EzRandom.Range01() * 2f - 1f, EzRandom.Range01() * 2f - 1f));
        float dist = EzRandom.Range01() * radius;
        return dir * dist;
    }

    [ServerRpc(RequireOwnership = false)]
    private void Shoot_ServerRPC(int fromClientGameId, BulletHoleMessage[] bulletHoleMessages)
    {
        Shoot_ClientRPC(bulletHoleMessages, GameIdRPCTargets.SendToOppositeClient(fromClientGameId));
    }

    [ClientRpc(RequireOwnership = false)]
    private void Shoot_ClientRPC(BulletHoleMessage[] bulletHoleMessages, GameIdRPCTargets rpcTargets)
    {
        if (rpcTargets.IsTarget == false) return;

        ShootEffects(bulletHoleMessages);
    }

    /// <summary>
    /// Method to actually fire a shot
    /// </summary>
    private void ShootEffects(BulletHoleMessage[] bulletHoleMessages)
    {
        int randomAudioId = EzRandom.Range(0, coreStats.shootAudioClips.Length);
        float randomPitch = EzRandom.Range(MathLogic.Lerp(coreStats.minMaxPitch, coreStats.minMaxPitchAtMaxHeat, heatSinkHandler.HeatPercentage));

        gunShotSource.PlayClipWithPitch(coreStats.shootAudioClips[randomAudioId], randomPitch);

        // Create Decal trhough DecalVfxManager
        DecalVfxManager.Instance.RegisterDecal(bulletHoleMessages);
    }

    #endregion


    public override void OnDestroy()
    {
        UpdateScheduler.UnRegisterUpdate(OnUpdate);
        UpdateScheduler.UnRegisterFixedUpdate(OnFixedUpdate);

        DisposeGunData();
        DisposeHandlerData();

        currentGunId.OnValueChanged = null;

        base.OnDestroy();
    }
    private void DisposeGunData()
    {
        coreStats.Dispose();
        gunSwayHandler.Dispose();
    }
    private void DisposeHandlerData()
    {
        adsHandler.Dispose();
    }




#if UNITY_EDITOR
    [SerializeField] private bool overrideIsOwner;
#endif
}
