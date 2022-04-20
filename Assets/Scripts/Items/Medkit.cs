using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Medkit : Item
{
    public Medkit()
    {
        id = 5;
        itemName = "Medkit";
        desc = "Natural health regen increased by 1 health per second";
        stacks = 1;
    }

    public override void Passive(PlayerController player)
    {
        player.regen += 0.1f * stacks;
    }
}
