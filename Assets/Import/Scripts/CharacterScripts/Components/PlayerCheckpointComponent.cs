using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCheckpointComponent
{
    private readonly SecMainCharacter owner;

    public PlayerCheckpointComponent(SecMainCharacter owner)
    {
        this.owner = owner;
    }

    public void TrySetCheckpoint()
    {
        if (!owner.checkpointEnabled) return;
        if (owner.MenuPanel.activeSelf || Time.timeScale <= 0 || ManualCheckpoint.Has || ManualCheckpoint.Used) return;
        if (SceneManager.GetActiveScene().name == "MainScene") return;
        if (!IsGrounded()) return;
        if (owner.inAIZone) { Debug.Log("[SecMainCharacter] Нельзя: внутри зоны AI"); return; }

        ManualCheckpoint.Set(owner.transform.position, SceneManager.GetActiveScene().name);
        Debug.Log("[SecMainCharacter] Чекпоинт установлен");
    }

    private bool IsGrounded() => owner.rb.GetContacts(owner.groundFilter, owner.groundContacts) > 0;
}
