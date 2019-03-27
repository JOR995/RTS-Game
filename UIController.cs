using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum PanelType { empty, building, factory, unit, engineer, pause }

public class UIController : MonoBehaviour
{
    public static UIController uiControllerInstance;

    [SerializeField]
    GameObject emptyPanel, constructionPanel, buildingPanel, factoryPanel, endPanel;

    private Text panelName, panelHealth, panelTitle;
    private List<GameObject> selectedObjects;


    void Awake()
    {
        uiControllerInstance = this;
    }

    //Sets initial states of all panel objects to ensure correct one is active on startup
    void Start()
    {
        emptyPanel.SetActive(true);
        constructionPanel.SetActive(false);
        buildingPanel.SetActive(false);
        factoryPanel.SetActive(false);
        endPanel.SetActive(false);
        panelTitle = endPanel.transform.GetChild(1).GetComponent<Text>();
        selectedObjects = new List<GameObject>();
        Time.timeScale = 1;
    }

    /// <summary>
    /// When the active panel needs to be changed
    /// Disables and enables panel objects dependent on the Paneltype parameter
    /// </summary>
    /// <param name="type"></param>
    /// <param name="objects"></param>
    public void ChangePanel(PanelType type, List<GameObject> objects)
    {
        selectedObjects = objects;

        switch (type)
        {
            case PanelType.empty:
                emptyPanel.SetActive(true);
                constructionPanel.SetActive(false);
                buildingPanel.SetActive(false);
                factoryPanel.SetActive(false);
                endPanel.SetActive(false);
                break;
            case PanelType.building:
                emptyPanel.SetActive(false);
                constructionPanel.SetActive(false);
                buildingPanel.SetActive(true);
                factoryPanel.SetActive(false);
                endPanel.SetActive(false);

                panelName = buildingPanel.transform.GetChild(0).GetComponent<Text>();
                panelName.text = selectedObjects[0].transform.tag;

                panelHealth= buildingPanel.transform.GetChild(1).GetComponent<Text>();
                panelHealth.text = selectedObjects[0].GetComponent<Building>().Health.ToString();
                break;
            case PanelType.factory:
                emptyPanel.SetActive(false);
                constructionPanel.SetActive(false);
                buildingPanel.SetActive(false);
                factoryPanel.SetActive(true);
                endPanel.SetActive(false);
                break;
            case PanelType.unit:
                emptyPanel.SetActive(false);
                constructionPanel.SetActive(false);
                buildingPanel.SetActive(true);
                factoryPanel.SetActive(false);
                endPanel.SetActive(false);

                panelName = buildingPanel.transform.GetChild(0).GetComponent<Text>();
                panelName.text = selectedObjects[0].transform.tag;

                panelHealth = buildingPanel.transform.GetChild(1).GetComponent<Text>();
                panelHealth.text = selectedObjects[0].GetComponent<Unit>().Health.ToString();
                break;
            case PanelType.engineer:
                emptyPanel.SetActive(false);
                constructionPanel.SetActive(true);
                buildingPanel.SetActive(false);
                factoryPanel.SetActive(false);
                endPanel.SetActive(false);
                break;
            case PanelType.pause:
                emptyPanel.SetActive(false);
                constructionPanel.SetActive(false);
                buildingPanel.SetActive(false);
                factoryPanel.SetActive(false);
                endPanel.SetActive(true);
                panelTitle.text = "Paused";
                Time.timeScale = 0;
                break;
        }
    }

    //Tells the selected factory object to create the chosen unit
    public void ConstructUnit(string name)
    {
        selectedObjects[0].GetComponent<Factory_Player>().CreateUnit(name);
    }

    //Tells the selected engineer object to create the chosen building
    public void PlaceBuilding(GameObject building)
    {
        selectedObjects[0].GetComponent<Engineer>().Construct(building);
    }

    //Called when the core building is destroyed and the player has lost
    //Freezes the game and dispplays the end panel
    public void EndGame()
    {
        emptyPanel.SetActive(false);
        constructionPanel.SetActive(false);
        buildingPanel.SetActive(false);
        factoryPanel.SetActive(false);
        endPanel.SetActive(true);
        endPanel.transform.GetChild(0).GetComponent<Button>().interactable = false;
        panelTitle.text = "Game Over";
        Time.timeScale = 0;
    }

    //Methods called from button click events
    public void OnContinueClick()
    {
        emptyPanel.SetActive(true);
        constructionPanel.SetActive(false);
        buildingPanel.SetActive(false);
        factoryPanel.SetActive(false);
        endPanel.SetActive(false);
        Time.timeScale = 1;
        PlayerInteraction.playerInteractionInstance.Unpause();
    }


    public void OnRestartClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void OnMenuClick()
    {
        SceneManager.LoadScene("MainMenu");
    }


    public void OnQuitClick()
    {
        Application.Quit();
    }
}
