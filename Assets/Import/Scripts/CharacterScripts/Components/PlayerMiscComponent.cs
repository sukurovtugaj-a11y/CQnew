using UnityEngine;

public class PlayerMiscComponent
{
    private readonly SecMainCharacter owner;

    public PlayerMiscComponent(SecMainCharacter owner)
    {
        this.owner = owner;
    }

    public void ToggleMenu()
    {
        bool menuActive = !owner.MenuPanel.activeSelf;
        owner.MenuPanel.SetActive(menuActive);
        Cursor.visible = menuActive;
        Time.timeScale = menuActive ? 0f : 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    public void CreatePlayerCamera()
    {
        GameObject cameraObject = new GameObject("PlayerCamera");
        cameraObject.AddComponent<Camera>();
        owner.cameraController = cameraObject.AddComponent<CameraController>();
        owner.cameraController.target = owner.transform;
        cameraObject.transform.position = owner.transform.position + new Vector3(0, 1, -10);
    }

    public void OnAINotifyEnter()
    {
        owner.inAIZone = true;
    }

    public void OnAINotifyExit()
    {
        owner.inAIZone = false;
    }

    public void UpdateAITriggerCheck(Collider2D other)
    {
        if (other.isTrigger &&
            (other.GetComponent<AIcontroller>() != null ||
             other.GetComponentInParent<AIcontroller>() != null ||
             other.GetComponentInChildren<AIcontroller>() != null))
        {
            if (owner.gameObject.activeInHierarchy)
            {
                // Determined by caller (OnTriggerEnter2D/Exit2D)
            }
        }
    }
}
