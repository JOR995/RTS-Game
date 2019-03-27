using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engineer : Unit
{
    private GameObject target;
    private bool reachedTarget;


    public override void SpawnUnit()
    {
        unitType = UnitType.engineer;
        thisCollider = GetComponent<Collider>();
        Health = 50;

        base.SpawnUnit();
    }

    /// <summary>
    /// Called when needed to construct a building, will move towards a building to allow its construction to begin
    /// </summary>
    /// <param name="building"></param>
    public void Construct(GameObject building)
    {
        navAgent.isStopped = false;
        target = building;
        navAgent.SetDestination(target.transform.position);
        reachedTarget = false;

        StartCoroutine(CheckDistance());
    }


    private IEnumerator CheckDistance()
    {
        do
        {
            if (Mathf.Abs(Vector3.Distance(transform.position, target.transform.position)) < 25)
            {
                navAgent.isStopped = true;
                reachedTarget = true;
            }

            yield return new WaitForSeconds(0.5f);
        }
        while (!reachedTarget);

        target.GetComponent<Building>().Construct();
    }
}
