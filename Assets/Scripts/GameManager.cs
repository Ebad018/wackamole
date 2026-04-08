using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public GridSpawner gridSpawner;

    [Header("Game Stats")]
    public int score = 0;
    public int misses = 0;
    public bool isGameActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore()
    {
        if (!isGameActive) return;

        score++;
        Debug.Log($"[GameManager] Score Hit! | Total Score: {score} | Total Misses: {misses}");
    }

    public void AddMiss()
    {
        if (!isGameActive) return;
        
        misses++;
        Debug.Log($"[GameManager] Missed! | Total Score: {score} | Total Misses: {misses}");
    }

    public void StartGame()
    {
        if (isGameActive) return;

        isGameActive = true;
        score = 0;
        misses = 0;
        
        Debug.Log("[GameManager] Game Started! Get ready to whack!");

        if (gridSpawner != null)
        {
            gridSpawner.StartGameSpawning();
        }
        else
        {
            Debug.LogError("[GameManager] GridSpawner reference is missing! Cannot start spawning.");
        }
    }
}
