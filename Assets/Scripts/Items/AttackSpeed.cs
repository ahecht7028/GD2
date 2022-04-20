using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSpeed : Item
{
    public AttackSpeed()
    {
        id = 3;
        itemName = "Syringe";
        desc = "Increases attack speed by 10%";
        stacks = 1;
    }

    public override void Passive(PlayerController player)
    {
        player.attackSpeed += 0.1f * stacks;
    }
}
