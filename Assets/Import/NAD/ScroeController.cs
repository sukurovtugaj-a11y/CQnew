using Unity.VisualScripting;
using UnityEngine;

public class ScroeController : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem PS;
    [SerializeField]
    private SpriteRenderer ImageSpritePoint;
    [SerializeField]
    private int Value;

    [Header("Floating Animation")]
    [SerializeField] private float floatSpeed = 1f;      
    [SerializeField] private float floatAmplitude = 0.5f; 

    private Vector3 startPos; 

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<SecMainCharacter>() != null)
        {
            ImageSpritePoint.enabled = false;
            PS.Play();
            transform.GetComponent<BoxCollider2D>().enabled = false;
            FindObjectOfType<SceneController>().ChangeValue(Value);
            Destroy(gameObject, 1f);
        }
    }
}
