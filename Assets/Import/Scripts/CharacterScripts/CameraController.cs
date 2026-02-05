using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    private void FixedUpdate()
    {
        if (target != null)
        {
            float x = 0;
            if (target.GetComponent<SpriteRenderer>())
            {
                if (target.GetComponent<SpriteRenderer>().flipX)
                {
                    x = 5;
                }
                else
                {
                    x = -5;
                }
            }
            transform.position = Vector3.Lerp(transform.position, target.position + new Vector3(x, 1.5f, -13), Time.deltaTime * 10f);
        }
    }
}
