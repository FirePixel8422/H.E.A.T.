using System;
using UnityEngine;
using UnityEngine.InputSystem;



public class PlayerHotBarHandler : MonoBehaviour
{
    public Action[] OnSwapToHotbarSlot;

    private PlayerHUDHandler hudHandler;


    private void Awake()
    {
        hudHandler = GetComponent<PlayerHUDHandler>();

        OnSwapToHotbarSlot = new Action[GlobalGameData.HotBarSlotCount];
        for (int i = 0; i < GlobalGameData.HotBarSlotCount; i++)
        {
            int slotId = i;
            OnSwapToHotbarSlot[i] += () => OnSwapToNewHotBarSlot(slotId);
        }
    }


    #region Input Callbacks

    public void OnSwapToHotBarSlot1(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            OnSwapToHotbarSlot[0]?.Invoke();
    }
    public void OnSwapToHotBarSlot2(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            OnSwapToHotbarSlot[1]?.Invoke();
    }
    public void OnSwapToHotBarSlot3(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            OnSwapToHotbarSlot[2]?.Invoke();
    }
    public void OnSwapToHotBarSlot4(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            OnSwapToHotbarSlot[3]?.Invoke();
    }

    #endregion


    public void OnSwapToNewHotBarSlot(int slotId)
    {
        //hudHandler.
    }
}