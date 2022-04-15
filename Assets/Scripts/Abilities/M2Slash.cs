using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class M2Slash : Ability
{
    public override void HandleMessage(string flag, string value)
    {
        if(flag == "SLASH")
        {
            if (IsServer)
            {
                string[] args = value.Split(',');

                int newOwner = int.Parse(args[0]);

                GameObject temp = MyCore.NetCreateObject(3, newOwner, transform.position + transform.forward, Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y - 90, transform.eulerAngles.z));
                temp.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 2 * Mathf.PI, 0);
                temp.GetComponent<SlashAttack>().damage *= float.Parse(args[1]);
            }
        }
    }

    public override void ActivateAbility(PlayerController player)
    {
        player.OnShoot();
        cooldownTime = 2f / player.attackSpeed;
        SendCommand("SLASH", Owner + "," + CalculateDamage(player).ToString());
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
