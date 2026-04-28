using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    public Transform target;
    private Vector3 additionalOffset;

    public void AddOffset(Vector3 offset)
    {
        additionalOffset += offset;
    }

    public void RemoveOffset(Vector3 offset)
    {
        additionalOffset -= offset;
    }

    private Vector3 GetBaseOffset(float x)
    {
        if (SceneManager.GetActiveScene().name == "MainScene")
            return new Vector3(x, 4f, -18.5f);
        if (SceneManager.GetActiveScene().name == "L1")
            return new Vector3(x, 2f, -23f);
        if (SceneManager.GetActiveScene().name == "L1Part2")
            return new Vector3(x, 0.7f, -23f);
        if (SceneManager.GetActiveScene().name == "L1Part3")
            return new Vector3(x, 2.5f, -23f);
        if (SceneManager.GetActiveScene().name == "L1Part4")
            return new Vector3(x, 2.5f, -23f);
        if (SceneManager.GetActiveScene().name == "L1Part5")
            return new Vector3(x, 2.5f, -23f);
        if (SceneManager.GetActiveScene().name == "L2")
            return new Vector3(x, 0.7f, -30f);
        if (SceneManager.GetActiveScene().name == "L3")
            return new Vector3(x, 3.5f, -25f);
        if (SceneManager.GetActiveScene().name == "L3part2")
            return new Vector3(x, 5.5f, -31f);
        if (SceneManager.GetActiveScene().name == "L3.2")
            return new Vector3(x, 5f, -25f);
        if (SceneManager.GetActiveScene().name == "TrainL")
            return new Vector3(x, 0.5f, -25f);
        if (SceneManager.GetActiveScene().name == "DreamRunning")
            return new Vector3(x, 0.5f, -25f);
        return new Vector3(x, 3f, -20f);
    }

    private void FixedUpdate()
    {
        if (target != null)
        {
            float x = target.transform.localScale.x < 0 ? 5f : -5f;
            Vector3 offset = GetBaseOffset(x) + additionalOffset;
            transform.position = Vector3.Lerp(transform.position, target.position + offset, Time.deltaTime * 10f);
        }
    }
}