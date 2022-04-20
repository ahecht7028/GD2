using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : Item
{
    public Ammo()
    {
        id = 2;
        itemName = "Ammo Crate";
        desc = "Increases damage dealt by 20%";
        stacks = 1;
    }

    public override void Passive(PlayerController player)
    {
        player.damage += 0.2f * stacks;
    }
}
