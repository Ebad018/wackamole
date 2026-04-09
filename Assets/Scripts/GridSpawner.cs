using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    [Header("Grid Settings")]
    public int rows = 3;
    public int columns = 3;
    public float spacing = 2.0f;
    
    [Header("Prefabs")]
    public GameObject holePrefab;
    
    [Header("Randomizer Settings")]
    public float minSpawnTime = 0.5f;
    public float maxSpawnTime = 2.0f;
    
    [Header("Difficulty Scaling")]
    public float maxSpeedMultiplier = 2.0f;
    public int maxConcurrentMoles = 3;

    private List<Mole> allMoles = new List<Mole>();
    private float baseMinSpawnTime;
    private float baseMaxSpawnTime;
    private int currentMaxConcurrent = 1;


    void Start()
    {
        baseMinSpawnTime = minSpawnTime;
        baseMaxSpawnTime = maxSpawnTime;
        GenerateGrid();
    }

    public void StartGameSpawning()
    {
        StartCoroutine(SpawnRoutine());
    }

    public void StopGameSpawning()
    {
        StopAllCoroutines();
    }

    public void ApplyDifficulty(float progress)
    {
        // Calculate multipliers based on 0-1 progress
        float speedMultiplier = Mathf.Lerp(1f, maxSpeedMultiplier, progress);
        currentMaxConcurrent = Mathf.FloorToInt(Mathf.Lerp(1, maxConcurrentMoles + 0.99f, progress));

        // Scale spawn frequency
        minSpawnTime = baseMinSpawnTime / speedMultiplier;
        maxSpawnTime = baseMaxSpawnTime / speedMultiplier;

        // Scale individual mole pop-up duration
        foreach (Mole mole in allMoles)
        {
            mole.SetSpeedMultiplier(speedMultiplier);
        }
    }

    public void ShowAllHits()
    {
        StopGameSpawning();
        foreach (Mole mole in allMoles)
        {
            mole.ForceShowHit();
        }
    }

    // Grid Setup
    void GenerateGrid()
    {
        float startX = -(columns - 1) * spacing / 2f;
        float startY = (rows - 1) * spacing / 2f;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 spawnPos = new Vector3(startX + (col * spacing), startY - (row * spacing), 0);
                
                GameObject newHole = Instantiate(holePrefab, spawnPos, Quaternion.identity, this.transform);
                newHole.name = $"Hole_{row}_{col}";

                Mole mole = newHole.GetComponentInChildren<Mole>();
                if (mole != null)
                {
                    allMoles.Add(mole);
                }
                else 
                {
                    Debug.LogWarning($"No Mole component found in {newHole.name}!");
                }
            }
        }
    }

    // Mole Popup Randomizer
    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            // Spawn multiple moles depending on current difficulty
            int molesToSpawn = Random.Range(1, currentMaxConcurrent + 1);
            
            for (int i = 0; i < molesToSpawn; i++)
            {
                List<Mole> hiddenMoles = allMoles.FindAll(m => m.IsHidden);
            
                if (hiddenMoles.Count > 0)
                {
                    int index = Random.Range(0, hiddenMoles.Count);
                    hiddenMoles[index].PopUp();
                }
            }
        }
    }
}
