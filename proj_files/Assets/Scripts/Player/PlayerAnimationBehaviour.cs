using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationBehaviour : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerMovement movementScript;

    public int animIsJumping;
    private int animIsRunning;
    private int animVelocity;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        movementScript = GetComponent<PlayerMovement>();

        animIsJumping = Animator.StringToHash("isJumping");
        animIsRunning = Animator.StringToHash( "isRunning" );
        animVelocity = Animator.StringToHash("Velocity");

    }

    private void FixedUpdate()
    {

        if( Mathf.Abs( rb.velocity.x ) > 0.1f )
            animator.SetBool( animIsRunning , true);
        else
            animator.SetBool( animIsRunning , false );


        if( movementScript.IsJumping )
            animator.SetBool( animIsJumping , true );
        else
            animator.SetBool( animIsJumping , false );


        animator.SetFloat( animVelocity , Mathf.Abs( rb.velocity.x / movementScript.maxSpeed ));
    }

}
