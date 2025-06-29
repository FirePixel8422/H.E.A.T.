using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;



[RequireComponent(typeof(RecoilHandler), typeof(HeatSink))]
public class GunCore : NetworkBehaviour
{
    [SerializeField] private GunCoreStatsSO coreStatsSO;
    [SerializeField] private HeatSinkStatsSO heatSinkStatsSO;

    [SerializeField] private GunCoreStats coreStats;

    [SerializeField] private Transform shootPointTransform;
    [SerializeField] private AudioSource gunSource;

    [SerializeField] private GameObject bulletHole;

    private RecoilHandler recoilHandler;
    private HeatSink heatSink;

    private bool shootButtonHeld;
    private float timeSinceLastShot;
    private float timeSinceShootButtonPress;

    private bool CanShoot => timeSinceLastShot >= coreStats.ShootInterval;


    private float inputBufferTimer;
    private bool ShootInputBuffered => shootButtonHeld || inputBufferTimer > 0f;


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
        heatSink.Init(heatSinkStatsSO.stats);

        coreStats = coreStatsSO.GetStats();

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
                PrepareShot();

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
    private void PrepareShot()
    {
        recoilHandler.AddRecoil(coreStats.recoilPerShot);
        heatSink.AddHeat(coreStats.heatPerShot, out float previousHeatPercentage);

        float2 spreadOffset = RandomApproxPointInCircle(coreStats.GetSpread(previousHeatPercentage));

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        float3 spreadDir = math.normalize(ray.direction + Camera.main.transform.right * spreadOffset.x + Camera.main.transform.up * spreadOffset.y);

        if (Physics.SphereCast(ray.origin, coreStats.bulletSize, spreadDir, out RaycastHit hit))
        {
            Quaternion bulletHoleRotation = Quaternion.LookRotation(-hit.normal) * Quaternion.Euler(0, 0, EzRandom.RandomRotationAxis());

            GameObject bulletHoleObj = Instantiate(bulletHole, hit.point + hit.normal * EzRandom.Range(0.0005f, 0.001f), bulletHoleRotation);
            bulletHole.transform.localScale = Vector3.one * coreStats.bulletHoleFXSize;

            Destroy(bulletHoleObj, 5);
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
        Shoot_ClientRPC(clientGameId, randomAudioId, randomPitch);
    }

    [ClientRpc(RequireOwnership = false)]
    private void Shoot_ClientRPC(int clientGameId, int randomAudioId, float randomPitch)
    {
        if (ClientManager.LocalClientGameId == clientGameId) return;

        Shoot(randomAudioId, randomPitch);
    }

    #endregion


#if UNITY_EDITOR
    [SerializeField] private bool overrideIsOwner = true;
#endif
}
