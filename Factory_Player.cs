using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Factory_Player : Building
{
    private Transform spawnPoint, unitPool;
    private GameObject spawnedUnit;

    void Start()
    {
        EnergyCost = 300;
        MetalCost = 500;
        CrystalCost = 0;
    }


    public override void PlaceBuilding(GameObject resource)
    {
        thisBuilding = gameObject;
        workedResource = null;
        buildingType = BuildingType.energyGenerator;
        MaxHealth = 300;
        Health = MaxHealth;
        constructionTime = 20f;
        light = transform.GetChild(1).GetComponentInChildren<Light>();
        spawnPoint = transform.GetChild(2);
        unitPool = GameObject.Find("PlayerUnitPool").transform;

        base.PlaceBuilding(workedResource);
    }

    /// <summary>
    /// Called when told to create a unit object
    /// </summary>
    /// <param name="unit"></param>
    public void CreateUnit(string unit)
    {
        float buildTime = 0;

        if (unit == "Engineer") buildTime = 8f;
        else if (unit == "Trike") buildTime = 4f;

        StartCoroutine(BuildTime(buildTime, unit));
    }

    /// <summary>
    /// Time delay before unit is spawned
    /// </summary>
    /// <param name="time"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    private IEnumerator BuildTime(float time, string name)
    {
        yield return new WaitForSeconds(time);

        foreach (Transform unit in unitPool)
        {
            if (unit.name == name + "(Clone)" & !unit.gameObject.activeSelf)
            {
                spawnedUnit = unit.gameObject;
                spawnedUnit.SetActive(true);
                spawnedUnit.transform.position = spawnPoint.position;
                spawnedUnit.GetComponent<Unit>().SpawnUnit();
                break;
            }
        }

    }
}
