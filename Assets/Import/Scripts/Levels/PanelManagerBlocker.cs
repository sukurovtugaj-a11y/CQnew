using UnityEngine;

public class PanelManagerBlocker : MonoBehaviour
{
    void Start()
    {
        if (GameProgressManager.Instance != null && 
            GameProgressManager.Instance.IsIntroWatched())
        {
            Destroy(GetComponent<Collider2D>());
        }
    }
}