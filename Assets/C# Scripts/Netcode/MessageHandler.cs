using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FirePixel.Networking
{
    public class MessageHandler : NetworkBehaviour
    {
        public static MessageHandler Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
        }

        [SerializeField] private InputActionReference confirmAction;

        [SerializeField] private GameObject textBoxPrefab;
        [SerializeField] private Transform chatContentHolder;

        [SerializeField] private Scrollbar scrollBar;

        [SerializeField] private bool showLocalNameAs_You;
        [SerializeField] private bool active;

        [SerializeField] private Color serverMessagesColor;

        [SerializeField] private float toggleSpeed;

        [SerializeField] private Vector3 enabledPos;
        [SerializeField] private Vector3 disabledPos;

        private TMP_InputField inputField;


        private void OnEnable()
        {
            confirmAction.action.performed += OnConfirm;
            confirmAction.action.Enable();
        }
        private void OnDisable()
        {
            confirmAction.action.performed -= OnConfirm;
            confirmAction.action.Disable();
        }

        public override void OnNetworkSpawn()
        {
            inputField = GetComponentInChildren<TMP_InputField>();
        }

        public void ToggleUI()
        {
            active = !active;
            StartCoroutine(ToggleUITimer(active ? enabledPos : disabledPos));
        }
        private IEnumerator ToggleUITimer(Vector3 pos)
        {
            while (Vector3.Distance(transform.localPosition, pos) > 0.001f)
            {
                yield return null;
                transform.localPosition = VectorLogic.InstantMoveTowards(transform.localPosition, pos, toggleSpeed * Time.deltaTime);
            }
        }
        /// <summary>
        /// Send typed message.
        /// </summary>
        public void OnConfirm(InputAction.CallbackContext ctx)
        {
            //DebugLogger.Log("Confirm action performed " + (active == false || ctx.performed == false || string.IsNullOrEmpty(inputField.text)));
            if (active == false || ctx.performed == false || string.IsNullOrEmpty(inputField.text)) return;

            SendTextGlobal_ServerRPC(ClientManager.LocalClientGameId, ClientManager.LocalUserName, inputField.text);

            inputField.ActivateInputField();
            inputField.text = "";
        }


        /// <summary>
        /// Local chat message sending.
        /// </summary>
        public void SendTextLocal(string message)
        {
            StartCoroutine(AddTextToChatBox(ClientManager.LocalClientGameId, ClientManager.LocalUserName, message));
        }

        [ServerRpc(RequireOwnership = false)]
        public void SendTextGlobal_ServerRPC(int clientGameId, string senderName, string text)
        {
            SendTextGlobal_ClientRPC(clientGameId, senderName, text);
        }
        [ClientRpc(RequireOwnership = false)]
        private void SendTextGlobal_ClientRPC(int clientGameId, string senderName, string text)
        {
            StartCoroutine(AddTextToChatBox(clientGameId, senderName, text));
        }


        [ServerRpc(RequireOwnership = false)]
        public void SendTextToClient_ServerRPC(int clientGameId, string senderName, string text)
        {
            SendTextToClient_ClientRPC(clientGameId, senderName, text);
        }
        [ClientRpc(RequireOwnership = false)]
        private void SendTextToClient_ClientRPC(int clientGameId, string senderName, string text)
        {
            // Send to only "toClientId"
            if (ClientManager.LocalClientGameId != clientGameId) return;

            StartCoroutine(AddTextToChatBox(clientGameId, senderName, text));
        }



        private IEnumerator AddTextToChatBox(int clientGameId, string playerName, string text)
        {
            GameObject obj = Instantiate(textBoxPrefab, chatContentHolder, false);

            TextMeshProUGUI textObj = obj.GetComponent<TextMeshProUGUI>();

            if (clientGameId == ClientManager.LocalClientGameId && showLocalNameAs_You)
            {
                playerName = "You";
            }
            else if (clientGameId == -1)
            {
                obj.GetComponent<TextMeshProUGUI>().color = serverMessagesColor;
            }

            textObj.text = $"[{playerName}]: " + text.ToString();

            //Set RectTransfrom Size to Fit all the text
            Vector2 temp = (textObj.transform as RectTransform).sizeDelta;
            temp.y = textObj.preferredHeight;
            (textObj.transform as RectTransform).sizeDelta = temp;

            yield return null;
            yield return new WaitForEndOfFrame();

            scrollBar.value = 0;
        }
    }
}