using UnityEngine;
using System.Collections;

public class VoiceAndSoundTrigger : MonoBehaviour
{
    public AudioSource audioMuzochku;
    public AudioSource audioPortalReset;
    public AudioSource audioPortalOpen;
    public AudioSource audioTestGame;
    public AudioSource musicHub;

    private bool triggered;

    void OnEnable()
    {
        triggered = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<SecMainCharacter>() == null || triggered) return;
        triggered = true;

        StopAllSounds();

        var gpm = GameProgressManager.Instance;
        bool trainDone = gpm.IsLevelCompleted("TrainL");
        bool l1Done = gpm.IsLevelCompleted("L1");
        bool l3Done = gpm.IsLevelCompleted("L3");
        bool dreamDone = gpm.IsLevelCompleted("DreamRunning");

        if (!trainDone && !l1Done)
        {
            StartCoroutine(SequenceBeforeTraining());
        }
        else if (trainDone && !l1Done)
        {
            PlayMusic();
            PlayOneShot(audioPortalOpen);
        }
        else if (l3Done && !dreamDone)
        {
            PlayMusic();
            PlayOneShot(audioTestGame);
        }
        else
        {
            PlayMusic();
        }
    }

    IEnumerator SequenceBeforeTraining()
    {
        yield return new WaitForSeconds(0.5f);
        
        PlayOneShot(audioMuzochku);
        yield return new WaitWhile(() => audioMuzochku.isPlaying);

        PlayMusic();
        
        yield return new WaitForSeconds(0.3f);
        
        PlayOneShot(audioPortalReset);
    }

    void PlayOneShot(AudioSource audio)
    {
        if (audio != null)
        {
            audio.loop = false;
            audio.Play();
        }
    }

    void PlayMusic()
    {
        if (musicHub != null && !musicHub.isPlaying)
        {
            musicHub.loop = true;
            musicHub.Play();
        }
    }

    void StopAllSounds()
    {
        if (audioMuzochku != null) audioMuzochku.Stop();
        if (audioPortalReset != null) audioPortalReset.Stop();
        if (audioPortalOpen != null) audioPortalOpen.Stop();
        if (audioTestGame != null) audioTestGame.Stop();
    }
}