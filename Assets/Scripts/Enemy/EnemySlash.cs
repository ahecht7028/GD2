using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public class EnemySlash : NetworkComponent
{
    public float damage = 30f;

    public Transform EnemyRef;
    Rigidbody rb;

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

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("ENEMYSLASH SPAWNED");
        if (IsServer)
        {
            // rb.angularVelocity = new Vector3(0, Mathf.PI, 0);
            StartCoroutine(TimeToLive());

        }

    }

    IEnumerator TimeToLive()
    {
        yield return new WaitForSeconds(0.5f);
        MyCore.NetDestroyObject(MyId.NetId);
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + 20 * Time.deltaTime, transform.eulerAngles.z);
        
    }

    private void OnTriggerEnter(Collider other)
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
}
