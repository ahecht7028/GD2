using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class Explosion : NetworkComponent
{
    public override void HandleMessage(string flag, string value)
    {

    }

    public override void NetworkedStart()
    {

    }

    public override IEnumerator SlowUpdate()
    {
        yield return new WaitForSeconds(0.2f);
        GetComponent<SphereCollider>().enabled = false;
        yield return new WaitForSeconds(2f);
        MyCore.NetDestroyObject(MyId.NetId);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
