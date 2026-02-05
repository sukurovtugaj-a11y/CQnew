using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Serialization;
using System;

public class MyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Color active = new Color(0.8485116f, 0.9716981f, 0.8195265f, 1f);
    public Color passive = new Color(0.75f, 0.95f, 0.70f, 1f);
    private Color current;

    public float hoverSize = 1.03f;
    public float sizeSpeed = 8f;
    private Vector3 normalSize;
    private Vector3 targetSize;

    private Image MineUI;

    [Serializable]
    public class ButtonClickedEvent : UnityEvent { }
    [FormerlySerializedAs("onClick")]
    [SerializeField]
    private ButtonClickedEvent m_OnClick = new ButtonClickedEvent();

    private void OnEnable()
    {
        MineUI = GetComponent<Image>();
        MineUI.color = passive;
        current = passive;

        normalSize = transform.localScale;
        targetSize = normalSize;
    }

    private void OnDisable()
    {
        transform.localScale = normalSize;
        MineUI.color = passive;
        current = passive;
        targetSize = normalSize;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        targetSize = normalSize * hoverSize;
        current = active;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        targetSize = normalSize;
        current = passive;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        m_OnClick.Invoke();
    }

    private void Update()
    {
        // Используем Time.unscaledDeltaTime вместо Time.deltaTime
        MineUI.color = Color.Lerp(MineUI.color, current, Time.unscaledDeltaTime * 10f);
        transform.localScale = Vector3.Lerp(transform.localScale, targetSize, Time.unscaledDeltaTime * sizeSpeed);
    }
}
