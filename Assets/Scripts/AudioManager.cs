using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource bgmSourceA;
    public AudioSource bgmSourceB;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip bgmClip;
    public AudioClip[] moleHitSounds;
    public AudioClip[] moleMissSounds;
    public AudioClip bombHitSound;

    [Header("BGM Fade Settings")]
    public float fadeDuration = 2f;
    public float timeBeforeEndToFade = 2.5f; // When to start fading cross

    [Header("Volume Controls")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private bool isUsingSourceA = true;
    private Coroutine crossfadeRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ApplyVolumes();
        if (bgmClip != null)
        {
            StartBGM();
        }
    }

    private void Update()
    {
        // Handle crossfading background music loop
        if (bgmClip != null)
        {
            AudioSource activeSource = isUsingSourceA ? bgmSourceA : bgmSourceB;
            if (activeSource.isPlaying && activeSource.clip != null)
            {
                float timeRemaining = activeSource.clip.length - activeSource.time;
                if (timeRemaining <= timeBeforeEndToFade && crossfadeRoutine == null)
                {
                    crossfadeRoutine = StartCoroutine(CrossfadeBGM());
                }
            }
        }
    }

    public void UpdateVolumes(float master, float music, float sfx)
    {
        masterVolume = master;
        musicVolume = music;
        sfxVolume = sfx;
        ApplyVolumes();
    }

    private void ApplyVolumes()
    {
        AudioListener.volume = masterVolume;
        
        float actualMusicVolume = musicVolume;
        if (bgmSourceA != null) bgmSourceA.volume = isUsingSourceA ? actualMusicVolume : 0f;
        if (bgmSourceB != null) bgmSourceB.volume = !isUsingSourceA ? actualMusicVolume : 0f;
        
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    private void StartBGM()
    {
        bgmSourceA.clip = bgmClip;
        bgmSourceA.volume = musicVolume;
        bgmSourceA.Play();
        
        bgmSourceB.clip = bgmClip;
        bgmSourceB.volume = 0f;
        
        isUsingSourceA = true;
        crossfadeRoutine = null;
    }

    private IEnumerator CrossfadeBGM()
    {
        AudioSource oldSource = isUsingSourceA ? bgmSourceA : bgmSourceB;
        AudioSource newSource = isUsingSourceA ? bgmSourceB : bgmSourceA;

        newSource.clip = bgmClip;
        newSource.time = 0f;
        newSource.volume = 0f;
        newSource.Play();

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            
            oldSource.volume = Mathf.Lerp(musicVolume, 0f, t);
            newSource.volume = Mathf.Lerp(0f, musicVolume, t);
            yield return null;
        }

        oldSource.Stop();
        oldSource.volume = 0f;
        newSource.volume = musicVolume;

        isUsingSourceA = !isUsingSourceA;
        crossfadeRoutine = null;
    }

    public void PlayMoleHit()
    {
        if (moleHitSounds != null && moleHitSounds.Length > 0 && sfxSource != null)
        {
            AudioClip clip = moleHitSounds[Random.Range(0, moleHitSounds.Length)];
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void PlayMoleMiss()
    {
        if (moleMissSounds != null && moleMissSounds.Length > 0 && sfxSource != null)
        {
            AudioClip clip = moleMissSounds[Random.Range(0, moleMissSounds.Length)];
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void PlayBombHit()
    {
        if (bombHitSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(bombHitSound, sfxVolume);
        }
    }

    public float GetBombHitDuration()
    {
        return bombHitSound != null ? bombHitSound.length : 1f;
    }
}
