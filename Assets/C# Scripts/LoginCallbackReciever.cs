using UnityEngine;
using UnityEngine.Events;


namespace FirePixel.Networking
{
    public class LoginCallbackReciever : MonoBehaviour
    {
        [Header("Called after network login is completed (ALMOST INSTANT)")]
        public UnityEvent onLoginCompleted;
    }
}