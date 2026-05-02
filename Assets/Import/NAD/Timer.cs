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

        if (TimeValue <= 0)
        {
            CancelInvoke("Tick");
            KillPlayer();
            return;
        }

        TimeValue--;
        Invoke("Tick", 1f);
    }

    private void KillPlayer()
    {
        var player = FindObjectOfType<SecMainCharacter>();
        if (player != null)
        {
            player.Die();
        }
    }
}
