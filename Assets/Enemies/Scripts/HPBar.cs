using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    public HP hp;
    public Slider slider;

    void Start()
    {
        slider.maxValue = hp.maxHP;
        slider.value = hp.maxHP;

        hp.onChangeHp.AddListener(UpdateSlider);
    }

    public void UpdateSlider(float value)
	{
        slider.value = value;
    }

	private void OnDestroy()
	{
        if (hp != null)
            hp.onChangeHp.RemoveListener(UpdateSlider);
	}
}
