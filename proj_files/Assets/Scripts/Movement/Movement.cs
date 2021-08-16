using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using noGame.Character.MonoBehaviours;
using System;

namespace noGame.MovementBehaviour
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Movement : MonoBehaviour
    {

        //dependecies
        private Rigidbody2D thisRigidBody;
        private Collider2D thisCollider;
        

        private Vector2 currentPosition;
        private Vector2 nextPosition;
        private Vector2 velocity = Vector2.zero;

        [Header ("Movement Settings")]
        [Tooltip ("Maximal speed at which the character can move sideways")]
        public float maxSpeed;

        [Tooltip("Height of jump")]
        public float jumpHeight;

        [Tooltip("Rate at which the character stop siedeways movement")]
        [Range(0f,1f)] public float stoppingTimeScale;

        [Tooltip("Force of gravity")]
        public Vector2 gravity;

        [Header( "Ground Detection" )]
        public Vector2 sizeOfRayBox;
        public Vector2 distanceOfBoxCast;
        public LayerMask TerrainLayerMask;

        private float currentSpeed;
        private bool isMoving = false;
        private bool isJumping = false;
        private bool isGrounded = false;
        
        public float CurrentSpeed { get => currentSpeed; }
        public Vector2 PrevPosition { get => currentPosition; }
        public Vector2 NextPosition { get => nextPosition; }
        public bool IsMoving { get => isMoving; set => isMoving = value; }
        public bool IsJumping { get => isJumping; set => isJumping = value; }
        

        private void Start()
        {
            thisRigidBody = GetComponent<Rigidbody2D>();
            thisCollider = GetComponent<Collider2D>();

            currentPosition = transform.position;
            nextPosition = currentPosition;
        }

        private void Update()
        {

            nextPosition += 0.5f * gravity * Time.deltaTime;

            Move();

            velocity = nextPosition - currentPosition;
            currentPosition = nextPosition;
        }

        private void FixedUpdate()
        {
            isGrounded = IsGrounded();
        }

        private void Move()
        {
            thisRigidBody.position = ( nextPosition );
        }

        public void MoveInDirection(float direction) // (left, right)
        {
            if( direction != 0 )
            {
                isMoving = true;
                nextPosition.x += direction * maxSpeed;
            }
            else
            {
                isMoving = false;
                //nextPosition.x -= currentPosition.x; //* (1f - stoppingTimeScale);
            }
        }

        public void HandleRotation(float direction)
        {
            if( isMoving )
            {

                transform.LookAt( transform.forward * direction );

            }
        }

        public bool IsGrounded()    
        {
            Vector2 boxCenter = (Vector2)transform.position - distanceOfBoxCast;

            return Physics2D.OverlapBox(boxCenter,sizeOfRayBox,0f,TerrainLayerMask);


        }

        public void Jump()
        {
            //if(isGrounded)
            //{
            //    nextPositionOffset.y += height;
            //    float jumpInitialVelocity = Mathf.Sqrt(2 * gravity.y * jumpHeight);
            //    nextPosition.y = jumpInitialVelocity;
            //}
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube( new Vector2(transform.position.x,transform.position.y) - distanceOfBoxCast , sizeOfRayBox );
        }

    }
}
