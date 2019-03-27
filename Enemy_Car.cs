using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Inherits from the Enemy class, has access to all public and protected methods and variables
/// </summary>
public class Enemy_Car : Enemy
{
    /// <summary>
    /// Overrides the SpawnUnit method within the Enemy class
    /// Assigns all required variables before calling the base method within the Enemy Class
    /// </summary>
    public override void SpawnUnit(GameObject target)
    {
        navAgent = GetComponent<NavMeshAgent>();

        if (target != null)
        {
            state = State.charging;
            targetObject = target;
        }
        else state = State.wandering;

        targets = new List<GameObject>();
        thisCollider = GetComponent<Collider>();
        headController = transform.GetChild(0).GetChild(0);
        barrelController = transform.GetChild(0).GetChild(0).GetChild(0);
        barrelEnds = new Transform[] { transform.GetChild(0).GetChild(0).GetChild(1), transform.GetChild(0).GetChild(0).GetChild(2) };
        projectilePool = transform.GetChild(1);

        rotationSpeed = 4;
        shootCooldown = 1f;
        range = 80;
        health = 120;

        base.SpawnUnit(target);
    }
}
