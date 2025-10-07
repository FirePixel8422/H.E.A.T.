using Unity.Netcode;



public class PlayerDataLibrary : NetworkBehaviour
{
    public static PlayerDataLibrary LocalInstance { get; private set; }

    public GunHandler gunHandler;
    public PlayerHealthHandler healthHandler;
    public PlayerController controller;
    public PlayerHotBarHandler hotBarHandler;
    public PlayerHUDHandler hudHandler;
    public RagDollController ragDollController;
    //public UtilityHandler utilityHandler;


    private void Start()
    {
        gunHandler = GetComponent<GunHandler>();
        healthHandler = GetComponent<PlayerHealthHandler>();
        controller = GetComponent<PlayerController>();
        hotBarHandler = GetComponent<PlayerHotBarHandler>();
        hudHandler = GetComponent<PlayerHUDHandler>();
        //utilityHandler = GetComponent<UtilityHandler>();

        ragDollController = GetComponentInChildren<RagDollController>(true);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }
    }
}