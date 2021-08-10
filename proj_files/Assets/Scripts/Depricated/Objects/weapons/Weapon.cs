using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    private float damage;
    private float range;


    private void Awake()
    {
        
    }

    private void Attack(GameObject target)
    {

    }

    private GameObject HitCheck()
    {
        return new GameObject();
    }

}
