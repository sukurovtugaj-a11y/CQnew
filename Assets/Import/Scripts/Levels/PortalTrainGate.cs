using UnityEngine;

public class PortalTrainGate : MonoBehaviour
{
    public GameObject closedPortal;
    public GameObject openPortal;

    void Start()
    {
        bool trainCompleted = GameProgressManager.Instance.IsLevelCompleted("TrainL");

        if (closedPortal != null) closedPortal.SetActive(!trainCompleted);
        if (openPortal != null) openPortal.SetActive(trainCompleted);
    }
}