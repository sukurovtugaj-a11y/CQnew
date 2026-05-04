using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActionProvider : MonoBehaviour
{
	public UnityEvent[] actions;

	public void DoAction(int index)
	{
		actions[index].Invoke();
	}
}
