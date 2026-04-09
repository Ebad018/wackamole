using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public GridSpawner gridSpawner;

    public Action OnStatsChanged; // Notify UI when score/miss changes
    public Action OnGameOver;     // Notify UI when time ends

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
        OnStatsChanged?.Invoke();
        Debug.Log($"[GameManager] Score Hit! | Total Score: {score} | Total Misses: {misses}");
    }

    public void AddMiss()
    {
        if (!isGameActive) return;
        
        misses++;
        OnStatsChanged?.Invoke();
        Debug.Log($"[GameManager] Missed! | Total Score: {score} | Total Misses: {misses}");
    }

    public void StartGame()
    {
        if (isGameActive) return;

        isGameActive = true;
        score = 0;
        misses = 0;
        
        OnStatsChanged?.Invoke();
        UpdateDifficulty(0); // Reset difficulty to start state
        Debug.Log("[GameManager] Game Started! Get ready to whack!");

        if (gridSpawner != null)
        {
            gridSpawner.StartGameSpawning();
        }
    }

    public void EndGame()
    {
        if (!isGameActive) return;

        isGameActive = false;
        
        if (gridSpawner != null)
        {
            gridSpawner.ShowAllHits();
        }

        OnGameOver?.Invoke();
        Debug.Log("[GameManager] Game Over! Final Score: " + score);
    }

    public void UpdateDifficulty(float progress)
    {
        if (gridSpawner != null)
        {
            gridSpawner.ApplyDifficulty(progress);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
