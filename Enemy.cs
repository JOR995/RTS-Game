using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    protected enum State { idle, wandering, following, attacking, charging }
    protected State state;

    protected NavMeshAgent navAgent;
    protected List<GameObject> targets;
    protected Collider thisCollider;
    protected Transform headController, barrelController, projectilePool;
    protected Transform[] barrelEnds;
    protected GameObject targetObject;

    protected int health;
    protected float damage, rotationSpeed, shootCooldown, range;
    protected bool hasTarget;

    private RangeDetector rangeDetector;
    private GameObject projectile;
    private List<GameObject> availableProjectiles, usedProjectiles;

    /// <summary>
    /// Initial setup of the enemy object
    /// </summary>
    /// <param name="target"></param>
    public virtual void SpawnUnit(GameObject target)
    {
        navAgent.enabled = true;
        rangeDetector = transform.GetChild(2).GetComponent<RangeDetector>();
        rangeDetector.Ready(range);
        hasTarget = false;

        usedProjectiles = new List<GameObject>();
        availableProjectiles = new List<GameObject>();

        foreach (Transform obj in projectilePool)
        {
            availableProjectiles.Add(obj.gameObject);
        }

        ChangeState(state);
    }

    /// <summary>
    /// Called when the enemy is hit by a projectile
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0) ChangeState(State.idle);
    }

    /// <summary>
    /// Called to change the logic state of the enemy unit
    /// </summary>
    /// <param name="newState"></param>
    protected virtual void ChangeState(State newState)
    {
        switch (newState)
        {
            case State.idle:
                state = State.idle;
                StopAllCoroutines();
                navAgent.enabled = false;
                EnemySpawner.enemySpawnerInstance.DespawnUnit(gameObject);
                break;
            case State.wandering:
                state = State.wandering;
                StartCoroutine(WanderRoutine());
                navAgent.isStopped = false;
                break;
            case State.attacking:
                state = State.attacking;
                StopCoroutine(WanderRoutine());
                navAgent.isStopped = true;
                break;
            case State.charging:
                state = State.charging;
                navAgent.SetDestination(targetObject.transform.position);
                navAgent.isStopped = false;
                break;
        }
    }

    /// <summary>
    /// If a target is in range and in the targets list, the units gun will turn to point at it
    /// </summary>
    private void Update()
    {
        if (hasTarget)
        {
            if (targets[0] != null && targets[0].activeSelf)
            { 
                Vector3 targetPosition = new Vector3(targets[0].transform.position.x, targets[0].transform.position.y + 1.5f, targets[0].transform.position.z)
                    - headController.position;
                Quaternion rotation = Quaternion.LookRotation(targetPosition);
                headController.rotation = Quaternion.LerpUnclamped(headController.rotation, rotation, Time.deltaTime * rotationSpeed);
            }
            else
            {
                targets.RemoveAt(0);
                hasTarget = false;
                StopCoroutine("Shoot");

                if (targetObject != null)
                    ChangeState(State.charging);
                else ChangeState(State.wandering);
            }
        }
        else if (targets != null && targets.Count > 0)
        {
            hasTarget = true;
            StartCoroutine(Shoot(shootCooldown, targets[0], headController, barrelEnds, thisCollider));
            ChangeState(State.attacking);
        }
        else if (targets != null && targets.Count == 0)
        {
            Quaternion rotation = Quaternion.LookRotation(transform.forward);
            headController.rotation = Quaternion.LerpUnclamped(headController.rotation, rotation, Time.deltaTime * rotationSpeed);
        }
    }

    /// <summary>
    /// When a target gets in range it is added to the target list
    /// </summary>
    /// <param name="target"></param>
    public void AddToTargets(GameObject target)
    {
        targets.Add(target);
    }

    /// <summary>
    /// When a target moves out of range it is removed from the target list
    /// </summary>
    /// <param name="target"></param>
    public void RemoveFromTargets(GameObject target)
    {
        if (target == targets[0])
        {
            hasTarget = false;
            StopCoroutine("Shoot");
        }

        targets.Remove(target);
    }

    /// <summary>
    /// When in wandering state, finds a random position around the unit for it to move to
    /// </summary>
    /// <returns></returns>
    private IEnumerator WanderRoutine()
    {
        int count = 0;
        Vector3 targetPosition = transform.position;
        bool validPoint = false;
        NavMeshHit hit;

        do
        {
            for (int i = 0; i < 30; i++)
            {
                Vector3 randomPoint = transform.position + Random.insideUnitSphere * 100f;

                if (NavMesh.SamplePosition(randomPoint, out hit, 200f, NavMesh.AllAreas))
                {
                    targetPosition = hit.position;
                    validPoint = true;
                    break;
                }
                else validPoint = false;
            }

            if (validPoint)
            {
                navAgent.SetDestination(targetPosition);
            }
            yield return new WaitForSeconds(Random.Range(5.5f, 8.4f));
        }
        while (state == State.wandering);//count < Random.Range(2, 5));
    }

    /// <summary>
    /// When a target is in range, a raycast is cast from the gun barrel end towards the target
    /// If it hits then a bullet is fired
    /// </summary>
    /// <param name="time"></param>
    /// <param name="target"></param>
    /// <param name="source"></param>
    /// <param name="projectileSource"></param>
    /// <param name="sourceCollider"></param>
    /// <returns></returns>
    private IEnumerator Shoot(float time, GameObject target, Transform source, Transform[] projectileSource, Collider sourceCollider)
    {
        RaycastHit hit;
        int layerMask = 1 << 9 | 1 << 17;

        do
        {
            if (Physics.Raycast(source.position, source.forward, out hit, Mathf.Infinity, layerMask))
            {
                if (hit.transform.gameObject == target)
                {
                    if (availableProjectiles.Count >= projectileSource.Length)
                    {
                        for (int i = 0; i < projectileSource.Length; i++)
                        {
                            projectile = availableProjectiles[0];
                            availableProjectiles.Remove(projectile);
                            projectile.transform.position = projectileSource[i].position;
                            projectile.transform.rotation = source.rotation;
                            Physics.IgnoreCollision(projectile.GetComponent<Collider>(), sourceCollider);
                            projectile.GetComponent<ProjectileScript>().SpawnBullet(gameObject, target, null, this, null);
                            usedProjectiles.Add(projectile);
                        }
                    }
                }
            }
            yield return new WaitForSeconds(time);
        }
        while (hasTarget);
    }

    /// <summary>
    /// Called by projectiles fired from this object, returns projectile objects to the projectile pool
    /// </summary>
    /// <param name="bullet"></param>
    public void ReloadProjectile(GameObject bullet)
    {
        bullet.transform.position = projectilePool.position;
        usedProjectiles.Remove(bullet);
        availableProjectiles.Add(bullet);
    }
}
