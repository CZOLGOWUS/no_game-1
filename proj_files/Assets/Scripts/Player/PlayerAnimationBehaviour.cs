using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationBehaviour : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerMovement movementScript;

    private int animIsJumping;
    private int animIsRunning;
    private int animVelocity;
    private int animAttackTrigger;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        movementScript = GetComponent<PlayerMovement>();

        animIsJumping = Animator.StringToHash("isJumping");
        animIsRunning = Animator.StringToHash( "isRunning" );
        animVelocity = Animator.StringToHash("Velocity");
        animAttackTrigger = Animator.StringToHash( "AttackTrigger" );

    }

    private void FixedUpdate()
    {

        animator.SetBool( animIsRunning , Mathf.Abs( rb.velocity.x ) > 0.1f );
        animator.SetBool( animIsJumping , movementScript.IsJumping );


        animator.SetFloat( animVelocity , Mathf.Abs( rb.velocity.x / movementScript.maxSpeed ));
    }

    public IEnumerator AttackPlay()
    {
        animator.SetLayerWeight(animator.GetLayerIndex("Attacking"), 1f );
        animator.SetTrigger( animAttackTrigger );

        //i know , i know...
        yield return new WaitForSecondsRealtime( 0.9f );

        animator.SetLayerWeight( animator.GetLayerIndex( "Attacking" ) , 0f);
    }

}
