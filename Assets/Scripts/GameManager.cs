using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public GridSpawner gridSpawner;

    [Header("UI References (Optional)")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI missText;

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
        UpdateScoreUI();
        Debug.Log($"[GameManager] Score Hit! | Total Score: {score} | Total Misses: {misses}");
    }

    public void AddMiss()
    {
        if (!isGameActive) return;
        
        misses++;
        UpdateScoreUI();
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

        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = $"Score: {score}";
        if (missText != null) missText.text = $"Misses: {misses}";
    }

    public void RestartGame()
    {
        Debug.Log("[GameManager] Restarting Game...");
        // Re-loads the current active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        Debug.Log("[GameManager] Exiting Game...");
        // This only works in a built application
        Application.Quit();
        
        // If in Unity Editor, this will stop the play mode
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
