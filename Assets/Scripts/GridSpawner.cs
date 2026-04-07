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

    private List<Mole> allMoles = new List<Mole>();


    void Start()
    {
        GenerateGrid();
        StartCoroutine(SpawnRoutine());
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

            List<Mole> hiddenMoles = allMoles.FindAll(m => m.IsHidden);
            
            if (hiddenMoles.Count > 0)
            {
                int index = Random.Range(0, hiddenMoles.Count);
                hiddenMoles[index].PopUp();
            }
        }
    }
}
