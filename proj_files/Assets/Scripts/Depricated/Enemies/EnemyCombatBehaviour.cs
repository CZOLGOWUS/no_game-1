using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombatBehaviour : MonoBehaviour
{
    [SerializeField]
    private float health;
    public float maxHealth;
    public float attackDamage;

    public void TakeHit(float damage)
    {
        health -= damage;

        if( health <= 0f )
            Die();

    }

    private void Die()
    {
        this.gameObject.SetActive( false );
    }
}
