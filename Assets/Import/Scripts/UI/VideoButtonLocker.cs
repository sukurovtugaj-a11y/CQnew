using UnityEngine;
using System.Collections.Generic;

public class VideoButtonLocker : MonoBehaviour
{
    public List<string> levelNames = new List<string>();
    public List<MyButton> buttons = new List<MyButton>();
    public Sprite lockedSprite;

    void Start()
    {
        if (buttons.Count == 0 || levelNames.Count == 0) return;

        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] == null) continue;

            string level = i < levelNames.Count ? levelNames[i] : levelNames[0];
            bool completed = GameProgressManager.Instance.IsLevelCompleted(level);

            if (!completed)
            {
                if (lockedSprite != null) buttons[i].SetLockedSprite(lockedSprite);
                buttons[i].Lock();
            }
            else
            {
                buttons[i].Unlock();
            }
        }
    }
}