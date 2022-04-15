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

                GameObject temp = MyCore.NetCreateObject(2, Owner, camPivot.position + camPivot.forward * 2, Quaternion.identity);
                temp.GetComponent<Rigidbody>().velocity = camPivot.forward * 80;
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
