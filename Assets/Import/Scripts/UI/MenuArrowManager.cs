using UnityEngine;
using UnityEngine.EventSystems;

public class MenuArrowManager : MonoBehaviour
{
    [System.Serializable]
    public class ButtonArrowPair
    {
        public MyButton button;
        public GameObject arrow;
    }

    public ButtonArrowPair[] pairs;

    private void Start()
    {
        foreach (var pair in pairs)
        {
            if (pair.button == null || pair.arrow == null) continue;

            var trigger = pair.button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = pair.button.gameObject.AddComponent<EventTrigger>();

            var enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => ShowArrow(pair.arrow));
            trigger.triggers.Add(enterEntry);

            var exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => HideArrow(pair.arrow));
            trigger.triggers.Add(exitEntry);
        }

        // Показываем стрелку первой кнопки
        if (pairs.Length > 0 && pairs[0].arrow != null)
            pairs[0].arrow.SetActive(true);
    }

    private void ShowArrow(GameObject arrow)
    {
        foreach (var pair in pairs)
            if (pair.arrow != null) pair.arrow.SetActive(false);

        arrow.SetActive(true);
    }

    private void HideArrow(GameObject arrow) { }
}
