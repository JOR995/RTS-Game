using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    //Public variable for this class to be accessed from other classes
    //Static to ensure only one instance of this class cna be created
    public static PlayerInteraction playerInteractionInstance;

    [SerializeField]
    Transform metalParent, buildingPool;

    [SerializeField]
    Text infoText;

    private ResourceManager resourceManager;
    private GameObject selectedConstruction, closestResource;
    private List<GameObject> selectedObjects;

    private bool canBePlaced, paused;


    void Awake()
    {
        //Sets the public static variable equal to this current instance of the class
        playerInteractionInstance = this;
    }


    void Start()
    {
        resourceManager = ResourceManager.resourceManagerInstance;
        selectedConstruction = null;
        canBePlaced = false;
        paused = false;
        selectedObjects = new List<GameObject>();
    }


    void Update()
    {
        if (!paused)
        {
            if (Input.GetButtonDown("Cancel"))
            {
                UIController.uiControllerInstance.ChangePanel(PanelType.pause, selectedObjects);
                paused = true;
            }

            if (selectedConstruction == null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    GetSelection();
                }

                if (selectedObjects.Count > 0)
                {
                    if (Input.GetMouseButtonDown(1))
                    {
                        if (selectedObjects[0].layer == 17)
                        {
                            MoveSelection();
                        }
                    }
                }
            }
            //Whilst a building is currently selected for construction, runs through the called method every frame
            if (selectedConstruction != null)
            {
                UpdateSelectedObject();
            }
        }
    }

    /// <summary>
    /// Upon left mouse button click, casts a ray from mouse position to see if the user is trying to select and object
    /// The selection is then added to a list
    /// </summary>
    private void GetSelection()
    {
        RaycastHit hit;
        int layerMask = 1 << 11;
        layerMask = ~layerMask;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, layerMask, QueryTriggerInteraction.Ignore))
        {
            selectedObjects = new List<GameObject>();

            /*if (hit.transform.gameObject.layer == 8)
            {
                UIController.uiControllerInstance.ChangePanel(PanelType.empty, null);
                selectedObjects = new List<GameObject>();
            }*/
            if (hit.transform.tag == "Factory")
            {
                selectedObjects.Add(hit.transform.gameObject);
                UIController.uiControllerInstance.ChangePanel(PanelType.factory, selectedObjects);
            }
            else if (hit.transform.tag == "Engineer")
            {
                selectedObjects.Add(hit.transform.gameObject);
                UIController.uiControllerInstance.ChangePanel(PanelType.engineer, selectedObjects);
            }
            else if (hit.transform.gameObject.layer == 9)
            {
                selectedObjects.Add(hit.transform.gameObject);
                UIController.uiControllerInstance.ChangePanel(PanelType.building, selectedObjects);
            }
            else if (hit.transform.gameObject.layer == 17)
            {
                selectedObjects.Add(hit.transform.gameObject);
                UIController.uiControllerInstance.ChangePanel(PanelType.unit, selectedObjects);
            }
        }
    }

    //When a unit is selected, right clicking will set a destination target for it navAgent to make the unit move
    private void MoveSelection()
    {
        RaycastHit hit;
        int layerMask = 1 << 8;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, layerMask, QueryTriggerInteraction.Ignore))
        {
            selectedObjects[0].GetComponent<Unit>().SetMoveTarget(hit.point);
        }
    }

    /// <summary>
    /// Called when a building is selected for construction
    /// Updates the position and state of the building 
    /// </summary>
    private void UpdateSelectedObject()
    {
        RaycastHit hit;
        int layerMask = 1 << 8;
        float distance;

        //Allows for the building to be rotated around the y-axis in world space using the mouse scrollwheel
        selectedConstruction.transform.Rotate(new Vector3(0, Input.GetAxis("Mouse ScrollWheel") * 100, 0), Space.World);

        //Creates a raycast out from the current position of the mouse cursor and outputs the RaycastHit hit
        //Uses a layermask to only return true if a collision with an object on layer 8 (terrain) has occurred 
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, layerMask))
        {
            //If the selected building is an extractor then specific movement updates are applied
            if (selectedConstruction.tag == "MetalExtractor")
            {
                //Iterates through all active metal resource nodes in the scene
                foreach (Transform metal in metalParent)
                {
                    if (metal.gameObject.activeSelf)
                    {
                        //Finds the absolute value of the distance between the current resource object and the hit position from the raycast
                        distance = Mathf.Abs(Vector3.Distance(metal.transform.position, hit.point));

                        //If that distance is less than 10, then the position of the selected building is snapped to the resource object and can be placed
                        if (distance < 10f)
                        {
                            selectedConstruction.transform.position = metal.transform.position + new Vector3(0, 2.5f, 0);
                            closestResource = metal.gameObject;
                            canBePlaced = true;
                            break;
                        }
                        else canBePlaced = false;

                        //Otherwise the building cannot be placed and it's position follows the hit position of the raycast
                        if (!canBePlaced)
                        {
                            selectedConstruction.transform.position = hit.point;
                        }
                    }
                }
            }
            else
            {
                //If the selected building is not an extractor, it's position follows the hit position of the raycast
                selectedConstruction.transform.position = hit.point;

                //If the y coord of the hit position falls within this range then the building can be placed, otherwise it cannot
                if (hit.point.y >= 16f && hit.point.y <= 22.09f)
                {
                    canBePlaced = true;
                }
                else
                {
                    canBePlaced = false;
                }

            }

            //If the right mouse button is clicked then the current construction is deselected and despawned
            //The selectedConstruction is also set to null
            if (Input.GetMouseButtonDown(1))
            {
                selectedConstruction.transform.position = buildingPool.position;
                selectedConstruction.SetActive(false);
                selectedConstruction = null;
                closestResource = null;
                CameraController.cameraControllerInstance.HoldingBuilding = false;
            }
            //If the left mouse button is clicked and the building can be placed then the bulding will stop following the hit position
            else if (Input.GetMouseButtonDown(0) & canBePlaced)
            {
                bool canAfford = false;
                float buildingEnergyCost, buildingMetalCost, buildingCrystalCost;

                //Gets a reference to the Building class attached to the object
                Building buildingScript = selectedConstruction.GetComponent<Building>();

                buildingEnergyCost = buildingScript.EnergyCost;
                buildingMetalCost = buildingScript.MetalCost;
                buildingCrystalCost = buildingScript.CrystalCost;

                if (buildingEnergyCost <= resourceManager.EnergyCurrent &&
                    buildingMetalCost <= resourceManager.MetalCurrent &&
                    buildingCrystalCost <= resourceManager.CrystalCurrent) canAfford = true;


                if (canAfford)
                {
                    //If the building was an extractor, sets the resource object the building was placed on as innactive
                    //Either way the PlaceBuilding method in the Builing class is called
                    if (selectedConstruction.CompareTag("MetalExtractor"))
                    {
                        buildingScript.PlaceBuilding(closestResource);
                        closestResource.SetActive(false);
                    }
                    else buildingScript.PlaceBuilding(null);

                    //Passes the placed building to the UIController script before setting it back to null
                    UIController.uiControllerInstance.PlaceBuilding(selectedConstruction);
                    selectedConstruction = null;
                    CameraController.cameraControllerInstance.HoldingBuilding = false;
                    resourceManager.UseResources(buildingEnergyCost, buildingMetalCost, buildingCrystalCost);
                }
                else
                {
                    StartCoroutine(ShowMessage());
                }
            }
        }
    }


    private IEnumerator ShowMessage()
    {
        infoText.text = "Not enough resources to build";
        yield return new WaitForSeconds(3f);
        infoText.text = "";
    }

    //Public method called by OnClick events from UI button objects in the scene
    //String parameter used to make up the file path for the selected building within the building pool
    //The building is then spawned into the scene and begins following the mouse cursor
    public void SelectConstruction(string building)
    {
        foreach (Transform b in buildingPool)
        {
            if (b.name == building & !b.gameObject.activeSelf)
            {
                selectedConstruction = b.gameObject;
                selectedConstruction.transform.position = Vector3.zero;
                selectedConstruction.SetActive(true);
                break;
            }
        }

        if (selectedConstruction.CompareTag("EnergyGenerator"))
        {
            selectedConstruction.transform.rotation = Quaternion.Euler(-90, 0, 0);
        }


        CameraController.cameraControllerInstance.HoldingBuilding = true;
    }


    public void Unpause()
    {
        paused = false;
    }
}
