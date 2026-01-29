using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public PlayerCombat combat;

    private void OnEnable()
    {
        SkillSlot.OnAbilityPointSpent += HandleAbilityPointSpent;
    }

    private void OnDisable()
    {
        SkillSlot.OnAbilityPointSpent -= HandleAbilityPointSpent;
    }

    private void HandleAbilityPointSpent(SkillSlot slot)
    {
        string skillName = slot.skillSO.skillName;
        switch(skillName)
        {
            case "HelathBoost":
                StatsManager.Instance.UpdateMaxHealth(1);
                break;
                
            case "Sword Slash":
                combat.enabled = true;
                break;

            default:
                Debug.LogWarning("未知技能: " + skillName);
                break;

        }
    }
}
