using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NETWORK_ENGINE;
using UnityEngine.AI;

public class BlueBox : Enemy
{
        public NavMeshAgent MyAgent;
    public List<Vector3> Goals;
    public Vector3 CurrentGoal;
    int index;
    public override void HandleMessage(string flag, string value)
    {
        /*
        if (flag == "NAV")
        {

            index = int.Parse(value);
            //Debug.Log("Nav Called, index = " + index + ", speed = " + speed);
            CurrentGoal = Goals[index];
            LookAtTarget(CurrentGoal);
            MyAgent.SetDestination(CurrentGoal);
        }
        */
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

            if ((this.transform.position - CurrentGoal).magnitude <= 1f)
            {
                int temp = Random.Range(0, Goals.Count);
                //SendUpdate("NAV", temp.ToString());
                CurrentGoal = Goals[temp];
                // LookAtTarget(CurrentGoal);
                MyAgent.SetDestination(CurrentGoal);

            }


            yield return new WaitForSeconds(.05f);
        }
        yield return new WaitForSeconds(.05f);

    }
    // Update is called once per frame
    void Update()
    {

    }
    
    /*
    public void LookAtTarget(Vector3 target)
    {
        //Debug.Log("Looking at " + target);
        transform.LookAt(new Vector3(target.x, transform.position.y, target.z));
    }
    */
}
