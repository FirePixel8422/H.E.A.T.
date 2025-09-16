using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using FirePixel.Networking;
using System.Runtime.CompilerServices;



public class GunCore : NetworkBehaviour
{
    [SerializeField] private Transform gunParentTransform;
    [SerializeField] private GunRefHolder gunRefHolder;

    [Header("Data Driven Gun Parts")]
    [SerializeField] private GunStatsSO gunStatsSO;
    [SerializeField] private GunCoreStats coreStats;

    [SerializeField] private CameraHandler camHandler;
    [SerializeField] private RecoilHandler recoilHandler;
    [SerializeField] private HeatSink heatSink;
    [SerializeField] private GunShakeHandler gunShakeHandler;
    [SerializeField] private GunVisualHandler gunEmmisionHandler;

    [Header("Additional Refs")]
    [SerializeField] private Transform shootPointTransform;
    [SerializeField] private AudioSource gunShotSource;
    [SerializeField] private AudioSource gunOverheatSource;

    private Camera mainCam;

    private bool CanShoot => timeSinceLastShot >= coreStats.ShootInterval;

    private float inputBufferTimer;
    private bool ShootInputBuffered => shootButtonHeld || inputBufferTimer > 0f;

    private bool shootButtonHeld;
    private bool adsButtonHeld;
    private bool IsScopedIn => adsButtonHeld && heatSink.Overheated == false;

    private float timeSinceLastShot;
    private float timeSinceShootButtonPress;
    [SerializeField] private float shootingIntensity;

    private int burstShotsLeft = 0;
    private float burstShotTimer = 0f;

    [Tooltip("multiplier to counter the offset of the DecalProjector Component's pivot, so the decal still sticks to the surface correctly")]
    public const float DecalProjectorPivotMultiplier = 0.0125f;


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
        // Get the shoot button state (held true or released false)
        adsButtonHeld = ctx.ReadValueAsButton();
    }

    public void OnMouseMovement(InputAction.CallbackContext ctx)
    {
        recoilHandler.OnMouseMovement(ctx.ReadValue<Vector2>());
    }


    private void OnEnable()
    {
        UpdateScheduler.RegisterUpdate(OnUpdate);
        UpdateScheduler.RegisterFixedUpdate(OnFixedUpdate);
    }
    private void OnDisable()
    {
        UpdateScheduler.UnregisterUpdate(OnUpdate);
        UpdateScheduler.UnregisterFixedUpdate(OnFixedUpdate);
    }

    public override void OnNetworkSpawn()
    {
#if UNITY_EDITOR
        if (overrideIsOwner) return;
#endif

        if (IsOwner)
        {
            mainCam = GetComponentInChildren<Camera>();
            DecalVfxManager.Instance.Init(mainCam);

            gunShakeHandler.Init(gunParentTransform);
            gunEmmisionHandler.Init();

            recoilHandler.Init(camHandler);
            GetComponent<PlayerController>().Init(camHandler);

            SwapGun(0);
        }

    }

#if UNITY_EDITOR
    private void Start()
    {
        if (overrideIsOwner)
        {
            mainCam = GetComponentInChildren<Camera>();
            DecalVfxManager.Instance.Init(mainCam);

            gunShakeHandler.Init(gunParentTransform);
            gunEmmisionHandler.Init();

            SwapGun(0);
        }
    }
#endif


    #region SwapGun

    private void SwapGun(int gunId)
    {
        SetupNewGunData(gunId);
        SwapGun_ServerRPC(ClientManager.LocalClientGameId, gunId);

        timeSinceLastShot = coreStats.ShootInterval;
        burstShotTimer = coreStats.burstShotInterval;

        UpdateVisualHeatEmmision_ServerRPC(0);
    }

    [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void SwapGun_ServerRPC(int forClientGameId, int gunId)
    {
        SwapGun_ClientRPC(gunId, GameIdRPCTargets.SendToOppositeClient(forClientGameId));
    }

    [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    private void SwapGun_ClientRPC(int gunId, GameIdRPCTargets rpcTargets)
    {
        if (rpcTargets.IsTarget == false) return;

        SetupNewGunData(gunId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetupNewGunData(int gunId)
    {
        GunManager.Instance.SwapGun(gunParentTransform, gunId, IsOwner, ref gunRefHolder, out coreStats, out heatSink.stats, out gunShakeHandler.stats);
        gunEmmisionHandler.SwapGun(gunRefHolder.EmissionMatInstance);
    }

    #endregion


    private void OnUpdate()
    {
#if UNITY_EDITOR
        if (IsOwner == false && overrideIsOwner == false) return;
#else
        if (IsOwner == false) return;
#endif

        if (Input.GetKeyDown(KeyCode.V))
        {
            GunManager.Instance.SwapToNextGun(gunParentTransform, IsOwner, ref gunRefHolder, out coreStats, out heatSink.stats, out gunShakeHandler.stats, out int gunId);
            gunEmmisionHandler.SwapGun(gunRefHolder.EmissionMatInstance);

            timeSinceLastShot = coreStats.ShootInterval;
            burstShotTimer = coreStats.burstShotInterval;

            SwapGun_ServerRPC(ClientManager.LocalClientGameId, gunId);
        }

        float deltaTime = Time.deltaTime;

        if (heatSink.Overheated)
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

            ProcessBurstShots(deltaTime);
        }

        ProcessShooting();
        ProcessRecoilAndHeat(deltaTime);

        recoilHandler.OnUpdate(coreStats.recoilForce * deltaTime);
        gunShakeHandler.OnUpdate(deltaTime);

        if (coreStats.autoFire == false)
        {
            shootButtonHeld = false;
        }

        timeSinceLastShot += deltaTime;
        inputBufferTimer -= deltaTime;
    }











    /// <summary>
    /// FIX THIS
    /// FIX THIS
    /// FIX THIS
    /// FIX THIS
    /// FIX THIS
    /// FIX THIS
    /// FIX THIS
    /// </summary>
    /// //UPDATE HAS TO BE AFTER UPDATE NOT BEFROE
    /// 
    //ALSO UPDATE UPDATE LOGIC SCHEDULEMENT LIKE ITS DONE IN PLAYERMOVEMENT
    //DONT UPDATE FOR NON OWNERS

    private void OnFixedUpdate()
    {
#if UNITY_EDITOR
        if (IsOwner == false && overrideIsOwner == false) return;
#else
        if (IsOwner == false) return;
#endif

        if (gunEmmisionHandler != null)
        {
            float heatPercent = heatSink.HeatPercentage;

            UpdateVisualHeatEmmision_ServerRPC(heatPercent);
            gunEmmisionHandler.UpdateHeatEmission(heatPercent);
        }
    }


    #region Process Shots, Burst Shots And Recoil + Heat

    private void ProcessBurstShots(float deltaTime)
    {
        if (burstShotsLeft <= 0) return;

        burstShotTimer -= deltaTime;

        // Burst shots fire at fixed intervals; this loop handles multiple shots if lagged behind
        while (burstShotTimer <= 0 && burstShotsLeft > 0)
        {
            PrepareShot(coreStats.projectileCount);
            burstShotsLeft--;
            burstShotTimer += coreStats.burstShotInterval;
        }

        if (burstShotsLeft == 0)
        {
            burstShotTimer = coreStats.burstShotInterval; // Reset timer for next burst
        }
    }

    private void ProcessShooting()
    {
        if (ShootInputBuffered && CanShoot && !heatSink.Overheated)
        {
            // Handles catch-up shots if input was buffered longer than shoot interval
            while (timeSinceShootButtonPress >= coreStats.ShootInterval)
            {
                PrepareShot(coreStats.projectileCount);
                timeSinceShootButtonPress -= coreStats.ShootInterval;
                timeSinceLastShot = 0;

                burstShotsLeft += coreStats.burstShots; // Queue burst shots for each fired shot
            }
        }
    }

    private void ProcessRecoilAndHeat(float deltaTime)
    {
        // Only stabilize recoil if no burst is currently firing
        if (timeSinceLastShot > coreStats.recoilRecoveryDelay && burstShotsLeft == 0)
        {
            recoilHandler.StabilizeRecoil(coreStats.recoilRecovery * deltaTime);

            coreStats.DecreaseADSRecoil(deltaTime);

            shootingIntensity -= deltaTime / coreStats.shootIntensityDescreaseMultplier;
            if (shootingIntensity < 0f)
            {
                shootingIntensity = 0f;
            }
        }

        // Send -1 to heatSink if burst is ongoing to indicate gun isn't idle yet, otherwise send timeSinceLastShot
        heatSink.UpdateHeatSink(burstShotsLeft == 0 ? timeSinceLastShot : -1, deltaTime);
    }

    #endregion


    #region Shooting Part

    /// <summary>
    /// Inbetween method for Shoot method for ServerRPC logic
    /// </summary>
    private void PrepareShot(int projectileCount)
    {
        shootingIntensity += coreStats.ShootInterval / coreStats.shootIntensityGainMultplier;

        gunShakeHandler.AddShake(coreStats.ShootInterval);

        //float2 recoil = new float2(0, coreStats.GetHipFireRecoil(shootingIntensity));
        float2 recoil = coreStats.GetADSRecoil();
        StartCoroutine(recoilHandler.AddRecoil(recoil, coreStats.ShootInterval));

        heatSink.AddHeat(coreStats.heatPerShot, out float prevHeatPercentage, out float newHeatPercentage);

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        Vector3 mainCamRight = mainCam.transform.right;
        Vector3 mainCamUp = mainCam.transform.up;

        float DEBUG_damageThisShot = 0;

        BulletHoleMessage[] bulletHoleMessages = new BulletHoleMessage[projectileCount];

        for (int i = 0; i < projectileCount; i++)
        {
            float2 spreadOffset = float2.zero;// RandomApproxPointInCircle(coreStats.GetHipFireSpread(previousHeatPercentage));

            Vector3 rayDirWithSpread = math.normalize(ray.direction + mainCamRight * spreadOffset.x + mainCamUp * spreadOffset.y);

            // Shoot an invisble sphere to detect a hit
            if (Physics.SphereCast(ray.origin, coreStats.bulletSize, rayDirWithSpread, out RaycastHit hit))
            {
                Vector3 hitNormal = hit.normal;

                // Deal damage to hit player
                DEBUG_damageThisShot += coreStats.GetDamageOutput(hit.distance, false);


                #region BulletHole FX

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

        if (heatSink.Overheated)
        {
            gunOverheatSource.PlayClipWithPitch(coreStats.overHeatAudioClip, EzRandom.Range(coreStats.overHeatMinMaxPitch));
        }

        // Call shoot method through the server and all clients, except self > call shoot locally
        Shoot_ServerRPC(ClientManager.LocalClientGameId, bulletHoleMessages);
        ShootEffects(bulletHoleMessages);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateVisualHeatEmmision_ServerRPC(float percent)
    {
        UpdateVisualHeatEmmision_ClientRPC(GameIdRPCTargets.SendToOppositeClient(ClientManager.LocalClientGameId), percent);
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

    /// <summary>
    /// Method to actually fire a shot
    /// </summary>
    private void ShootEffects(BulletHoleMessage[] bulletHoleMessages)
    {
        int randomAudioId = EzRandom.Range(0, coreStats.shootAudioClips.Length);
        float randomPitch = EzRandom.Range(MathLogic.Lerp(coreStats.minMaxPitch, coreStats.minMaxPitchAtMaxHeat, heatSink.HeatPercentage));

        gunShotSource.PlayClipWithPitch(coreStats.shootAudioClips[randomAudioId], randomPitch);

        // Create Decal trhough DecalVfxManager
        DecalVfxManager.Instance.RegisterDecal(bulletHoleMessages);
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

    #endregion


    public override void OnDestroy()
    {
        base.OnDestroy();

        coreStats.Dispose();
    }


#if UNITY_EDITOR
    [SerializeField] private bool overrideIsOwner = true;

    [SerializeField] private int DEBUG_toSwapGunId = 0;

    [ContextMenu("DEBUG_SwapGun")]
    private void DEBUG_SwapGun()
    {
        SwapGun(DEBUG_toSwapGunId);
    }
#endif
}
