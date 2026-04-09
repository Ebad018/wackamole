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

    private SpriteRenderer spriteRenderer;
    private Collider2D col2D;
    
    public bool IsHidden { get; private set; } = true;
    public bool IsPermanentlyExploded { get; private set; } = false;
    private bool isBomb = false;

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
        popUpDuration = basePopUpDuration / multiplier;
    }

    public void PopUp(bool spawnAsBomb = false)
    {
        if (!IsHidden || IsPermanentlyExploded) return;
        
        IsHidden = false;
        isBomb = spawnAsBomb;
        
        if (spriteRenderer != null) 
            spriteRenderer.sprite = isBomb ? hitableBombSprite : hitableSprite;
            
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
            if (AudioManager.Instance != null) AudioManager.Instance.PlayBombHit();
            
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

    private IEnumerator BombExplosionSequence()
    {
        if (spriteRenderer != null) spriteRenderer.sprite = hitBomb0Sprite;
        yield return new WaitForSeconds(0.15f);
        
        if (spriteRenderer != null) spriteRenderer.sprite = hitBombSprite;
        yield return new WaitForSeconds(0.15f);
        
        if (spriteRenderer != null) spriteRenderer.sprite = explodedBombSprite;
        
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
        SetStateEmpty();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        Whack();
    }
}
