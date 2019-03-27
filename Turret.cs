using System.Collections;
using System.Collections.Generic;
using VolumetricLines;
using UnityEngine;

public class Turret : Building
{
    protected Transform[] barrelEnds;
    protected Transform headController, projectilePool;
    protected List<GameObject> targets;
    protected Collider thisCollider;

    protected float damage, shootCooldown, rotationSpeed;
    protected string bulletType;
    protected bool hasTarget;

    private VolumetricLineBehavior volumeLine;
    private GameObject projectile;
    private List<GameObject> availableProjectiles, usedProjectiles;


    public override void PlaceBuilding(GameObject resource)
    {
        usedProjectiles = new List<GameObject>();
        availableProjectiles = new List<GameObject>();

        foreach (Transform obj in projectilePool)
        {
            availableProjectiles.Add(obj.gameObject);
        }

        base.PlaceBuilding(workedResource);
    }


    private void Update()
    {
        if (hasTarget)
        {
            if (targets[0] != null && targets[0].activeSelf)
            {
                Vector3 targetPosition = targets[0].transform.position - headController.position;
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
        else if (targets != null && targets.Count > 0 & constructed)
        {
            hasTarget = true;
            StartCoroutine(Shoot(shootCooldown, targets[0], headController, barrelEnds, thisCollider));
        }
        else if (targets != null && targets.Count == 0 & constructed)
        {
            Quaternion rotation = Quaternion.LookRotation(transform.forward);
            headController.rotation = Quaternion.LerpUnclamped(headController.rotation, rotation, Time.deltaTime * rotationSpeed);
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
        projectile = null;

        do
        {
            if (Physics.Raycast(source.position, source.forward, out hit, Mathf.Infinity, layerMask))
            {
                if (hit.transform.gameObject == target || hit.transform.tag == "Enemy")
                {
                    if (bulletType == "Laser_Player")
                    {
                        //if (projectile == null)
                        //{
                        //    yield return new WaitForSeconds(2);
                        //    projectile = Instantiate(Resources.Load("Projectiles/" + bulletType), projectileSource[0].position, Quaternion.identity, projectileParent) as GameObject;
                        //    Physics.IgnoreCollision(projectile.GetComponent<Collider>(), sourceCollider);
                        //    volumeLine = projectile.GetComponent<VolumetricLineBehavior>();
                        //}

                        //projectile.transform.position = projectileSource[0].position;
                        //projectile.transform.rotation = source.rotation;
                        //volumeLine.StartPos = source.position;
                        //volumeLine.EndPos = target.transform.position;
                    }
                    else
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
                                projectile.GetComponent<ProjectileScript>().SpawnBullet(gameObject, target, this, null, null);
                                usedProjectiles.Add(projectile);
                            }
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
}
