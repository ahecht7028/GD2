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
                Transform camPivot = transform.Find("CameraCenter");

                GameObject temp = MyCore.NetCreateObject(2, Owner, transform.position + (transform.forward * 1) + (transform.up * 1.5f), Quaternion.identity);
                temp.GetComponent<Rigidbody>().velocity = (camPivot.forward - (camPivot.up / 4)).normalized * 60;
                temp.GetComponent<RapidShotPellet>().damage *= float.Parse(value);
            }
        }
    }

    public override void ActivateAbility(PlayerController player)
    {
        player.OnShoot();
        cooldownTime = 0.4f / player.attackSpeed;
        SendCommand("SHOOT", CalculateDamage(player).ToString());
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
