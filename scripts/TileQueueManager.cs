using System.Collections.Generic;
using UnityEngine;

public class PlacementQueueManager : MonoBehaviour
{
    public static PlacementQueueManager Instance;
    private static GameManager GameManager => GameManager.instance;

    public string resourcesPath = "prefabs/Tiles"; // Path within Resources folder with tile prefabs
    public Vector3 offscreenSpawnPoint = new Vector3(-999, -999, -999); // Hide prefabs until placed

    private Queue<TilePlacer> tileQueue = new Queue<TilePlacer>();
    private TilePlacer currentPlacer;
    public TilePlacer current => currentPlacer;
    public bool isEnd = false;

    void Awake()
    {
        Instance = this;
        LoadAndEnqueuePrefabs();
    }

    private void LoadAndEnqueuePrefabs() //spawns all tiles and then enqueues them in random order with a static starting tile
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>(resourcesPath);

        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning($"No prefabs found at Resources/{resourcesPath}.");
            return;
        }

        List<TilePlacer> allPlacers = new List<TilePlacer>();
        TilePlacer startingPlacer = null;

        foreach (GameObject prefab in prefabs) //iterates through prefab objects in folder
        {
            TileData data = prefab.GetComponent<TileData>();
            int count = data.spawnCount;

            for (int i = 0; i < count; i++) //spawns desired number of the same tile
            {
                GameObject instance = Instantiate(prefab, offscreenSpawnPoint, Quaternion.identity);
                TilePlacer placer = instance.GetComponent<TilePlacer>();

                if (placer != null)
                {
                    placer.enabled = false;

                    if (data.isStartingTile && startingPlacer == null)
                    {
                        startingPlacer = placer;
                    }
                    else
                    {
                        allPlacers.Add(placer);
                    }
                }
                else
                {
                    Debug.LogWarning($"Prefab '{prefab.name}' does not have a TilePlacer component.");
                    Destroy(instance);
                }
            }
        }

        // Shuffle the rest
        Shuffle(allPlacers);

        // Enqueue the starting tile first
        if (startingPlacer != null)
        {
            EnqueueTile(startingPlacer);
        }
        else
        {
            Debug.LogWarning("No starting tile found. Queue will begin with random tile.");
        }

        // Then enqueue the rest
        foreach (var placer in allPlacers)
        {
            EnqueueTile(placer);
        }

        Debug.Log($"Loaded and enqueued {tileQueue.Count} tile prefabs (including starting tile).");
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public void EnqueueTile(TilePlacer placer)
    {
        placer.enabled = false; // Deactivate by default
        tileQueue.Enqueue(placer);

        if (currentPlacer == null)
        {
            ActivateNextTile();
        }
    }

    public void NotifyTilePlaced(TilePlacer placer)
    {
        if (placer == currentPlacer)
        {
            currentPlacer = null;
            ActivateNextTile();
        }
    }

    private void ActivateNextTile()
    {
        if (tileQueue.Count > 0)
        {
            currentPlacer = tileQueue.Dequeue();
            currentPlacer.enabled = true;
        }
        else
        {
            Debug.Log("All tiles placed!");
            TilePlacer.isGameEnd = true;
            GameManager.GameEnd();
        }
    }
}
