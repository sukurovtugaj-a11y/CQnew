using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealthComponent
{
    private readonly SecMainCharacter owner;
    private readonly Rigidbody2D rb;

    public PlayerHealthComponent(SecMainCharacter owner, Rigidbody2D rb)
    {
        this.owner = owner;
        this.rb = rb;
    }

    public void Damage(float dam, Transform respawnPoint = null)
    {
        if (owner.isInvulnerable) return;
        owner.currentHealth -= (int)dam;
        if (owner.currentHealth <= 0) Die();
        else if (respawnPoint != null) { owner.transform.position = respawnPoint.position; rb.velocity = Vector2.zero; }
    }

    public void ResetAfterRespawn()
    {
        rb.isKinematic = false;
        owner.currentHealth = owner.maxHealth;
    }

    public bool IsInvulnerable() => owner.isInvulnerable;

    private void Die()
    {
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        SpawnPointManager.LastHealth = owner.currentHealth;

        if (TryCheckpointRespawn()) return;

        if (owner.playerMenuScript != null)
            owner.playerMenuScript.ShowDeathPanel();
        else
            Debug.LogError("[SecMainCharacter] playerMenuScript не назначен!");
    }

    private bool TryCheckpointRespawn()
    {
        if (!ManualCheckpoint.Has) return false;

        owner.currentHealth = 50;

        if (SceneManager.GetActiveScene().name == ManualCheckpoint.SceneName)
            owner.transform.position = ManualCheckpoint.Position;
        else
        {
            var sp = SpawnPointManager.Instance;
            owner.transform.position = sp != null ? sp.spawnPosition : Vector3.zero;
        }

        ManualCheckpoint.Consume();
        rb.isKinematic = false;
        Debug.Log("[SecMainCharacter] Респаун по чекпоинту: 50 HP");
        return true;
    }
}
