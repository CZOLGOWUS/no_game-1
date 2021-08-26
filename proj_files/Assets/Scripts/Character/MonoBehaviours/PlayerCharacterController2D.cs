using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent( typeof( CharacterController2D ) )]
public class PlayerCharacterController2D : MonoBehaviour
{
    private CharacterController2D thisCharacterController;


    [SerializeField] private float gravity = -10f;
    [SerializeField] private float jumpHeight = 4f;
    [SerializeField] private float timeToJumpApex = 0.3f;
    [SerializeField] private float moveSpeed = 10f;
    [Range( 0f , 0.5f )]
    [SerializeField] private float accelerationTimeAirborn = .2f;
    [Range( 0f , 0.5f )]
    [SerializeField] private float accelerationTimeGrounded = .2f;

    private float jumpVelocity = 10f;

    private float velocityXSmooth;

    private float movementInput;
    private bool jumpInput; //button

    private Vector3 newVelocity;
    private Vector3 oldVelocity;

    private void Awake()
    {
        thisCharacterController = GetComponent<CharacterController2D>();
    }

    private void Start()
    {

        gravity = -(2 * jumpHeight) / (timeToJumpApex * timeToJumpApex);
        jumpVelocity = Mathf.Abs( gravity * timeToJumpApex );

        print( "Gravity: " + gravity + "jump vel:" + jumpVelocity );

    }


    private void FixedUpdate()
    {
        if( thisCharacterController.collisions.top || thisCharacterController.collisions.bottom )
        {
            newVelocity.y = 0f;
        }

        if( jumpInput && thisCharacterController.collisions.bottom )
        {
            newVelocity.y = jumpVelocity;
        }

        float targetVelocityX = movementInput * moveSpeed;

        newVelocity.x = Mathf.SmoothDamp( newVelocity.x , targetVelocityX , ref velocityXSmooth , (thisCharacterController.collisions.bottom) ? accelerationTimeGrounded : accelerationTimeAirborn );
        newVelocity.y += gravity * Time.fixedDeltaTime;
        thisCharacterController.Move( newVelocity * Time.fixedDeltaTime );

    }


    public void OnMovement( InputAction.CallbackContext ctx )
    {
        movementInput = ctx.ReadValue<float>();
    }

    public void OnJump( InputAction.CallbackContext ctx )
    {
        if( ctx.performed || ctx.started )
        {
            jumpInput = true;
        }
        else if( ctx.canceled )
        {
            jumpInput = false;
        }
        print( jumpInput );
    }

}
