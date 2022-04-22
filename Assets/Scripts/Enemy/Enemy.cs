using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;

public abstract class Enemy : NetworkComponent
{
    public float hp, attack, attackCooldown, timer, DetectionRange, AttackRange;
    public Vector3 target;
    public bool hasTarget;
    bool isFlashing = false;

    public Material baseMat;
    public Material redFlash;
    public override void HandleMessage(string flag, string value)
    {
        if (flag == "DMG" && IsServer)
        {

            hp -= float.Parse(value);
            Debug.Log("Enemy Taking Dmg, HP is " + hp);
            if (hp != 0)
            {
                StartCoroutine(FlashRed());
            }
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
                    if (hp != 0)
            {
                StartCoroutine(FlashRed());
            }
        if (hp <= 0)
        {
            Debug.Log("Enemy Dying");
            foreach (PlayerController p in FindObjectsOfType<PlayerController>())
            {
                if (p.Owner == _owner)
                {
                    GM_Script gm = FindObjectOfType<GM_Script>();
                    p.GetMoney(15 * gm.roundNum);

                    p.GetEXP(10 * gm.roundNum);
                    MyCore.NetDestroyObject(MyId.NetId);
                }
            }



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
                hasTarget = true;
                return true;

            }

        }
        hasTarget = false;
        return false;
    }

    public IEnumerator FlashRed()
    {
        Debug.Log("EnemyFlashing Called");
        if (!isFlashing)
        {
            isFlashing = true;
            SkinnedMeshRenderer mr = transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();
            Material[] mats = new Material[3];
            if (mr != null)
            {
                mats = mr.materials;
            }

            for (int i = 0; i < 10; i++)
            {
                if (mr != null)
                {
                    mats[1] = redFlash;
                    mr.materials = mats;
                }
                yield return new WaitForSeconds(0.03f);
                if (mr != null)
                {
                    mats[1] = baseMat;
                    mr.materials = mats;
                }
                yield return new WaitForSeconds(0.03f);
            }

            isFlashing = false;
        }
        yield return null;
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
