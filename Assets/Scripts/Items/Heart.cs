using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : Item
{
    public Heart()
    {
        id = 1;
        itemName = "Heart";
        desc = "Increases Max HP by 30%";
        stacks = 1;
    }

    public override void Passive(PlayerController player)
    {
        player.maxHealthMod += 0.3f * stacks;
    }
}
