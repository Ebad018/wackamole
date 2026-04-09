using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Screens")]
    public GameObject startScreen;
    public GameObject hudScreen;
    public GameObject endScreen;
    public GameObject optionsScreen;

    [Header("Text Displays")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI missText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalMissesText;

    [Header("Options Settings")]
    public Slider masterVolSlider;
    public Slider musicVolSlider;
    public Slider sfxVolSlider;

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
        if (optionsScreen != null) optionsScreen.SetActive(false);
    }

    public void ShowOptionsScreen()
    {
        startScreen.SetActive(false);
        if (optionsScreen != null) optionsScreen.SetActive(true);
        
        // Sync sliders with current AudioManager state
        if (AudioManager.Instance != null)
        {
            if (masterVolSlider != null) masterVolSlider.value = AudioManager.Instance.masterVolume;
            if (musicVolSlider != null) musicVolSlider.value = AudioManager.Instance.musicVolume;
            if (sfxVolSlider != null) sfxVolSlider.value = AudioManager.Instance.sfxVolume;
        }
    }

    public void HideOptionsScreen()
    {
        if (optionsScreen != null) optionsScreen.SetActive(false);
        startScreen.SetActive(true);
    }

    public void OnVolumeChanged()
    {
        if (AudioManager.Instance != null && masterVolSlider != null && musicVolSlider != null && sfxVolSlider != null)
        {
            AudioManager.Instance.UpdateVolumes(masterVolSlider.value, musicVolSlider.value, sfxVolSlider.value);
        }
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
    
    public void OnPlayAgainButtonPressed()
    {
        currentTime = gameDuration;
        UpdateStatsUI();

        endScreen.SetActive(false);
        hudScreen.SetActive(true);
        
        GameManager.Instance.RestartGame();
    }

    public void OnExitButtonPressed()
    {
        GameManager.Instance.ExitGame();
    }
}
