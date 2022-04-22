using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class Explosion : NetworkComponent
{
    public float damage = 500f;

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

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer)
        {
            if (other.gameObject.tag == "Player")
            {
                other.GetComponent<PlayerController>().TakeDamage(damage, Owner, true);
            }
            if (other.gameObject.tag == "Enemy")
            {
                other.GetComponent<Enemy>().TakeDamage(damage, Owner);
            }
        }
        if (IsLocalPlayer)
        {
            if (other.gameObject.tag == "Player" || other.gameObject.tag == "Enemy")
            {
                GameObject.Find("PlayerCanvas").GetComponent<DamageTextController>().CreateFloatingText(damage.ToString(), transform.position, false, 3);
            }
        }
    }
}
