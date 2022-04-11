using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class M1RapidShoot : Ability
{

    public override void HandleMessage(string flag, string value)
    {
        if(flag == "SHOOT")
        {
            if (IsServer)
            {
                // Figure out projectile
                GameObject temp = MyCore.NetCreateObject(2, Owner, transform.position + transform.forward * 2, Quaternion.identity);
                temp.GetComponent<Rigidbody>().velocity = transform.forward * 30;
            }
        }
    }

    public override void ActivateAbility()
    {
        SendCommand("SHOOT", "1");
    }

    // Start is called before the first frame update
    void Start()
    {
        cooldownTime = 0.4f;
        autoUse = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
