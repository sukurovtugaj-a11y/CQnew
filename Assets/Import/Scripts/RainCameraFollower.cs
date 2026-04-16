using UnityEngine;

public class RainCameraFollower : MonoBehaviour
{
    void LateUpdate()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            transform.position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, transform.position.z);
        }
    }
}