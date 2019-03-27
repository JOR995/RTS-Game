using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    protected enum UnitType { engineer, trike };
    protected UnitType unitType;

    protected NavMeshAgent navAgent;
    protected List<GameObject> targets;
    protected Collider thisCollider;
    protected Transform headController, barrelController, projectilePool;
    protected Transform[] barrelEnds;
    protected GameObject targetObject;

    private int health;
    protected float damage, rotationSpeed, shootCooldown, range;
    protected bool hasTarget;

    private Transform unitPool;
    private RangeDetector rangeDetector;
    private GameObject projectile;
    private List<GameObject> availableProjectiles, usedProjectiles;


    public virtual void SpawnUnit()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.enabled = true;

        unitPool = GameObject.Find("PlayerUnitPool").transform;

        if (unitType != UnitType.engineer)
        {
            rangeDetector = transform.GetChild(2).GetComponent<RangeDetector>();
            rangeDetector.Ready(range);
            hasTarget = false;

            usedProjectiles = new List<GameObject>();
            availableProjectiles = new List<GameObject>();

            foreach (Transform obj in projectilePool)
            {
                availableProjectiles.Add(obj.gameObject);
            }
        }
    }


    public void TakeDamage(int damage)
    {
        Health -= damage;

        if (Health <= 0)
        {
            StopAllCoroutines();
            navAgent.enabled = false;
            transform.position = unitPool.position;
            gameObject.SetActive(false);
        }
    }


    public void SetMoveTarget(Vector3 target)
    {
        navAgent.isStopped = false;
        navAgent.SetDestination(target);
    }


    private void Update()
    {
        if (unitType != UnitType.engineer)
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
                }
            }
            else if (targets != null && targets.Count > 0)
            {
                hasTarget = true;
                StartCoroutine(Shoot(shootCooldown, targets[0], headController, barrelEnds, thisCollider));
            }
            else if (targets != null && targets.Count == 0)
            {
                Quaternion rotation = Quaternion.LookRotation(transform.forward);
                headController.rotation = Quaternion.LerpUnclamped(headController.rotation, rotation, Time.deltaTime * rotationSpeed);
            }
        }
    }


    public void AddToTargets(GameObject target)
    {
        targets.Add(target);
    }


    public void RemoveFromTargets(GameObject target)
    {
        if (target == targets[0])
        {
            hasTarget = false;
            StopCoroutine("Shoot");
        }

        targets.Remove(target);
    }


    private IEnumerator Shoot(float time, GameObject target, Transform source, Transform[] projectileSource, Collider sourceCollider)
    {
        RaycastHit hit;
        int layerMask = 1 << 11;

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
                            projectile.GetComponent<ProjectileScript>().SpawnBullet(gameObject, target, null, null, this);
                            usedProjectiles.Add(projectile);
                        }
                    }
                }
            }
            yield return new WaitForSeconds(time);
        }
        while (hasTarget);
    }


    public void ReloadProjectile(GameObject bullet)
    {
        bullet.transform.position = projectilePool.position;
        usedProjectiles.Remove(bullet);
        availableProjectiles.Add(bullet);
    }


    public int Health
    {
        get
        {
            return health;
        }
        set
        {
            health = value;
        }
    }
}
