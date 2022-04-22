using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class Grenade : NetworkComponent
{
    public float damage = 500f;

    bool isExploding;

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

    private void OnCollisionEnter(Collision collision)
    {
        if (IsServer && !isExploding)
        {
            isExploding = true;
            StartCoroutine(Explode());
        }
    }

    IEnumerator Explode()
    {
        yield return new WaitForSeconds(1.5f);
        GameObject temp = MyCore.NetCreateObject(5, Owner, transform.position, Quaternion.identity);
        temp.GetComponent<Explosion>().damage = damage;
        MyCore.NetDestroyObject(MyId.NetId);
    }

    // Start is called before the first frame update
    void Start()
    {
        isExploding = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
