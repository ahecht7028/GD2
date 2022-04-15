using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class SpecialGrenade : Ability
{
    public override void HandleMessage(string flag, string value)
    {
        if(flag == "THROW")
        {
            if (IsServer)
            {
                GameObject temp = MyCore.NetCreateObject(4, Owner, transform.position + transform.forward * 2, Quaternion.identity);
                temp.GetComponent<Rigidbody>().velocity = transform.forward * 30 + transform.up * 10;
                temp.GetComponent<Grenade>().damage *= float.Parse(value);
            }
        }
    }

    public override void ActivateAbility(PlayerController player)
    {
        player.OnShoot();
        cooldownTime = 1f / player.attackSpeed;
        SendCommand("THROW", CalculateDamage(player).ToString());
    }

    // Start is called before the first frame update
    void Start()
    {
        cooldownTime = 1f;
        autoUse = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
