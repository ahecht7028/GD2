using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class RapidShotPellet : NetworkComponent
{
    public float damage = 100f;

    public override void HandleMessage(string flag, string value)
    {

    }

    public override void NetworkedStart()
    {

    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(0.1f);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (IsServer)
        {
            if(other.gameObject.tag == "Player")
            {
                other.GetComponent<PlayerController>().TakeDamage(damage, Owner, true);
            }

            if(other.gameObject.tag == "Enemy")
            {
                other.GetComponent<Enemy>().TakeDamage(damage, Owner);
            }
            MyCore.NetDestroyObject(MyId.NetId);
        }
    }
}
