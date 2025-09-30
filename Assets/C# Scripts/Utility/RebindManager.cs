using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private InputActionReference cancelRebindAction;

    private InputActionRebindingExtensions.RebindingOperation currentOperation;

    [Tooltip("Path to rebind save file)")]
    private const string RebindsFilePath = "Input/Rebinds";

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [SerializeField] private bool logRebindOperations = true;
#endif




    private void OnEnable()
    {
        cancelRebindAction.action.performed += OnCancelRebind;
        cancelRebindAction.action.Enable();
    }
    private void OnDisable()
    {
        cancelRebindAction.action.performed -= OnCancelRebind;
        cancelRebindAction.action.Disable();
    }
    private void OnCancelRebind(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            CancelRebind();
        }
    }

    private async void Start()
    {
        await LoadRebindsAsync();
    }


    public void StartRebind(string actionName, int bindingIndex)
    {
        if (inputActions.TryFindAction(actionName, out InputAction action))
        {
            action.Disable();

            currentOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            .OnCancel(op =>
            {
                if (logRebindOperations) DebugLogger.Log($"Rebind for {actionName} canceled.");

                action.Enable();
                op.Dispose();
                currentOperation = null;
            })
            .OnComplete(op =>
            {
                if (logRebindOperations) DebugLogger.Log($"Rebound {actionName} to {action.bindings[bindingIndex].effectivePath}");

                action.Enable();
                op.Dispose();
                currentOperation = null;
                _ = SaveRebindsAsync();
            })
            .Start();
        }
    }
    public void CancelRebind()
    {
        if (currentOperation != null)
        {
            currentOperation.Cancel();
            currentOperation.Dispose();
            currentOperation = null;
        }
    }


    #region Save, Load and Reset Rebinds

    private async Task LoadRebindsAsync()
    {
        (bool success, ValueWrapper<string> rebindJson) = await FileManager.LoadInfoAsync<ValueWrapper<string>>(RebindsFilePath);
        if (success && !string.IsNullOrEmpty(rebindJson.Value))
        {
            inputActions.LoadBindingOverridesFromJson(rebindJson.Value);
        }
    }
    private async Task SaveRebindsAsync()
    {
        string json = inputActions.SaveBindingOverridesAsJson();
        await FileManager.SaveInfoAsync(new ValueWrapper<string>(json), RebindsFilePath);
    }

    public void ResetRebinds()
    {
        inputActions.RemoveAllBindingOverrides();

        bool success = FileManager.TryDeleteFile(RebindsFilePath);


#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (logRebindOperations)
        {
            if (success)
            {
                DebugLogger.Log("Rebinds reset and rebind file deleted.");
            }
            else
            {
                DebugLogger.Log("Rebinds reset but no rebind file found to delete.");
            }
        }
#endif
    }

    #endregion


    private void OnDestroy()
    {
        CancelRebind();
    }
}
