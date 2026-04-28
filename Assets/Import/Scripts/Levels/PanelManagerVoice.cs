using UnityEngine;
using System.Collections;

public class PanelManagerVoice : MonoBehaviour
{
    public AudioSource audioSource;

    public void StartVoice()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
            
            // Блокируем управление: длительность звука + 1.5 сек
            float lockTime = audioSource.clip.length + 1.5f;
            var player = FindObjectOfType<SecMainCharacter>();
            if (player != null)
                player.controlLockTimer = lockTime;
            
            StartCoroutine(WaitForVoiceEnd());
        }
    }

    private IEnumerator WaitForVoiceEnd()
    {
        yield return new WaitUntil(() => !audioSource.isPlaying);
        OpenFloors();
        
        if (GameProgressManager.Instance != null)
            GameProgressManager.Instance.MarkVoiceoverPlayed();
    }

    public void StopVoice()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    public void OpenFloors()
    {
        var mover = FindObjectOfType<OPollMover>();
        if (mover != null) mover.StartMoving();
    }
}