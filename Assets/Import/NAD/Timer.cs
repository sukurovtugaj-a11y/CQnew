using UnityEngine;

public class Timer : MonoBehaviour
{
    public int TimeValue = 400;

    private void Start()
    {
        Tick();
    }

    private void Tick()
    {
        transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = TimeValue.ToString("000");
        if (TimeValue > -1) { TimeValue--; Invoke("Tick", 1f);}
        else { CancelInvoke("Tick"); }
    }
}
