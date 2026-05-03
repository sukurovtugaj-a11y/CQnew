using UnityEngine;

/// <summary>
/// Звуковой триггер-зона: пока игрок внутри — звук играет, вышел — звук выключается.
/// Проверка только по компоненту SecMainCharacter.
/// </summary>
public class ZoneSoundTrigger : MonoBehaviour
{
    [Header("Настройки звука")]
    public AudioSource audioSource;

    private int playersInside = 0;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Убеждаемся, что звук выключен в начале
        if (audioSource != null)
            audioSource.Stop();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем только по наличию компонента SecMainCharacter
        if (other.GetComponent<SecMainCharacter>() == null) return;

        playersInside++;

        // Если это первый игрок в зоне — включаем звук
        if (playersInside == 1 && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
            Debug.Log($"[{name}] Звук включен (игрок вошел)");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Проверяем только по наличию компонента SecMainCharacter
        if (other.GetComponent<SecMainCharacter>() == null) return;

        playersInside = Mathf.Max(0, playersInside - 1);

        // Если игроков в зоне не осталось — выключаем звук
        if (playersInside == 0 && audioSource != null)
        {
            audioSource.Stop();
            Debug.Log($"[{name}] Звук выключен (игрок вышел)");
        }
    }

    void OnDisable()
    {
        // На всякий случай выключаем звук, если объект отключается
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }
}
