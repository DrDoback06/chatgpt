using UnityEngine;

// Basic audio manager using a single AudioSource component.
// Consider expanding with multiple sources (music, sfx) and volume controls.
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    // Singleton Instantiation (Optional but often useful for managers)
    public static AudioManager Instance;

    [Header("Audio Clips")]
    [Tooltip("Default background music loop.")]
    [SerializeField] private AudioClip backgroundMusic;
    [Tooltip("Example sound effect clip.")]
    [SerializeField] private AudioClip genericSoundEffect; // Renamed for clarity
    [Tooltip("Example voice over clip.")]
    [SerializeField] private AudioClip genericVoiceOver; // Renamed for clarity

    [Header("Audio Sources (Recommended Split)")]
    [Tooltip("AudioSource dedicated to playing background music (loops).")]
    [SerializeField] private AudioSource musicSource; // Assign in Inspector
    [Tooltip("AudioSource used for one-shot sound effects (multiple plays overlap).")]
    [SerializeField] private AudioSource sfxSource; // Assign in Inspector
    // Add more sources if needed (e.g., voiceSource, uiSource)

    // --- Unity Lifecycle ---
    private void Awake()
    {
        // Singleton Setup
        if (Instance == null)
        {
            Instance = this;
             // Decide if AudioManager should persist across scenes
             // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // --- Validate AudioSource References ---
        // If not assigned in inspector, try to get them from this GameObject
        if (musicSource == null)
        {
             // Find sources or add them if missing
             AudioSource[] sources = GetComponents<AudioSource>();
             if (sources.Length > 0) musicSource = sources[0];
             else musicSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("AudioManager: Music Source not assigned, attempting to find/add one.", this);
            musicSource.playOnAwake = false; // Ensure default settings are suitable
             musicSource.loop = true;
        }

        if (sfxSource == null)
        {
            AudioSource[] sources = GetComponents<AudioSource>();
             // Look for a second source, or add one
             if (sources.Length > 1) sfxSource = sources[1];
             else sfxSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("AudioManager: SFX Source not assigned, attempting to find/add one.", this);
            sfxSource.playOnAwake = false;
             sfxSource.loop = false;
        }
         // Configure sources further (volume, spatial blend etc.) as needed
    }

    void Start()
    {
        // Automatically play background music on start if assigned
        if (backgroundMusic != null)
        {
            PlayBackgroundMusic(backgroundMusic);
        }
    }

    // --- Music Control ---

    /// <summary>
    /// Plays the specified background music track, stopping any current music.
    /// </summary>
    /// <param name="musicClip">The music track to play.</param>
    public void PlayBackgroundMusic(AudioClip musicClip)
    {
        if (musicSource != null && musicClip != null)
        {
            musicSource.Stop(); // Stop previous music
            musicSource.clip = musicClip;
            musicSource.loop = true; // Ensure looping is enabled
            musicSource.Play();
            Debug.Log($"AudioManager playing music: {musicClip.name}");
        }
         else if (musicSource == null) Debug.LogError("AudioManager: Music Source is not available!");
         else if (musicClip == null) Debug.LogWarning("AudioManager: PlayBackgroundMusic called with a null clip.");
    }

    public void PauseBackgroundMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

     public void ResumeBackgroundMusic()
    {
        if (musicSource != null && !musicSource.isPlaying) // Check if paused or stopped
        {
             // Might need logic here if completely stopped vs paused
             if (musicSource.time > 0) // Likely paused
                 musicSource.UnPause();
             else // Likely stopped, start from beginning
                 musicSource.Play();
        }
    }


    public void StopBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    // --- Sound Effect Control ---

    /// <summary>
    /// Plays a one-shot sound effect.
    /// </summary>
    /// <param name="sfxClip">The sound effect clip to play.</param>
     /// <param name="volumeScale">Optional volume multiplier.</param>
    public void PlaySoundEffect(AudioClip sfxClip, float volumeScale = 1.0f)
    {
        if (sfxSource != null && sfxClip != null)
        {
            sfxSource.PlayOneShot(sfxClip, volumeScale);
            // Debug.Log($"AudioManager playing SFX: {sfxClip.name}"); // Can be spammy
        }
        else if (sfxSource == null) Debug.LogError("AudioManager: SFX Source is not available!");
        else if (sfxClip == null) Debug.LogWarning("AudioManager: PlaySoundEffect called with a null clip.");
    }

     /// <summary>
     /// Plays a specific example sound effect (replace with specific methods or dictionary lookup).
     /// </summary>
     public void PlayGenericSoundEffect() { PlaySoundEffect(genericSoundEffect); }

    // --- Voice Over Control ---

    /// <summary>
    /// Plays a one-shot voice over clip (could use sfxSource or a dedicated voiceSource).
    /// </summary>
    /// <param name="voClip">The voice over clip.</param>
     /// <param name="volumeScale">Optional volume multiplier.</param>
    public void PlayVoiceOver(AudioClip voClip, float volumeScale = 1.0f)
    {
        // Use sfxSource for simplicity, or add a dedicated voiceSource
        if (sfxSource != null && voClip != null)
        {
            // Optional: Stop previous VO on this source? Depends on design.
            // sfxSource.Stop();
            sfxSource.PlayOneShot(voClip, volumeScale);
            Debug.Log($"AudioManager playing VO: {voClip.name}");
        }
         else if (sfxSource == null) Debug.LogError("AudioManager: SFX/Voice Source is not available!");
         else if (voClip == null) Debug.LogWarning("AudioManager: PlayVoiceOver called with a null clip.");
    }

     public void PlayGenericVoiceOver() { PlayVoiceOver(genericVoiceOver); }

}