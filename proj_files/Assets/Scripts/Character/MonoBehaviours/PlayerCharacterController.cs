using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using noGame.MovementBehaviour;

namespace noGame.Character.MonoBehaviours
{
    [RequireComponent( typeof( CharacterController ) )]
    [RequireComponent( typeof( Movement ) )]
    [RequireComponent( typeof( PlayerInput ) )]
    public class PlayerCharacterController : MonoBehaviour
    {
        private CharacterController characterController;
        private Movement movement;

        //movement variables
        private float walkInput; // <-1;1>
        private bool hasPressedJump;
        private bool isJumpingPressed = false;
        private bool isMovementPressed = false;

        public CharacterController CharacterController { get => characterController; }
        public float WalkInput { get => walkInput; }
        public bool HasPressedJump { get => hasPressedJump; }
        public Movement Movement { get => movement; set => movement = value; }

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            movement = GetComponent<Movement>();
        }

        private void FixedUpdate()
        {
            movement.Move(WalkInput);
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
                isJumpingPressed = true;
            }

            if(ctx.canceled)
            {
                isJumpingPressed = false;
            }
        }

    }
}
