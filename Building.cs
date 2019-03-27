using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BuildingType
{
    energyGenerator,
    metalExtractor,
    crystalExtractor,
    wall,
    turret,
    baseCore
}


public class Building : MonoBehaviour
{
    protected BuildingType buildingType;
    protected GameObject thisBuilding, workedResource;
    protected Light light;

    protected float constructionTime, range;
    protected bool constructed;

    private Transform buildingPool;
    private RangeDetector rangeDetector;
    private float energyCost, metalCost, crystalCost;
    private int health, maxHealth;


    public virtual void PlaceBuilding(GameObject resource)
    {
        buildingPool = GameObject.Find("PlayerBuildingPool").transform;
        constructed = false;
    }


    public void Construct()
    {
        StartCoroutine(ConstructionTimer(constructionTime));
    }


    public virtual void TakeDamage(int damage)
    {
        Health -= damage;

        if (Health <= 0) GetDestroyed();
    }

    /// <summary>
    /// Called when the building is hit with a projectile and health reaches zero
    /// Despawns the building and returns it to the building pool
    /// </summary>
    private void GetDestroyed()
    {
        if (workedResource != null)
        {
            workedResource.SetActive(true);
        }

        if (buildingType != BuildingType.baseCore)
        {
            EnemySpawner.enemySpawnerInstance.RemoveFromBuildingList(thisBuilding);
            ResourceManager.resourceManagerInstance.RemoveBuilding(buildingType);

            transform.position = buildingPool.position;

            thisBuilding.layer = LayerMask.NameToLayer("Wireframe");

            foreach (Transform child in thisBuilding.transform)
            {
                if (child.name != "Range" && child.name != "BulletPool")
                {
                    child.gameObject.layer = LayerMask.NameToLayer("Wireframe");

                    if (child.childCount > 0)
                    {
                        foreach (Transform grandChild in child)
                        {
                            grandChild.gameObject.layer = LayerMask.NameToLayer("Wireframe");
                        }
                    }
                }

            }
        }

        gameObject.SetActive(false);

        if (buildingType == BuildingType.baseCore)
        {
            UIController.uiControllerInstance.EndGame();
        }
    }

    /// <summary>
    /// Called when the building is being constructed
    /// Changes the layer from wireframe to building
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private IEnumerator ConstructionTimer(float time)
    {
        yield return new WaitForSeconds(3);

        thisBuilding.layer = LayerMask.NameToLayer("Buildings");

        foreach (Transform child in thisBuilding.transform)
        {
            if (child.name != "Range" && child.name != "BulletPool")
            {
                child.gameObject.layer = LayerMask.NameToLayer("Buildings");

                if (child.childCount > 0)
                {
                    foreach (Transform grandChild in child)
                    {
                        grandChild.gameObject.layer = LayerMask.NameToLayer("Buildings");
                    }
                }
            }

        }
        constructed = true;

        if (buildingType == BuildingType.turret)
        {
            rangeDetector = GetComponentInChildren<RangeDetector>();
            rangeDetector.Ready(range);
        }

        //light.enabled = true;
        EnemySpawner.enemySpawnerInstance.AddToBuildingList(thisBuilding);
        ResourceManager.resourceManagerInstance.AddBuilding(buildingType);
    }


    public float EnergyCost
    {
        get
        {
            return energyCost;
        }

        set
        {
            energyCost = value;
        }
    }

    public float MetalCost
    {
        get
        {
            return metalCost;
        }

        set
        {
            metalCost = value;
        }
    }

    public float CrystalCost
    {
        get
        {
            return crystalCost;
        }

        set
        {
            crystalCost = value;
        }
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

    public int MaxHealth
    {
        get
        {
            return maxHealth;
        }

        set
        {
            maxHealth = value;
        }
    }
}
