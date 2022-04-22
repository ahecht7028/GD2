using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.AI;

public class RedBox : Enemy
{
    public NavMeshAgent MyAgent;
    public List<Vector3> Goals;
    public Vector3 CurrentGoal;


    int index;
    public override void HandleMessage(string flag, string value)
    {

        if (flag == "NAV")
        {

            index = int.Parse(value);
            //Debug.Log("Nav Called, index = " + index + ", speed = " + speed);
            CurrentGoal = Goals[index];
            LookAtTarget(CurrentGoal);
            MyAgent.SetDestination(CurrentGoal);
        }

        if (flag == "SLASH")
        {

            Debug.Log("Server Creating Attack");
            string[] args = value.Split(',');

            int newOwner = int.Parse(args[0]);



        }


    }
    void Start()
    {
        MyAgent = GetComponent<NavMeshAgent>();
        //Add points (empty gameobjects) tagged with NavPoint
        GameObject[] temp = GameObject.FindGameObjectsWithTag("NavPoint");
        Goals = new List<Vector3>();
        foreach (GameObject g in temp)
        {
            Goals.Add(g.transform.position);
        }
    }

    public override IEnumerator SlowUpdate()
    {
        if (IsServer)
        {
            int temp = Random.Range(0, Goals.Count); //Goal Set to random initial goal

            CurrentGoal = Goals[temp];
            MyAgent.SetDestination(CurrentGoal);
        }
        while (IsServer)
        {
            GetTarget();
            if (GetTarget() && hasTarget)
            {
                CurrentGoal = target;
                MyAgent.SetDestination(CurrentGoal);
                Debug.Log("Got Target");
            }

            CheckAttack();

            if ((this.transform.position - CurrentGoal).magnitude <= 1f)
            {
                int temp = Random.Range(0, Goals.Count);
                SendUpdate("NAV", temp.ToString());
                CurrentGoal = Goals[temp];
                LookAtTarget(CurrentGoal);
                MyAgent.SetDestination(CurrentGoal);

            }

            timer += 0.05f;
            yield return new WaitForSeconds(.05f);
        }

    }
    // Update is called once per frame
    void Update()
    {

    }



    public void LookAtTarget(Vector3 target)
    {
        //Debug.Log("Looking at " + target);
        transform.LookAt(new Vector3(target.x, transform.position.y, target.z));
    }

    public override void Attack()
    {
        Debug.Log("Attack Called, Command Sent");
        SendUpdate("SLASH", Owner + "," + attack.ToString());
        GameObject temp = MyCore.NetCreateObject(9, Owner, transform.position + transform.forward, Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y - 90, transform.eulerAngles.z));
        temp.GetComponent<EnemySlash>().playerRef = transform;
        temp.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 2 * Mathf.PI, 0);
        temp.GetComponent<EnemySlash>().damage = attack;
    }

    public bool CheckAttack()
    {
        if (timer >= attackCooldown)
        {
            foreach (PlayerController p in FindObjectsOfType<PlayerController>())
            {
                if ((transform.position - p.transform.position).magnitude <= AttackRange)
                {
                    Attack();
                    timer = 0;

                    return true;

                }

            }



        }

        return false;
    }
}
