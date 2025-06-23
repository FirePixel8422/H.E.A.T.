using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;



[RequireComponent(typeof(RecoilHandler), typeof(HeatSink))]
public class GunCore : NetworkBehaviour
{
    [SerializeField] private GunCoreStatsSO coreStatsSO;
    [SerializeField] private GunCoreStats coreStats;

    [SerializeField] private Transform shootPoint;
    [SerializeField] private AudioSource gunSource;

    private RecoilHandler recoilHandler;
    private HeatSink heatSink;

    private bool shootButtonHeld;
    private float timeSinceLastShot;
    private float timeSinceShootButtonPress;

    [Tooltip("Whether the gun is allowed too shoot according to coreStats.ShootInterval")]
    private bool CanShoot => timeSinceLastShot >= coreStats.ShootInterval;


    public void OnShoot(InputAction.CallbackContext ctx)
    {
        shootButtonHeld = ctx.ReadValueAsButton();

        if (shootButtonHeld)
        {
            timeSinceShootButtonPress = math.min(timeSinceLastShot, coreStats.ShootInterval);
        }
        else
        {
            timeSinceShootButtonPress = 0;
        }
    }

    private void Start()
    {
        recoilHandler = GetComponent<RecoilHandler>();
        heatSink = GetComponent<HeatSink>();

        coreStats = coreStatsSO.stats;

        timeSinceLastShot = coreStats.ShootInterval;
    }


    private void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    private void OnDisable() => UpdateScheduler.UnregisterUpdate(OnUpdate);


    private void OnUpdate()
    {
        if (shootButtonHeld && heatSink.Overheated == false)
        {
            timeSinceShootButtonPress += Time.deltaTime;
        }

        //when shoot button is held AND gun can shoot according to shootInterval AND heatSink is not overheated,
        if (shootButtonHeld && CanShoot && heatSink.Overheated == false)
        {
            //fire shots and add recoil for every timeSinceLastShot time that exceeds shootInterval (to catch up in cases with HIGH lagg)
            while (timeSinceShootButtonPress >= coreStats.ShootInterval)
            {
                Shoot();

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
        recoilHandler.OnUpdate();

        //if autofire is disabled, auto release in script shootButton
        if (coreStats.autoFire == false)
        {
            shootButtonHeld = false;
        }

        timeSinceLastShot += Time.deltaTime;
    }


    /// <summary>
    /// Actually firing a shot
    /// </summary>
    private void Shoot()
    {
        recoilHandler.AddRecoil(coreStats.recoilPerShot);
        heatSink.AddHeat(coreStats.heatPerShot);

        Shoot_ServerRPC(ClientManager.LocalClientGameId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void Shoot_ServerRPC(int clientGameId)
    {
        Shoot_ClientRPC(clientGameId, EzRandom.Range(0, coreStats.shootAudioClips.Length), EzRandom.Range(coreStats.minMaxPitch));
    }

    [ClientRpc(RequireOwnership = false)]
    private void Shoot_ClientRPC(int clientGameId, int randomAudioId, float randomPitch)
    {
        if (ClientManager.LocalClientGameId == clientGameId) return;

        gunSource.clip = coreStats.shootAudioClips[randomAudioId];
        gunSource.pitch = randomPitch;
        gunSource.Play();
    }
}
