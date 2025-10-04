using UnityEngine;



[System.Serializable]
public class HotBarItem
{
    public Sprite sprite;

    private float cooldownLeft;
    private float lastCoolDownTime;
    private bool wentOnCooldownThisFrame;

    public bool OnCooldown => cooldownLeft == 0;

    /// <summary>
    /// Percentage01 of how completed the cooldown is
    /// </summary>
    public float CoolDownCompletionPercent01 => lastCoolDownTime == 0 ? 1 : lastCoolDownTime / cooldownLeft;


    public void Use()
    {

    }
    public void SetCooldown(float cooldown)
    {
        cooldownLeft = cooldown;
        lastCoolDownTime = cooldown;
        wentOnCooldownThisFrame = true;
    }
    public void DecreaseCooldown(float deltaTime)
    {
        if (wentOnCooldownThisFrame)
        {
            wentOnCooldownThisFrame = false;
            return;
        }

        cooldownLeft = Mathf.MoveTowards(cooldownLeft, 0, deltaTime);
    }
}