using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class RapidShotPellet : NetworkComponent
{
    public float damage = 10f;

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


            MyCore.NetDestroyObject(MyId.NetId);
        }
    }
}
