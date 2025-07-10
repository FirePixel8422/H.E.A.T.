using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;



[RequireComponent(typeof(RecoilHandler), typeof(HeatSink))]
public class GunCore : NetworkBehaviour
{
    [SerializeField] private GunStatsSO gunStatsSO;

    [SerializeField] private GunCoreStats coreStats;

    [SerializeField] private Transform shootPointTransform;
    [SerializeField] private AudioSource gunSource;

    [SerializeField] private GameObject bulletHolePrefab;

    private RecoilHandler recoilHandler;
    private HeatSink heatSink;
    private Camera cam;

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


    public void OnShoot(InputAction.CallbackContext ctx)
    {
        //get the shoot button state (held true or released false)
        shootButtonHeld = ctx.ReadValueAsButton();

        if (shootButtonHeld)
        {
            inputBufferTimer = coreStats.InputBufferTime;
            timeSinceShootButtonPress = math.min(timeSinceLastShot, coreStats.ShootInterval);
        }
    }





    // REMOVETHIS LATER, EDITOR DEBUUGGGG
    // REMOVETHIS LATER, EDITOR DEBUUGGGG
    // REMOVETHIS LATER, EDITOR DEBUUGGGG
    // REMOVETHIS LATER, EDITOR DEBUUGGGG
    // REMOVETHIS LATER, EDITOR DEBUUGGGG
    // REMOVETHIS LATER, EDITOR DEBUUGGGG
    // REMOVETHIS LATER, EDITOR DEBUUGGGG
    // REMOVETHIS LATER, EDITOR DEBUUGGGG
    // REMOVETHIS LATER, EDITOR DEBUUGGGG
    private void Start()
    {
        OnNetworkSpawn();
    }

    public override void OnNetworkSpawn()
    {
        recoilHandler = GetComponent<RecoilHandler>();
        heatSink = GetComponent<HeatSink>();
        cam = GetComponentInChildren<Camera>();

        coreStats = gunStatsSO.GetCoreStats();
        heatSink.Init(gunStatsSO.GetHeatSinkStats());

        timeSinceLastShot = coreStats.ShootInterval;
        burstShotTimer = coreStats.burstShotInterval;

        DecalVfxManager.Instance.Initialize(cam);
    }


    private void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    private void OnDisable() => UpdateScheduler.UnregisterUpdate(OnUpdate);


    private void OnUpdate()
    {
#if UNITY_EDITOR
        if (IsOwner == false && overrideIsOwner == false) return;
#else
    if (IsOwner == false) return;
#endif

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
        ProcessRecoilAndHeat();

        recoilHandler.OnUpdate(coreStats.recoilForce);

        if (coreStats.autoFire == false)
        {
            shootButtonHeld = false;
        }

        timeSinceLastShot += deltaTime;
        inputBufferTimer -= deltaTime;
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

    private void ProcessRecoilAndHeat()
    {
        // Only stabilize recoil if no burst is currently firing
        if (timeSinceLastShot > coreStats.recoilRecoveryDelay && burstShotsLeft == 0)
        {
            recoilHandler.StabilizeRecoil(coreStats.recoilRecovery);
        }

        // Send -1 to heatSink if burst is ongoing to indicate gun isn't idle yet, otherwise send timeSinceLastShot
        heatSink.OnGunIdle(burstShotsLeft == 0 ? timeSinceLastShot : -1);
    }

    #endregion


    #region Shooting Part

    /// <summary>
    /// Inbetween method for Shoot method for ServerRPC logic
    /// </summary>
    private void PrepareShot(int projectileCount)
    {
        recoilHandler.AddRecoil(coreStats.recoilPerShot);
        heatSink.AddHeat(coreStats.heatPerShot, out float previousHeatPercentage);

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        Vector3 camRight = cam.transform.right;
        Vector3 camUp = cam.transform.up;

        float DEBUG_damageThisShot = 0;

        for (int i = 0; i < projectileCount; i++)
        {
            float2 spreadOffset = RandomApproxPointInCircle(coreStats.GetSpread(previousHeatPercentage));

            Vector3 rayDirWithSpread = math.normalize(ray.direction + camRight * spreadOffset.x + camUp * spreadOffset.y);

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
                Vector3 bulletHoleScale = Vector3.one * EzRandom.Range(coreStats.bulletHoleFXSize);

                // Create Decal trhough DecalVfxManager
                DecalVfxManager.Instance.RegisterDecal(bulletHolePos, bulletHoleRotation, bulletHoleScale, hitSurfaceType, coreStats.bulletHoleFXLifetime);

                #endregion
            }
        }

        print("Shot hit for " + DEBUG_damageThisShot + " damage!");

        int randomAudioId = EzRandom.Range(0, coreStats.shootAudioClips.Length);
        float randomPitch = EzRandom.Range(coreStats.minMaxPitch);

        // Call shoot method on the server and all clients, except self > call shoot locally
        Shoot_ServerRPC(ClientManager.LocalClientGameId, randomAudioId, randomPitch);
        Shoot(randomAudioId, randomPitch);
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
    private void Shoot(int randomAudioId, float randomPitch)
    {
        gunSource.PlayClipWithPitch(coreStats.shootAudioClips[randomAudioId], randomPitch);
    }


    [ServerRpc(RequireOwnership = false)]
    private void Shoot_ServerRPC(int clientGameId, int randomAudioId, float randomPitch)
    {
        Shoot_ClientRPC(randomAudioId, randomPitch, NetcodeUtility.SendToOppositeClient(clientGameId));
    }

    [ClientRpc(RequireOwnership = false)]
    private void Shoot_ClientRPC(int randomAudioId, float randomPitch, ClientRpcParams rpcParams)
    {
        Shoot(randomAudioId, randomPitch);
    }

    #endregion


#if UNITY_EDITOR
    [SerializeField] private bool overrideIsOwner = true;
#endif
}
