using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using noGame.MovementBehaviour;

namespace noGame.Character.MonoBehaviours
{
    //[RequireComponent( typeof( Rigidbody2D ) )]
    //[RequireComponent( typeof( Collider2D ) )]
    [RequireComponent( typeof( Movement ) )]
    [RequireComponent( typeof( PlayerInput ) )]
    public class PlayerCharacterController : MonoBehaviour
    {
        //private Rigidbody2D thisRigidBody;
        private Collider2D thisCollider;
        private Movement movement;

        //movement variables
        private float walkInput; // <-1;1>
        private bool hasPressedJump;
        private bool isJumpPressed = false;
        private bool isMovementPressed = false;


        public float WalkInput { get => walkInput; }
        public bool HasPressedJump { get => hasPressedJump; }
        public Movement Movement { get => movement; set => movement = value; }

        private void Start()
        {
            //thisRigidBody = GetComponent<Rigidbody2D>();
            //thisCollider = GetComponent<Collider2D>();
            movement = GetComponent<Movement>();
        }

        private void Update()
        {
            movement.MoveInDirection(WalkInput);
        }

        public void OnWalk( InputAction.CallbackContext ctx )
        {
            print( ctx.ReadValue<float>() );
            walkInput = ctx.ReadValue<float>();

            if(ctx.started)
            {
                isMovementPressed = true;
            }

            if(ctx.canceled)
            {
                isMovementPressed = false;
            }
        }

        public void OnJump( InputAction.CallbackContext ctx )
        {
            if( ctx.performed )
            {
                hasPressedJump = true;
                isJumpPressed = true;
                movement.Jump();
            }

            if(ctx.canceled)
            {
                isJumpPressed = false;
            }
        }

    }
}
