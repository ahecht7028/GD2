using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using NETWORK_ENGINE;

public class RapidShotPellet : NetworkComponent
{
    public float damage = 100f;

    public override void HandleMessage(string flag, string value)
    {
        if (flag == "DISABLE_VFX")
        {
            transform.Find("PelletTrail").GetComponent<VisualEffect>().Stop();
        }
    }

    public override void NetworkedStart()
    {

    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            if (IsServer)
            {
                if (Mathf.Abs(transform.position.x) > 1000f || Mathf.Abs(transform.position.y) > 1000f || Mathf.Abs(transform.position.z) > 1000f)
                {
                    MyCore.NetDestroyObject(MyId.NetId);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (IsServer)
        {
            if (other.gameObject.tag == "Player")
            {
                other.GetComponent<PlayerController>().TakeDamage(damage, Owner, true);
                GameObject.Find("PlayerCanvas").GetComponent<DamageTextController>().CreateFloatingText(damage.ToString(), transform.position, false, 3);
            }

            if (other.gameObject.tag == "Enemy")
            {
                other.GetComponent<Enemy>().TakeDamage(damage, Owner);
                GameObject.Find("PlayerCanvas").GetComponent<DamageTextController>().CreateFloatingText(damage.ToString(), transform.position, false, 3);
            }
            GetComponent<SphereCollider>().enabled = false;
            SendUpdate("DISABLE_VFX", "");
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            StartCoroutine(KillProjectile());
        }
        if (IsClient)
        {
            if (other.gameObject.tag == "Player" || other.gameObject.tag == "Enemy")
            {
                GameObject.Find("PlayerCanvas").GetComponent<DamageTextController>().CreateFloatingText(damage.ToString(), transform.position, false, 3);
            }
        }
    }

    IEnumerator KillProjectile()
    {
        yield return new WaitForSeconds(1);
        MyCore.NetDestroyObject(MyId.NetId);
    }
}
