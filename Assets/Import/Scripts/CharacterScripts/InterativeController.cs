using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class InterativeController : MonoBehaviour
{
    [Header("Настройки")]
    public string objectName = "Объект";
    public Sprite interactIcon;

    [Serializable]
    public class ButtonClickedEvent : UnityEvent { }

    [FormerlySerializedAs("onDO")]
    [SerializeField] private ButtonClickedEvent e_onDO = new ButtonClickedEvent();
    [SerializeField] private ButtonClickedEvent enter_DO = new ButtonClickedEvent();
    [SerializeField] private ButtonClickedEvent exit_DO = new ButtonClickedEvent();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        enter_DO.Invoke();
        collision.GetComponent<PlayerInteraction>()?.ShowInteractIcon(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        exit_DO.Invoke();
        collision.GetComponent<PlayerInteraction>()?.HideInteractIcon();
    }

    public void Do() => e_onDO.Invoke();
}
