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

        // Удаляем старый маркер если есть
        ManualCheckpoint.ConsumeMarker();

        // Спавним новый маркер (на ~1.2м ниже позиции игрока)
        if (owner.checkpointMarkerPrefab != null)
        {
            var markerPos = owner.transform.position + new Vector3(0, -1.2f, 0);
            var marker = Object.Instantiate(owner.checkpointMarkerPrefab, markerPos, Quaternion.identity);
            ManualCheckpoint.SetMarker(marker);
        }

        ManualCheckpoint.Set(owner.transform.position, SceneManager.GetActiveScene().name);
        owner.sound?.PlayCheckpointSound();
        Debug.Log("[SecMainCharacter] Чекпоинт установлен");
    }

    private bool IsGrounded() => owner.rb.GetContacts(owner.groundFilter, owner.groundContacts) > 0;
}
