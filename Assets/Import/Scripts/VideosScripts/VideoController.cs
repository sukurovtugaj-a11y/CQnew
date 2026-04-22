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
            bool isIntro = videoPlayer.clip == videos[0];
            autoReturn = false;

            if (!string.IsNullOrEmpty(currentLevelForVideo))
            {
                GameProgressManager.Instance.MarkVideoWatched(currentLevelForVideo);
                currentLevelForVideo = null;
            }

            introJustPlayed = isIntro && spawnAtIntroZone;
            spawnAtIntroZone = false;
            SceneManager.LoadScene("MainScene");
        }
    }
}