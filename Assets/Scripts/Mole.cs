using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems; 

public class Mole : MonoBehaviour, IPointerDownHandler
{
    [Header("Mole Settings")]
    public float popUpDuration = 1.5f;
    public float resultDisplayDuration = 0.5f;

    private float basePopUpDuration;
    
    [Header("Mole Sprites")]
    public Sprite emptyHoleSprite;
    public Sprite hitableSprite;
    public Sprite hitSprite;
    public Sprite missSprite;

    [Header("Bomb Sprites")]
    public Sprite hitableBombSprite;
    public Sprite hitBomb0Sprite;
    public Sprite hitBombSprite;
    public Sprite explodedBombSprite;
    public Sprite missBombSprite;

    [Header("Bomb Sprites Optional")]
    [Tooltip("If empty, uses the normal emptyHoleSprite")]
    public Sprite emptyBombHoleSprite; 

    [Header("Bomb Animation Settings")]
    public float pulseDuration = 0.3f;
    public float pulseScaleBig = 1.2f;
    public float pulseScaleNormal = 1.0f;
    public float finalExpandScale = 1.5f;

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer generatedBackgroundHole;
    private Collider2D col2D;
    
    public bool IsHidden { get; private set; } = true;
    public bool IsPermanentlyExploded { get; private set; } = false;
    private bool isBomb = false;

    private Coroutine currentCoroutine;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col2D = GetComponent<Collider2D>();
        
        // Dynamically create a background hole renderer so you don't have to make one in the Editor
        GameObject bgObj = new GameObject("BackgroundHole");
        bgObj.transform.SetParent(this.transform);
        bgObj.transform.localPosition = Vector3.zero;
        
        generatedBackgroundHole = bgObj.AddComponent<SpriteRenderer>();
        generatedBackgroundHole.sprite = emptyBombHoleSprite != null ? emptyBombHoleSprite : emptyHoleSprite;
        generatedBackgroundHole.sortingLayerID = spriteRenderer.sortingLayerID;
        generatedBackgroundHole.sortingOrder = spriteRenderer.sortingOrder - 1; // Always place it behind the main sprite
        generatedBackgroundHole.enabled = false;
        
        basePopUpDuration = popUpDuration;
        SetStateEmpty();
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        popUpDuration = basePopUpDuration / multiplier;
    }

    public void PopUp(bool spawnAsBomb = false)
    {
        if (!IsHidden || IsPermanentlyExploded) return;
        
        IsHidden = false;
        isBomb = spawnAsBomb;
        
        if (spriteRenderer != null) 
            spriteRenderer.sprite = isBomb ? hitableBombSprite : hitableSprite;
            
        // Turn on the dynamically generated hole if this is a bomb
        if (isBomb && generatedBackgroundHole != null)
            generatedBackgroundHole.enabled = true;
            
        if (col2D != null) col2D.enabled = true;
        
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(TimerToMiss());
    }

    public void Whack()
    {
        if (IsHidden || IsPermanentlyExploded) return;
        
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        if (col2D != null) col2D.enabled = false;
        
        if (isBomb)
        {
            IsPermanentlyExploded = true;
            
            currentCoroutine = StartCoroutine(BombExplosionSequence());
        }
        else
        {
            if (spriteRenderer != null) spriteRenderer.sprite = hitSprite;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMoleHit();
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore();
            }
            
            currentCoroutine = StartCoroutine(ShowResultAndReset());
        }
    }

    private IEnumerator TimerToMiss()
    {
        yield return new WaitForSeconds(popUpDuration);
        
        if (col2D != null) col2D.enabled = false;

        if (isBomb)
        {
            if (spriteRenderer != null) spriteRenderer.sprite = missBombSprite;
            
            // Dodging a bomb gives +1 score
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore();
            }
        }
        else
        {
            if (spriteRenderer != null) spriteRenderer.sprite = missSprite;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMoleMiss();
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddMiss();
            }
        }

        currentCoroutine = StartCoroutine(ShowResultAndReset());
    }

    private IEnumerator PulseScale(float targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = new Vector3(targetScale, targetScale, targetScale);
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            t = t * t * (3f - 2f * t); // Smooth step for nicer pulse
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        transform.localScale = endScale;
    }

    private IEnumerator BombExplosionSequence()
    {
        // Stage 1: Hit 0 - Pulse 1
        if (spriteRenderer != null) spriteRenderer.sprite = hitBomb0Sprite;
        yield return StartCoroutine(PulseScale(pulseScaleBig, pulseDuration / 2f));
        yield return StartCoroutine(PulseScale(pulseScaleNormal, pulseDuration / 2f));
        
        // Stage 2: Hit 1 - Pulse 2
        if (spriteRenderer != null) spriteRenderer.sprite = hitBombSprite;
        yield return StartCoroutine(PulseScale(pulseScaleBig + 0.1f, pulseDuration / 2f));
        yield return StartCoroutine(PulseScale(pulseScaleNormal, pulseDuration / 2f));
        
        // Stage 3: Audio and Final Expansion
        float audioDuration = 1f;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBombHit();
            audioDuration = AudioManager.Instance.GetBombHitDuration();
        }
        
        yield return StartCoroutine(PulseScale(finalExpandScale, audioDuration));
        
        // Stage 4: Exploded
        if (spriteRenderer != null) spriteRenderer.sprite = explodedBombSprite;
        transform.localScale = new Vector3(pulseScaleNormal, pulseScaleNormal, pulseScaleNormal); // Reset scale
        
        IsHidden = false; // Keeps it "visible" and unavailable in the spawner
        
        if (GameManager.Instance != null && GameManager.Instance.gridSpawner != null)
        {
            GameManager.Instance.gridSpawner.CheckGridDestroyed();
        }
    }

    private IEnumerator ShowResultAndReset()
    {
        yield return new WaitForSeconds(resultDisplayDuration);
        SetStateEmpty();
    }

    private void SetStateEmpty()
    {
        if (IsPermanentlyExploded) return;

        IsHidden = true;
        isBomb = false;
        
        if (spriteRenderer != null) spriteRenderer.sprite = emptyHoleSprite;
        if (generatedBackgroundHole != null) generatedBackgroundHole.enabled = false;
        if (col2D != null) col2D.enabled = false;
    }

    public void ForceShowHit()
    {
        if (IsPermanentlyExploded) return;
        
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        
        IsHidden = false;
        if (spriteRenderer != null) spriteRenderer.sprite = hitSprite;
        if (col2D != null) col2D.enabled = false;
    }

    public void HideAndReset()
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        IsPermanentlyExploded = false;
        transform.localScale = new Vector3(pulseScaleNormal, pulseScaleNormal, pulseScaleNormal); // Reset scale here as well to fix weird bugs on new games
        if (generatedBackgroundHole != null) generatedBackgroundHole.enabled = false; // Reset the bomb background
        SetStateEmpty();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        Whack();
    }
}
