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

    private void Start()
    {
        recoilHandler = GetComponent<RecoilHandler>();
        heatSink = GetComponent<HeatSink>();
        cam = GetComponentInChildren<Camera>();

        coreStats = gunStatsSO.GetCoreStats();
        heatSink.Init(gunStatsSO.GetHeatSinkStats());

        timeSinceLastShot = coreStats.ShootInterval;
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

        //count how long shoot button is held down, so we can buffer the input for a short time
        if (ShootInputBuffered && heatSink.Overheated == false)
        {
            timeSinceShootButtonPress += deltaTime;
        }

        //when shoot button is held AND gun can shoot according to shootInterval AND heatSink is not overheated,
        if (ShootInputBuffered && CanShoot && heatSink.Overheated == false)
        {
            //fire shots and add recoil for every timeSinceLastShot time that exceeds shootInterval (to catch up in cases with HIGH lagg)
            while (timeSinceShootButtonPress >= coreStats.ShootInterval)
            {
                PrepareShot(coreStats.projectileCount, coreStats.burstShots);

                timeSinceShootButtonPress -= coreStats.ShootInterval;
                timeSinceLastShot = 0;
            }
        }
        else
        {
            //start recoil recovery if enough time has passed since last shot
            if (timeSinceLastShot > coreStats.recoilRecoveryDelay)
            {
                recoilHandler.StabilizeRecoil(coreStats.recoilRecovery);
            }

            //mark heatsink for idle, heatSink executes the rest of its functionality itself;
            heatSink.OnGunIdle(timeSinceLastShot);
        }

        //Call Update method for recoil handler
        recoilHandler.OnUpdate(coreStats.recoilForce);

        //if autofire is disabled, auto release in script shootButton if inputBuffer window is over
        if (coreStats.autoFire == false)
        {
            shootButtonHeld = false;
        }

        timeSinceLastShot += deltaTime;
        inputBufferTimer -= deltaTime;
    }


    #region Shooting Part

    /// <summary>
    /// Inbetween method for Shoot method for ServerRPC logic
    /// </summary>
    private void PrepareShot(int projectileCount, int burstCount)
    {
        recoilHandler.AddRecoil(coreStats.recoilPerShot);
        heatSink.AddHeat(coreStats.heatPerShot, out float previousHeatPercentage);

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        Vector3 camRight = cam.transform.right;
        Vector3 camUp = cam.transform.up;

        for (int i = 0; i < projectileCount; i++)
        {
            float2 spreadOffset = RandomApproxPointInCircle(coreStats.GetSpread(previousHeatPercentage));

            Vector3 rayDirWithSpread = math.normalize(ray.direction + camRight * spreadOffset.x + camUp * spreadOffset.y);

            // Shoot an invisble sphere to detect a hit
            if (Physics.SphereCast(ray.origin, coreStats.bulletSize, rayDirWithSpread, out RaycastHit hit))
            {
                //Deal damage to hit player




                #region BulletHole FX

                Vector3 bulletHolePos = hit.point;

                //if rayCast can hit a surface, use raycast hitPoint for bulletHole position, otherwise fallback to sphereCast hitPoin
                if (Physics.Raycast(ray.origin, rayDirWithSpread, out hit))
                {
                    bulletHolePos = hit.point;
                }

                // rotation for the bullet hole so it "stick" to the hit surdface, also rotate it randomly around the normal axis
                Quaternion bulletHoleRotation = Quaternion.LookRotation(rayDirWithSpread) * Quaternion.Euler(0, 0, EzRandom.RandomRotationAxis());

                // Instantiate bullet hole at the hit point, slightly offset in the direction of the normal to avoid z-fighting
                GameObject bulletHoleObj = Instantiate(bulletHolePrefab, bulletHolePos + rayDirWithSpread * 0.0125f, bulletHoleRotation);
                // Set scale
                bulletHoleObj.transform.localScale = Vector3.one * coreStats.bulletHoleFXSize;

                // Register bullet hole for destruction after a certain time through BulletHoleManager
                BulletHoleManager.Instance.RegisterBulletHole(bulletHoleObj, coreStats.bulletHoleFXLifetime);

                #endregion
            }
        }

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
