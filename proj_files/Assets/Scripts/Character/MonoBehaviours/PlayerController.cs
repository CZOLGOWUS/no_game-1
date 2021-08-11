using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace noGame.Character.MonoBehaviours
{
    [RequireComponent( typeof( CharacterController ) )]
    [RequireComponent( typeof( PlayerInput ) )]
    public class PlayerController : MonoBehaviour
    {

        private float walkInput; // <-1;1>
        private bool hasPressedJump;




        public void OnWalk( InputAction.CallbackContext ctx )
        {
            walkInput = ctx.ReadValue<float>();
        }

        public void OnJump( InputAction.CallbackContext ctx )
        {
            if( ctx.performed )
            {
                hasPressedJump = true;
            }
        }

    }
}
