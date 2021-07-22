using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationBehaviour : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerMovement movementScript;

    public readonly bool isRunning;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        movementScript = GetComponent<PlayerMovement>();
    }

    private void FixedUpdate()
    {

        if( Mathf.Abs( rb.velocity.x ) > 0.1f )
            animator.SetBool( "isRunning" ,true);
        else
            animator.SetBool( "isRunning" , false );


        if( movementScript.IsJumping )
            animator.SetBool( "isJumping" , true );
        else
            animator.SetBool( "isJumping" , false );

    }

}
