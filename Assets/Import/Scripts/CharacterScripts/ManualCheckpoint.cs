using UnityEngine;

public static class ManualCheckpoint
{
    public static Vector3 Position { get; private set; }
    public static string SceneName { get; private set; }
    public static bool Has => SceneName != null;
    public static bool Used { get; private set; }

    public static void Set(Vector3 pos, string scene)
    {
        Position = pos;
        SceneName = scene;
        Used = false;
    }

    public static void Consume()
    {
        Position = Vector3.zero;
        SceneName = null;
        Used = true;
    }

    public static void Clear()
    {
        Position = Vector3.zero;
        SceneName = null;
        Used = false;
    }
}
