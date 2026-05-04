using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HP : MonoBehaviour
{
    public float maxHP;
    public UnityEvent<GameObject> onHit;
    public UnityEvent<float> onChangeHp;
    public UnityEvent<float> onTakeDamage;
    public UnityEvent onDie;

    private float currentHP;
	private bool alive = true;

	private void Start()
	{
		currentHP = maxHP;	
	}

	public void TakeDamage(float damage, GameObject damageSource)
	{
		if (alive == false)
			return;

		onHit.Invoke(damageSource);
		currentHP -= damage;
		if (currentHP <= 0)
			currentHP = 0;

		onChangeHp.Invoke(currentHP);
		onTakeDamage.Invoke(damage);

		if(currentHP <= 0 && alive)
		{
			onDie.Invoke();
			alive = false;
		}
	}

	public void SeflfDestroy()
	{
		Destroy(this.gameObject);
	}
}
