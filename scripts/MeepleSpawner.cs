using UnityEngine;

public class MeepleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject knightPrefab;
    [SerializeField] private GameObject citizenPrefab;
    [SerializeField] private GameObject monkPrefab;
    public static MeepleSpawner instance;
    private void Awake()
    {
        instance = this;
    }

    public void SpawnMeeple(IFeature feature, Vector3 position) //spawns meeple model based on feature type
    {

        Quaternion rotation = Quaternion.Euler(0, 90f, 0);
        if (feature is CityFeature)
        {
            Instantiate(knightPrefab, position, rotation);
        }
        else if (feature is RoadFeature)
        {
            Instantiate(citizenPrefab, position, rotation);
        }
        else if (feature is ChurchFeature)
        {
            Instantiate(monkPrefab, position, rotation);
        }
        else
        {
            Debug.LogError($"Unknown feature type: {feature.GetType().Name}");
        }
    }
}
