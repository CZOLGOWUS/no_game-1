using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace noGame.Character.MonoBehaviours
{
    [RequireComponent( typeof( Rigidbody2D ) )]
    [RequireComponent( typeof( Collider2D ) )]
    public class CharacterController2D : MonoBehaviour
    {
        //dependecie references
        private Rigidbody2D thisRigidBody;
        private Collider2D thisCollider;

        [Header( "Ground Check Options" )]
        public Vector2 sizeOfRayBox;
        public float distanceOfBoxCast;
        public LayerMask TerrainLayerMask;

        //movement
        private float walkInput; // <-1;1>
        private bool hasPressedJump;

        private Vector2 currentPosition;
        private Vector2 prevPosition;
        private bool isGrounded;

        public bool IsGrounded { get => isGrounded; }
        public Rigidbody2D ThisRigidBody { get => thisRigidBody; }
        public Collider2D ThisCollider { get => thisCollider; }

        private void Start()
        {
            thisRigidBody = GetComponent<Rigidbody2D>();
            thisCollider = GetComponent<Collider2D>();
        }

        private void FixedUpdate()
        {
            
        }

        private bool CheckIfGrounded()
        {
            return
                Physics2D.BoxCast( transform.position , sizeOfRayBox , 0.0f , Vector2.down , distanceOfBoxCast , TerrainLayerMask ) &&
                currentPosition.y - prevPosition.y <= 0.01f;
        }

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
