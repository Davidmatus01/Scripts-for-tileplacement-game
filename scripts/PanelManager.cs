using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public static PanelManager instance;
    public GameObject buttonPanel;
    private IFeature cityFeature;
    private IFeature roadFeature;
    private IFeature churchFeature;
    public GameObject cityButton;
    public GameObject roadButton;
    public GameObject churchButton;
    public GameObject endPannel;
    public TextMeshProUGUI endGameText;

    public void ShowFeatures(List<IFeature> features) // Method for showing available features for meeple placement
    {
        buttonPanel.SetActive(true);

        cityButton.SetActive(false);
        roadButton.SetActive(false);
        churchButton.SetActive(false);

        foreach (var feature in features)
        {
            if (feature is CityFeature city)
            {
                cityFeature = city;
                cityButton.SetActive(true);
            }
            else if (feature is RoadFeature road)
            {
                roadFeature = road;
                roadButton.SetActive(true);
            }
            else if (feature is ChurchFeature church)
            {
                churchFeature = church;
                churchButton.SetActive(true);
            }
        }
    }

    public void ShowEndPanel(string text)
    {
        endPannel.SetActive(true);
        endGameText.text = text;
    }

    public void OnCityClicked()
    {
        MeeplePlacementManager.instance.PlaceMeeple(cityFeature);
        buttonPanel.SetActive(false);
    }

    public void OnRoadClicked()
    {
        MeeplePlacementManager.instance.PlaceMeeple(roadFeature);
        buttonPanel.SetActive(false);
    }

    public void OnChurchClicked()
    {
        MeeplePlacementManager.instance.PlaceMeeple(churchFeature);
        buttonPanel.SetActive(false);
    }
    public void onCancelClicked()
    {
        buttonPanel.SetActive(false);
    }

    void Awake()
    {
        instance = this;
    }
}
