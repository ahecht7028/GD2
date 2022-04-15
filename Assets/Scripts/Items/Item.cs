using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    int id;
    string name;
    string desc;
    int stacks;

    public virtual void SetDefaults()
    {
        id = -1;
        name = "NAME";
        desc = "DESC";
        stacks = 1;
    }

    public virtual void OnShoot(PlayerController player) { }
    public virtual void OnTakeDamage(PlayerController player) { }
    public virtual void OnHit(PlayerController player) { }
    public virtual void Passive(PlayerController player) { }
}
