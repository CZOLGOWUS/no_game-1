using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace noGame.Character.MonoBehaviours
{

    [RequireComponent( typeof( CharacterController2D ) )]
    [RequireComponent( typeof( PlayerInput ) )]
    public class PlayerCharacterController : MonoBehaviour
    {
        private CharacterController2D characterController;

        //movement variables
        private float walkInput; // <-1;1>
        private bool hasPressedJump;
        private bool isJumpPressed = false;
        private bool isMovementPressed = false;


        public float WalkInput { get => walkInput; }
        public bool HasPressedJump { get => hasPressedJump; }
        public CharacterController2D Movement { get => characterController; set => characterController = value; }

        private void Start()
        {
            //thisRigidBody = GetComponent<Rigidbody2D>();
            //thisCollider = GetComponent<Collider2D>();
            characterController = GetComponent<CharacterController2D>();
        }

        private void Update()
        {
            characterController.MoveInDirection(WalkInput);
        }

        public void OnWalk( InputAction.CallbackContext ctx )
        {
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
                characterController.Jump();
            }

            if(ctx.canceled)
            {
                isJumpPressed = false;
            }
        }

    }
}
