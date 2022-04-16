using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class Drone : Enemy
{
    // Start is called before the first frame update
    public override void HandleMessage(string flag, string value)
    {

    }

    public override void Attack()
    {

    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void Die()
    {
        //Funny particle FX here
        MyCore.NetDestroyObject(MyId.NetId);
    }
}
