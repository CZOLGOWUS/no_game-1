using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerCombatSystem : MonoBehaviour
{
    private EnemyCombatBehaviour enemy;
    private PlayerManager playerMenagerSc;

    private Vector2 attackPointWorldPos;

    public LayerMask enemiesLayer;

    public Vector2 attackPoint;
    public float attackRadius;
    public float attackRate; // attacks per second
    private float timeSinceLastAttack; //in seconds
    [SerializeField]  public bool isAbleToAttack { get; private set; }

    public bool wasHit;
    public float timeBeetwenHits;
    public bool isInvincible = false;
    internal float timeSinceLastHit;

    public int hitPoints;

    private void Start()
    {
        playerMenagerSc = GetComponent<PlayerManager>();
        isAbleToAttack = true;
    }

    //this is just a testing function prototype xd
    public void Attack()
    {
        if( !isAbleToAttack )
            return;

        attackPointWorldPos = new Vector2( transform.position.x - attackPoint.x * Mathf.Sign( transform.rotation.eulerAngles.y - 90 ) , transform.position.y + attackPoint.y );
        Collider2D attackHitCollider = Physics2D.OverlapCircle( attackPointWorldPos , attackRadius , enemiesLayer );


        if( attackHitCollider )
        {
            enemy = attackHitCollider.GetComponent<EnemyCombatBehaviour>();

            Vector2 pushBackDir = (enemy.transform.position - transform.position).normalized;


            enemy.TakeHit( 2 );

            enemy.GetComponent<Rigidbody2D>().AddForce( (enemy.transform.position - transform.position).normalized * 1000 , ForceMode2D.Impulse );

        }

        StartCoroutine( AttackCooldown() );

    }

    private void FixedUpdate()
    {

    }

    private IEnumerator AttackCooldown()
    {
        if( isAbleToAttack )
        {
            isAbleToAttack = false;

            yield return new WaitForSecondsRealtime( attackRate );

            isAbleToAttack = true;

        }
    }

    public void TakeHit( GameObject enemy )
    {
        isInvincible = true;
        EnemyCombatBehaviour enemyCombatSC =  enemy.GetComponent<EnemyCombatBehaviour>();

        playerMenagerSc.transform.Translate( (enemy.transform.position - transform.position).normalized * enemyCombatSC.attackDamage );
        StartCoroutine( InvincibleAfterHitToggle(timeBeetwenHits) );
        
    }


    private IEnumerator InvincibleAfterHitToggle( float timeToWait )
    {
        yield return new WaitForSeconds( timeToWait );

        isInvincible = !isInvincible;

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere( attackPointWorldPos , attackRadius );
    }
}
