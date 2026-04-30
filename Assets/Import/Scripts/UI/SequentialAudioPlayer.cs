using UnityEngine;
using System.Collections;

public class SequentialAudioPlayer : MonoBehaviour
{
    [Header("Аудио источники")]
    public AudioSource firstAudio;
    public AudioSource secondAudio;

    [Header("Настройки")]
    public float delayAfterFirst = 0f;

    void Start()
    {
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        if (firstAudio != null && firstAudio.clip != null)
        {
            firstAudio.Play();
            yield return new WaitForSeconds(firstAudio.clip.length);
        }

        if (delayAfterFirst > 0f)
        {
            yield return new WaitForSeconds(delayAfterFirst);
        }

        if (secondAudio != null)
        {
            secondAudio.Play();
        }
    }
}
