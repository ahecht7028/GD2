using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boots : Item
{
    public Boots()
    {
        id = 0;
        itemName = "Running Boots";
        desc = "Boosts movement speed by 15%";
        stacks = 1;
    }

    public override void SetDefaults()
    {
        id = 0;
        itemName = "Running Boots";
        desc = "Boosts movement speed by 15%";
        stacks = 1;
    }

    public override void Passive(PlayerController player)
    {
        player.movementSpeed += 0.15f * stacks;
    }
}
