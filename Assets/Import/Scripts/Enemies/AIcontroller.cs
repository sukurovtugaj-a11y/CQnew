using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class AIcontroller : MonoBehaviour
{
    public TypeAI typeAi = TypeAI.Other;
    [System.Serializable]
    public class ButtonClickedEvent : UnityEvent { }
    [FormerlySerializedAs("OnHit")]
    [SerializeField]
    private ButtonClickedEvent OnHit = new ButtonClickedEvent();
    [FormerlySerializedAs("OnShoot")]
    [SerializeField]    
    private ButtonClickedEvent OnShoot = new ButtonClickedEvent();
    [FormerlySerializedAs("OnCathUp")]
    [SerializeField]
    private ButtonClickedEvent CathUp = new ButtonClickedEvent();
    [FormerlySerializedAs("OnDeth")]
    [SerializeField]
    private ButtonClickedEvent OnDeth = new ButtonClickedEvent();
    [FormerlySerializedAs("OnIdle")]
    [SerializeField]
    private ButtonClickedEvent OnIdle = new ButtonClickedEvent();

    public MonoBehaviour Target;
    public float Speed = 1.0f;

    public WeaponItem CurrentItemInHand;

    [Header("Weapon Settings")]
    public List<WeaponItem> weaponList = new List<WeaponItem>();
    public int CurrentWeaponItem = -1;

    [Header("Shooting Settings")]
    public float fireInterval = 1.5f;
    public Transform shootPoint;
    private float nextFireTime = 0f;

    private void Update()
    {
        if (Target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, Target.transform.position, Time.deltaTime * 10f * Speed);
        }

        CurrentItemInHand = Converter.GetItemForIndex(weaponList.ToArray(), CurrentWeaponItem, weaponList[0]);

        //if (Target != null && CurrentWeaponItem >= 0 && Time.time >= nextFireTime)
        //{
        //    SetShoot(null);
        //    nextFireTime = Time.time + fireInterval;
        //}


    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.GetComponent<SecMainCharacter>();
        if (player != null)
        {
            Target = player;
            OnShoot.Invoke();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        var player = collision.GetComponent<SecMainCharacter>();
        if (player != null)
        {
            Target = null;
            OnIdle.Invoke();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<SecMainCharacter>() != null)
        {
            OnHit.Invoke();
        }
    }

    public void SwitchWeapon(int index)
    {
        if (index >= 0 && index < weaponList.Count)
        {
            CurrentWeaponItem = index;
            Debug.Log($"[{name}] ������ ������� ��: {weaponList[index].NameWeapon}");
        }
        else
        {
            Debug.LogWarning($"[{name}] �������� ������ ������: {index}");
        }
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.GetComponent<MainCharacter>())
    //    {
    //        OnHit.Invoke();
    //    }
    //}

    public void SetShoot(GameObject bullet)
    {
        
        if (bullet != null)
        {
            Vector3 spawnPos = shootPoint != null ? shootPoint.position : transform.position;
            Quaternion spawnRot = shootPoint != null ? shootPoint.rotation : transform.rotation;
            Instantiate(bullet, spawnPos, spawnRot);
        }

        OnShoot.Invoke();
    }
}
