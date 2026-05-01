using UnityEngine;

public static class ManualCheckpoint
{
    public static Vector3 Position { get; private set; }
    public static string SceneName { get; private set; }
    public static bool Has => SceneName != null;
    public static bool Used { get; private set; }
    public static GameObject Marker { get; private set; }

    public static void Set(Vector3 pos, string scene)
    {
        Position = pos;
        SceneName = scene;
        Used = false;
    }

    public static void SetMarker(GameObject marker)
    {
        Marker = marker;
    }

    public static void ConsumeMarker()
    {
        if (Marker != null)
        {
            Object.Destroy(Marker);
            Marker = null;
        }
    }

    public static void Consume()
    {
        ConsumeMarker();
        Position = Vector3.zero;
        SceneName = null;
        Used = true;
    }

    public static void Clear()
    {
        ConsumeMarker();
        Position = Vector3.zero;
        SceneName = null;
        Used = false;
    }
}
