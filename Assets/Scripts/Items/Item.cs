using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Item
{
    public int id;
    public string itemName;
    public string desc;
    public int stacks;

    public virtual void SetDefaults()
    {
        id = -1;
        itemName = "NAME";
        desc = "DESC";
        stacks = 1;
    }

    public Sprite GetSprite()
    {
        return GameObject.Find("PlayerCanvas").GetComponent<IconSprites>().itemIcons[id];
    }

    public virtual void OnShoot(PlayerController player) { }
    public virtual void OnTakeDamage(PlayerController player) { }
    public virtual void OnHit(PlayerController player) { }
    public virtual void Passive(PlayerController player) { }
}
