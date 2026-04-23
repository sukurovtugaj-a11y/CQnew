using UnityEngine;

public class CarTrigger : MonoBehaviour
{
    [SerializeField]
    private MonoBehaviour Player;

    // ������ ��� ��������� �������� ������
    [SerializeField]
    private float carSpeed = 3f;

    [SerializeField]
    private GameObject Inside, NullCar;

    [SerializeField]
    private Transform EffectPoint, TextPanel;

    [SerializeField]
    private string[] Messages;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var player = collision.gameObject.GetComponent<SecMainCharacter>();
        
        if (player != null)
        {
            Player = player;
            Player.transform.GetChild(0).gameObject.SetActive(false);
            Player.enabled = false;

            Player.transform.parent = transform;

            NullCar.SetActive(false);
            Inside.SetActive(true);
            TextPanel.gameObject.SetActive(true);

            EffectPoint.gameObject.SetActive(true);

            SetMessage();
        }
        else
        {
            if (collision.gameObject.tag == "Platform")
            {
                Player.transform.GetChild(0).gameObject.SetActive(true);
                Player.enabled = true;

                Player.transform.parent = null;

                NullCar.SetActive(true);
                Inside.SetActive(false);

                Player = null;

                GetComponent<BoxCollider2D>().enabled = false;
                GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

                EffectPoint.gameObject.SetActive(false);
                TextPanel.gameObject.SetActive(false);

                CancelInvoke("SetMessage");

                this.enabled = false;
            }
        }
    }

    private void SetMessage()
    {
        float R = Random.Range(0, 2.5f);
        TextPanel.localPosition = new Vector3(0f, -1.63f, 0f);
        TextPanel.GetChild(0).GetComponent<TextMesh>().text = Messages[(int)R];

        Invoke("SetMessage", 1.5f);
    }

    private void FixedUpdate()
    {
        if (Player != null)
        {
            TextPanel.localPosition = Vector3.Lerp(TextPanel.localPosition, Vector3.zero, Time.deltaTime * 10f);
            TextPanel.GetChild(0).GetComponent<TextMesh>().color = Color.Lerp(TextPanel.GetChild(0).GetComponent<TextMesh>().color, Color.white, Time.deltaTime * 5f);


            float x = 0;
            if (Input.GetAxis("Horizontal") != 0) { x = -1; }

            transform.Translate(x * Time.deltaTime * carSpeed, 0f, 0f);

            for (int i = 0; i < EffectPoint.childCount; i++)
            {
                EffectPoint.GetChild(i).localScale = new Vector3(x, x, -x);
            }
        }
    }
}
