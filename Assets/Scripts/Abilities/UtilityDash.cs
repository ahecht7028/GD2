using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class UtilityDash : Ability
{

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "DASH")
        {
            if (IsServer)
            {
                GetComponent<PlayerController>().StartCoroutine(GetComponent<PlayerController>().StartDash());
            }
        }
    }

    public override void ActivateAbility()
    {
        SendCommand("DASH", "1");
    }

    // Start is called before the first frame update
    void Start()
    {
        cooldownTime = 2f;
        autoUse = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
