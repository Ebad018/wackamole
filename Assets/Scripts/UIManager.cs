using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Screens")]
    public GameObject startScreen;
    public GameObject hudScreen;
    public GameObject endScreen;

    [Header("Text Displays")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI missText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalMissesText;

    [Header("Game Settings")]
    public float gameDuration = 15f;
    private float currentTime;

    private void Start()
    {
        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStatsChanged += UpdateStatsUI;
            GameManager.Instance.OnGameOver += ShowEndScreen;
        }

        ShowStartScreen();
    }

    private void Update()
    {
        // Handle Timer logic during Gameplay
        if (GameManager.Instance != null && GameManager.Instance.isGameActive)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();

            // Calculate difficulty progress (0.0 to 1.0)
            float progress = 1.0f - (currentTime / gameDuration);
            GameManager.Instance.UpdateDifficulty(progress);

            if (currentTime <= 0)
            {
                currentTime = 0;
                GameManager.Instance.EndGame();
            }
        }
    }

    // --- Screen Logic ---

    public void ShowStartScreen()
    {
        startScreen.SetActive(true);
        hudScreen.SetActive(false);
        endScreen.SetActive(false);
    }

    public void OnPlayButtonPressed()
    {
        currentTime = gameDuration;
        UpdateStatsUI();
        
        startScreen.SetActive(false);
        hudScreen.SetActive(true);
        
        GameManager.Instance.StartGame();
    }

    private void ShowEndScreen()
    {
        hudScreen.SetActive(false);
        endScreen.SetActive(true);

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Total Hits: {GameManager.Instance.score}";
        }
        
        if (finalMissesText != null)
        {
            finalMissesText.text = $"Total Misses: {GameManager.Instance.misses}";
        }
    }

    // --- UI Update Logic ---

    private void UpdateStatsUI()
    {
        if (scoreText != null) scoreText.text = $"Score: {GameManager.Instance.score}";
        if (missText != null) missText.text = $"Misses: {GameManager.Instance.misses}";
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = $"Time: {Mathf.CeilToInt(currentTime)}s";
        }
    }

    // --- Button Hooks ---

    public void OnRestartButtonPressed()
    {
        GameManager.Instance.RestartGame();
    }

    public void OnExitButtonPressed()
    {
        GameManager.Instance.ExitGame();
    }
}
