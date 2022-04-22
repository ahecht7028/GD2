using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public abstract class Enemy : NetworkComponent
{
    public float hp, attack, attackCooldown, timer, DetectionRange, AttackRange;
    public Vector3 target;
    public bool hasTarget;
    public override void HandleMessage(string flag, string value)
    {
        if (flag == "DMG" && IsServer)
        {
            Debug.Log("Recieving DMG message");
            hp -= float.Parse(value);
        }


    }

    public override void NetworkedStart()
    {

    }

    public override IEnumerator SlowUpdate()
    {
        while (IsConnected)
        {
            timer += 0.1f;

            yield return new WaitForSeconds(0.1f);
        }
    }

    public virtual void TakeDamage(float _damage, int _owner)
    {
        hp -= _damage;
        SendUpdate("DMG", _damage.ToString());
        Debug.Log("Sent DMG message");
        if (hp <= 0)
        {
            foreach (PlayerController p in FindObjectsOfType<PlayerController>())
            {
                if (p.Owner == _owner)
                {
                    //add money and exp

                    p.exp += 2;
                }
            }
            MyCore.NetDestroyObject(MyId.NetId);
            Die();
        }
    }



    public virtual void Attack()
    {

    }
    public virtual void Die()
    {
        MyCore.NetDestroyObject(MyId.NetId);
    }

    public virtual bool GetTarget()
    {

        foreach (PlayerController p in FindObjectsOfType<PlayerController>())
        {
            if ((transform.position - p.transform.position).magnitude <= DetectionRange)
            {
                target = p.transform.position;
                hasTarget=true;
                return true;

            }

        }
        hasTarget= false;
        return false;
    }


    public virtual void SetScaling(float scaling)
    {
        hp *= scaling;
        attack *= scaling;
    }

    // Start is called before the first frame update
    void Start()
    {
        hp = 100;
        attack = 10;

        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }


}
