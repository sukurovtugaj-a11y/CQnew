using UnityEngine;

public class PlayerSoundComponent : MonoBehaviour
{
    // Звуки
    public AudioClip[] walkSounds;
    public AudioClip[] boostSounds;
    public AudioClip[] grassWalkSounds;
    public AudioClip[] grassBoostSounds;

    // Настройки
    public float walkSoundInterval = 0.4f;
    public float boostSoundInterval = 0.3f;

    private AudioSource audioSource;
    private float soundTimer;
    private bool isOnGrass;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void SetGrassState(bool onGrass)
    {
        if (isOnGrass != onGrass)
        {
            isOnGrass = onGrass;
            soundTimer = 0f; // Сбрасываем таймер при смене поверхности
        }
    }

    public void UpdateSounds(bool isMoving, bool isGrounded, bool isBoosting)
    {
        if (!isGrounded || !isMoving || audioSource == null)
        {
            soundTimer = 0f;
            return;
        }

        float interval = isBoosting ? boostSoundInterval : walkSoundInterval;
        soundTimer -= Time.deltaTime;

        if (soundTimer <= 0f)
        {
            PlayRandomSound(isBoosting);
            soundTimer = interval;
        }
    }

    private void PlayRandomSound(bool isBoosting)
    {
        AudioClip[] currentArray = null;

        if (isOnGrass)
        {
            currentArray = isBoosting ? grassBoostSounds : grassWalkSounds;
        }
        else
        {
            currentArray = isBoosting ? boostSounds : walkSounds;
        }

        if (currentArray == null || currentArray.Length == 0) return;

        int index = Random.Range(0, currentArray.Length);
        AudioClip clip = currentArray[index];

        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
