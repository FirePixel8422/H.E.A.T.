using UnityEngine;

public class EyeLookAtPlayer : MonoBehaviour
{
    public GameObject player;
    private void Update()
    {
        if(player != null)
        {
            transform.LookAt(player.transform);
        }
    }
}
