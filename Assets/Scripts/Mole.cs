using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems; 

public class Mole : MonoBehaviour, IPointerDownHandler
{
    [Header("Mole Settings")]
    public float popUpDuration = 1.5f;
    public float resultDisplayDuration = 0.5f;

    private float basePopUpDuration;
    
    [Header("Sprites")]
    public Sprite emptyHoleSprite;
    public Sprite hitableSprite;
    public Sprite hitSprite;
    public Sprite missSprite;

    private SpriteRenderer spriteRenderer;
    private Collider2D col2D;
    
    public bool IsHidden { get; private set; } = true;

    private Coroutine currentCoroutine;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col2D = GetComponent<Collider2D>();
        
        basePopUpDuration = popUpDuration;
        SetStateEmpty();
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        // multiplier of 2.0 means twice as fast (half the duration)
        popUpDuration = basePopUpDuration / multiplier;
    }

    // Spawn Setup and Hitable Sprite Switch
    public void PopUp()
    {
        if (!IsHidden) return;
        
        IsHidden = false;
        
        if (spriteRenderer != null) spriteRenderer.sprite = hitableSprite;
        if (col2D != null) col2D.enabled = true;
        
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(TimerToMiss());
    }

    // Whack Detection and Hit Sprite Switch
    public void Whack()
    {
        if (IsHidden) return;
        
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        
        if (spriteRenderer != null) spriteRenderer.sprite = hitSprite;
        if (col2D != null) col2D.enabled = false;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore();
        }
        else
        {
            Debug.LogWarning("GameManager not found in scene! Missing Score feature.");
            Debug.Log("Whack! (Hit)");
        }
        
        currentCoroutine = StartCoroutine(ShowResultAndReset());
    }

    // Miss Timer and Miss Sprite Switch
    private IEnumerator TimerToMiss()
    {
        yield return new WaitForSeconds(popUpDuration);
        
        if (spriteRenderer != null) spriteRenderer.sprite = missSprite;
        if (col2D != null) col2D.enabled = false;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMiss();
        }
        else
        {
            Debug.LogWarning("GameManager not found in scene! Missing Miss feature.");
            Debug.Log("Miss! (Timeout)");
        }

        currentCoroutine = StartCoroutine(ShowResultAndReset());
    }

    // Reset Delay
    private IEnumerator ShowResultAndReset()
    {
        yield return new WaitForSeconds(resultDisplayDuration);
        SetStateEmpty();
    }

    // Empty State Setup
    private void SetStateEmpty()
    {
        IsHidden = true;
        
        if (spriteRenderer != null) spriteRenderer.sprite = emptyHoleSprite;
        if (col2D != null) col2D.enabled = false;
    }

    // Force the mole to show its hit state and stop all logic
    public void ForceShowHit()
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        
        IsHidden = false;
        if (spriteRenderer != null) spriteRenderer.sprite = hitSprite;
        if (col2D != null) col2D.enabled = false;
    }
    
    // Input Detection
    public void OnPointerDown(PointerEventData eventData)
    {
        Whack();
    }
}
