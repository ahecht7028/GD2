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

    public void UseAbility()
    {
        if(cooldownTime <= currentTime)
        {
            ActivateAbility();
            currentTime = 0;
        }
    }

    public virtual void ActivateAbility()
    {

    }
}
