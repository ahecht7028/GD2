using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scope : Item
{
    public Scope()
    {
        id = 4;
        itemName = "Scope";
        desc = "Chance to deal critical damage increased by 15%";
        stacks = 1;
    }

    public override void Passive(PlayerController player)
    {
        player.critChance += 0.15f * stacks;
    }
}
