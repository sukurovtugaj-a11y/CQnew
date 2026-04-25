using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public VideoClip[] videos;
    public static VideoController Instance;

    public static int videoToPlay = -1;
    public static bool autoReturn = false;
    public static string currentLevelForVideo = null;
    public static bool introJustPlayed = false;
    public static bool spawnAtIntroZone = false;
    public static bool wokyJustPlayed = false;
    public static bool spawnAtWokyZone = false;

    void Awake()
    {
        Instance = this;
        videoPlayer.loopPointReached += OnVideoEnded;
    }

    void Start()
    {
        var rt = new RenderTexture(Screen.width, Screen.height, 24);
        videoPlayer.targetTexture = rt;

        var rawImage = FindObjectOfType<UnityEngine.UI.RawImage>();
        if (rawImage != null) rawImage.texture = rt;

        if (videoToPlay >= 0)
        {
            PlayVideoByIndex(videoToPlay);
            videoToPlay = -1;
        }
    }

    public void PlayVideoByIndex(int index)
    {
        if (index >= 0 && index < videos.Length)
        {
            videoPlayer.clip = videos[index];
            videoPlayer.Play();
        }
    }

    void OnVideoEnded(VideoPlayer vp)
    {
        if (autoReturn)
        {
            bool isWoky = videoPlayer.clip == videos[5];
            string levelForSignal = currentLevelForVideo;
            autoReturn = false;

            if (!string.IsNullOrEmpty(currentLevelForVideo))
            {
                GameProgressManager.Instance.MarkVideoWatched(currentLevelForVideo);
                currentLevelForVideo = null;
            }

            introJustPlayed = !isWoky && spawnAtIntroZone;
            wokyJustPlayed = isWoky && spawnAtWokyZone;
            spawnAtIntroZone = false;
            spawnAtWokyZone = false;

if (introJustPlayed)
            {
                PlayerPrefs.SetInt("IntroWatched", 1);
                PlayerPrefs.Save();
            }

            Time.timeScale = 1;
            AudioListener.pause = false;

            SceneManager.LoadScene("MainScene");
        }
    }
}