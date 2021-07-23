using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerCombatSystem : MonoBehaviour
{
    private EnemyCombatBehaviour enemy;
    private Vector2 attackPointWorldPos;

    public LayerMask enemiesLayer;

    public Vector2 attackPoint;
    public float attackRadius;


    //this is just a testing function prototype xd
    public void Attack()
    {
        attackPointWorldPos = new Vector2( transform.position.x - attackPoint.x * Mathf.Sign( transform.rotation.eulerAngles.y - 90 ) , transform.position.y + attackPoint.y );
        Collider2D attackHitCollider = Physics2D.OverlapCircle( attackPointWorldPos , attackRadius  , enemiesLayer );


        if( attackHitCollider )
        {
            enemy = attackHitCollider.GetComponent<EnemyCombatBehaviour>();

            Vector2 pushBackDir = (enemy.transform.position - transform.position).normalized;

            
            enemy.TakeHit(2);

            enemy.GetComponent<Rigidbody2D>().AddForce((enemy.transform.position - transform.position).normalized * 1000, ForceMode2D.Impulse );

        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackPointWorldPos , attackRadius );
    }
}
