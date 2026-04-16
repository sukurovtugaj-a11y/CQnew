using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    public Transform target;
    
    private void FixedUpdate()
    {
        if (target != null)
        {
            float x = 0;
            if (target.transform.localScale.x < 0)
            {
                x = 5;
            }
            else
            {
                x = -5;
            }
            // === СТАРОЕ (для Player с SpriteRenderer на root) ===
            //SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
            //if (sr == null) sr = target.GetComponentInChildren<SpriteRenderer>();
            //if (sr != null && sr.flipX)
            //{
            //    x = 5;
            //}
            //else
            //{
            //    x = -5;
            //}
            //transform.position = Vector3.Lerp(transform.position, target.position + new Vector3(x, 4f, -25), Time.deltaTime * 10f);
            Vector3 offset;
            if (SceneManager.GetActiveScene().name == "MainScene")
            {
                offset = new Vector3(x, 2.5f, -18.5f); // Для Hub
            }
            else if (SceneManager.GetActiveScene().name == "L1Part2")
            {
                offset = new Vector3(x, 0.7f, -20f); 
            }
            else if (SceneManager.GetActiveScene().name == "L1Part3")
            {
                offset = new Vector3(x, 2.5f, -20f);
            }
            else if (SceneManager.GetActiveScene().name == "L1Part4")
            {
                offset = new Vector3(x, 2.5f, -20f);
            }
            else if (SceneManager.GetActiveScene().name == "L1Part5")
            {
                offset = new Vector3(x, 2.5f, -20f);
            }
            else if (SceneManager.GetActiveScene().name == "L2")
            {
                offset = new Vector3(x, 0.7f, -30f);
            }
            else if (SceneManager.GetActiveScene().name == "L3")
            {
                offset = new Vector3(x, 0.5f, -25f);
            }
            else if (SceneManager.GetActiveScene().name == "L3part2")
            {
                offset = new Vector3(x, 0.5f, -25f);
            } 
            else if (SceneManager.GetActiveScene().name == "L3.2")
            {
                offset = new Vector3(x, 0.5f, -25f);
            }
            else if (SceneManager.GetActiveScene().name == "TrainL")
            {
                offset = new Vector3(x, 0.5f, -25f); 
            }
            else if (SceneManager.GetActiveScene().name == "DreamRunning")
            {
                offset = new Vector3(x, 0.5f, -25f);
            }
            else
            {
                offset = new Vector3(x, 3f, -20f); // Для других уровней
            }

            transform.position = Vector3.Lerp(transform.position, target.position + offset, Time.deltaTime * 10f);
        }
    }
}