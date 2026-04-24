using System.Collections;
using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    public static SpawnPointManager Instance;

    public static int LastHealth = 100;
    public static bool forceLookRight = false;

    [HideInInspector] public Vector3 spawnPosition;
    [HideInInspector] public bool setDirection = false;
    [HideInInspector] public bool lookRight = true;
    [HideInInspector] public bool hasSpawnData = false;
    [HideInInspector] public bool lockControls = false;
    [HideInInspector] public float lockDuration = 2f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

}

    public void ApplySpawnPointToSec(SecMainCharacter player)
    {
        if (!hasSpawnData) return;

        player.transform.position = spawnPosition;

        if (setDirection)
        {
            Vector3 s = player.transform.localScale;
            s.x = Mathf.Abs(s.x) * (lookRight ? -1 : 1);
            player.transform.localScale = s;
        }

        hasSpawnData = false;
        spawnPosition = Vector3.zero;
        setDirection = false;
    }

    public IEnumerator ApplyControlLock(SecMainCharacter player)
    {
        if (!lockControls) yield break;

        player.enabled = false;
        yield return new WaitForSeconds(lockDuration);
        player.enabled = true;

        lockControls = false;
        lockDuration = 2f;
    }
}