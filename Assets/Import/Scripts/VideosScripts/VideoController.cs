using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public VideoClip[] videos; // Все 5 видео
    public static VideoController Instance;

    void Awake()
    {
        Instance = this;
        videoPlayer.loopPointReached += OnVideoEnded;
    }

    // Метод для запуска конкретного видео
    public void PlayVideoByIndex(int index)
    {
        if (index >= 0 && index < videos.Length)
        {
            videoPlayer.clip = videos[index];
            videoPlayer.Play();
        }
    }

    // Для плейлиста (опционально)
    private int currentIndex = -1;

    public void PlayPlaylistStartingFrom(int startIndex)
    {
        currentIndex = startIndex;
        PlayVideoByIndex(currentIndex);
    }

    void OnVideoEnded(VideoPlayer vp)
    {
        // Если нужно авто-переключение
        if (currentIndex >= 0)
        {
            currentIndex++;
            if (currentIndex < videos.Length)
                PlayVideoByIndex(currentIndex);
        }
    }
}