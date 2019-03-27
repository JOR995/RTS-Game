using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager resourceManagerInstance;

    [SerializeField]
    Slider energySlider, metalSlider, crystalSlider;

    [SerializeField]
    Text energyText, energyGrowth, metalText, metalGrowth, crystalText, crystalGrowth;

    [SerializeField]
    Transform buildingParent;

    private List<GameObject> generatorList, metalExtractorList, crystalExtractorList;

    private float energyCurrent, energyMax, metalCurrent, metalMax, crystalCurrent, crystalMax;
    private float energyGeneration, energyUsage, metalGeneration, metalUsage, crystalGeneration, crystalUsage;
    private int generatorCount, metalExtractorCount, crystalExtractorCount;


    void Awake()
    {
        resourceManagerInstance = this;
    }

    /// <summary>
    /// Handles working out how much of each resource the player is generating / expending
    /// Uses lists and counts of the different resource generating buildings to work these variables out
    /// Then displays information in the text objects within the resources panel
    /// </summary>
    void Start()
    {
        generatorList = new List<GameObject>();
        metalExtractorList = new List<GameObject>();
        crystalExtractorList = new List<GameObject>();

        energyMax = 1000;
        energyCurrent = energyMax / 2;
        energyGeneration = 10;
        energySlider.maxValue = energyMax;
        energySlider.value = energyCurrent;

        metalMax = 1000;
        metalCurrent = metalMax / 2;
        metalGeneration = 3;
        metalSlider.maxValue = metalMax;
        metalSlider.value = metalCurrent;

        crystalMax = 100;
        crystalCurrent = 0;
        crystalGeneration = 0;
        crystalSlider.maxValue = crystalMax;
        crystalSlider.value = crystalCurrent;

        generatorCount = 0;
        metalExtractorCount = 0;
        crystalExtractorCount = 0;

        StartCoroutine(ResourceGeneration());
    }


    void Update()
    {

    }


    
    public void UseResources(float energyCost, float metalCost, float crystalCost)
    {
        energyCurrent -= energyCost;
        metalCurrent -= metalCost;
        crystalCurrent -= crystalCost;
    }


    public void AddBuilding(BuildingType type)
    {
        switch (type)
        {
            case BuildingType.energyGenerator:
                generatorCount++;
                break;
            case BuildingType.metalExtractor:
                metalExtractorCount++;
                break;
            case BuildingType.crystalExtractor:
                crystalExtractorCount++;
                break;
        }
    }


    public void RemoveBuilding(BuildingType type)
    {
        switch (type)
        {
            case BuildingType.energyGenerator:
                generatorCount--;
                break;
            case BuildingType.metalExtractor:
                metalExtractorCount--;
                break;
            case BuildingType.crystalExtractor:
                crystalExtractorCount--;
                break;
        }
    }


    private IEnumerator ResourceGeneration()
    {
        do
        {
            yield return new WaitForSeconds(1f);

            energyCurrent += energyGeneration + (generatorCount * 5);
            if (energyCurrent > energyMax) energyCurrent = energyMax;

            energySlider.maxValue = energyMax;
            energySlider.value = energyCurrent;
            energyText.text = energyCurrent + " / " + energyMax;
            energyGrowth.text = (energyGeneration + (generatorCount * 5)) - energyUsage + "/s";
            

            metalCurrent += metalGeneration + (metalExtractorCount * 10);
            if (metalCurrent > metalMax) metalCurrent = metalMax;

            metalSlider.maxValue = metalMax;
            metalSlider.value = metalCurrent;
            metalText.text = metalCurrent + " / " + metalMax;
            metalGrowth.text = (metalGeneration + (metalExtractorCount * 10)) - metalUsage + "/s";


            crystalCurrent += crystalGeneration + (crystalExtractorCount * 10);
            if (crystalCurrent > crystalMax) crystalCurrent = crystalMax;

            crystalSlider.maxValue = crystalMax;
            crystalSlider.value = crystalCurrent;
            crystalText.text = crystalCurrent + " / " + crystalMax;
            crystalGrowth.text = (crystalGeneration + (crystalExtractorCount * 10)) - crystalUsage + "/s";
        }
        while (true);
    }


    public float EnergyCurrent
    {
        get
        {
            return energyCurrent;
        }
    }

    public float MetalCurrent
    {
        get
        {
            return metalCurrent;
        }
    }

    public float CrystalCurrent
    {
        get
        {
            return crystalCurrent;
        }
    }
}
