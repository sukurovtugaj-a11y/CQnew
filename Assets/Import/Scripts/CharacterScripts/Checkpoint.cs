using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Visuals")]
    public Sprite activeSprite;
    private SpriteRenderer spriteRenderer;
    private bool isActive = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isActive)
        {
            //MainCharacter.Instance.SetRespawnPoint(transform.position);
            Activate();
        }
    }

    public void Activate()
    {
        isActive = true;
        if (activeSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = activeSprite;
    }
}