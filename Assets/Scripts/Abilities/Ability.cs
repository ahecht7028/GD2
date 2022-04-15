using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public abstract class Ability : NetworkComponent
{
    public bool autoUse;
    public float cooldownTime;
    float currentTime;

    public override void NetworkedStart()
    {
        currentTime = 0;
    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            currentTime += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void UseAbility(PlayerController player)
    {
        if(cooldownTime <= currentTime)
        {
            ActivateAbility(player);
            currentTime = 0;
        }
    }

    public virtual void ActivateAbility(PlayerController player)
    {

    }

    public float CalculateDamage(PlayerController player)
    {
        float critMod = 1f;

        if(Random.Range(0f, 1f) < player.critChance)
        {
            critMod = 2f;
        }
        return (player.damage * (player.level / 10.0f)) * critMod;
    }
}
