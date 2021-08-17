using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using noGame.Character.MonoBehaviours;
using System;

namespace noGame.MovementBehaviour
{
    //[RequireComponent(typeof(Rigidbody2D))]
    public class Movement : MonoBehaviour
    {

        //dependecies
        private Rigidbody2D thisRigidBody;
        private Collider2D thisCollider;


        private Vector2 currentPosition;
        private Vector2 nextPosition;
        private Vector2 velocity;

        [Header( "Character Settings" )]
        [SerializeField] private float heightOfPlayer = 2f;

        [Header( "Movement Settings" )]
        [Tooltip( "Maximal speed at which the character can move sideways" )]
        [SerializeField] private float maxSpeed;

        [SerializeField] private float jumpHeight;

        [Tooltip( "Rate at which the character stop siedeways movement" )]
        [Range( 0f , 1f )] public float stoppingTimeScale;

        [Tooltip( "Force of gravity" )]
        [SerializeField] private Vector2 gravity;

        [Header( "Ground Detection" )]
        //ray
        [Tooltip( "Ray for better ground detection and slope handaling" )]
        [SerializeField] private Vector2 originOfGroundRayCast;
        [SerializeField] private float distanceOfRayCast;

        [Space]
        //circle sphere overlap
        [Tooltip("The actual GroundCheck at the bottom of player")]
        [SerializeField] private Vector2 groundCheckPointCircle;
        [SerializeField] private float groundCheckPointSize;

        [Space]

        [Tooltip("the smootihing of the Ground snapping")]
        [SerializeField] private bool smoothGroundedTransition;
        [SerializeField] private float smoothGroundedTransitionTime;

        [SerializeField] private LayerMask terrainLayerMask;


        //unorginized
        private RaycastHit2D groundHit;
        private Vector2 currentGravity;
        private float currentSpeed;
        private bool isMoving = false;
        private bool isJumping = false;
        private bool isGrounded = false;


        public float CurrentSpeed { get => currentSpeed; }
        public Vector2 PrevPosition { get => currentPosition; }
        public Vector2 NextPosition { get => nextPosition; }
        public bool IsMoving { get => isMoving; }
        public bool IsJumping { get => isJumping; }


        private void Start()
        {
            //thisRigidBody = GetComponent<Rigidbody2D>();
            //thisCollider = GetComponent<Collider2D>();


            print( nextPosition );
        }

        //order of function calls is important!
        /// <summary>
        /// 1. apply forces taht are always aplied
        /// 2. apply players movement
        /// 3. invoke functions that are meant to keep the player where he is suposed to be
        ///     like:
        ///         - snapping to the ground
        ///         - collisons 
        ///         - etc.
        /// </summary>
        private void Update()
        {
            HandleGravity();


            Move();


            IsGrounded();


        }

        private void LateUpdate()
        {

            //currentPosition = nextPosition;
        }

        private void HandleGravity()
        {
            if( !isGrounded )
            {
                currentGravity = gravity;
            }
            else
            {
                currentGravity = Vector2.zero;
            }
        }

        private void FixedUpdate()
        {
            print( isGrounded );
        }

        private void Move()
        {
            Vector3 temp = Vector3.zero;
            temp.x = velocity.x + currentGravity.x;
            temp.y = velocity.y + currentGravity.y;

            transform.position += transform.TransformDirection( temp ) * Time.deltaTime;

            velocity = Vector2.zero;
        }

        public void MoveInDirection( float direction ) // (left, right)
        {
            if( direction != 0 )
            {
                isMoving = true;
                velocity.x += direction * maxSpeed;
            }
            else
            {
                isMoving = false;
            }
        }

        public void HandleRotation( float direction )
        {
            if( isMoving )
            {
                transform.LookAt( transform.forward * direction );
            }
        }


        public void IsGrounded()
        {
            Vector2 RayCastOrigin = new Vector2(
                transform.position.x + originOfGroundRayCast.x ,
                transform.position.y + originOfGroundRayCast.y
                );


            groundHit = Physics2D.Raycast(
                RayCastOrigin ,
                Vector2.down ,
                distanceOfRayCast ,
                terrainLayerMask
                );


            if( groundHit )
            {

                GroundedConfirm( groundHit );

            }
            else
            {
                isGrounded = false;
            }


        }

        //confirm if the cast and overlapingGroundCheck are the same (for slopes)
        private void GroundedConfirm( RaycastHit2D hit )
        {
            Vector2 groundCheckPosition = new Vector2(
                transform.position.x + groundCheckPointCircle.x ,
                transform.position.y + groundCheckPointCircle.y
                );

            Collider2D[] colls = new Collider2D[3];

            int numberOfCollisions = Physics2D.OverlapCircleNonAlloc(
                groundCheckPosition ,
                groundCheckPointSize ,
                colls ,
                terrainLayerMask
                );

            isGrounded = false;

            for( int i = 0 ; i < numberOfCollisions ; i++ )
            {
                if( colls[i].transform == hit.transform )
                {
                    groundHit = hit;
                    isGrounded = true;

                    if( smoothGroundedTransition )
                    {
                        transform.position = Vector2.Lerp(
                        transform.position ,
                        new Vector2(
                            transform.position.x ,
                            groundHit.point.y + heightOfPlayer / 2f
                        ) ,
                        smoothGroundedTransitionTime
                        );
                    }
                    else
                    {

                        transform.position = new Vector2(
                        transform.position.x ,
                        groundHit.point.y + heightOfPlayer / 2f 
                        );

                    }

                    break;

                }
            }

            if( numberOfCollisions <= 1 && groundHit.distance <= 0.2f )
            {
                if( colls[0] != null )
                {
                    RaycastHit2D slopeHit = Physics2D.Raycast(
                        transform.TransformDirection( originOfGroundRayCast ) ,
                        Vector2.down ,
                        terrainLayerMask
                        );

                    if( slopeHit.transform != colls[0].transform )
                    {
                        isGrounded = false;
                        return;
                    }
                }
            }

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
            Vector2 rayCastOrigin = new Vector2( 
                transform.position.x + originOfGroundRayCast.x ,
                transform.position.y + originOfGroundRayCast.y
                );

            Vector2 groundCheckPosition = new Vector2(
                transform.position.x + groundCheckPointCircle.x ,
                transform.position.y + groundCheckPointCircle.y
                );

            RaycastHit2D rayHit = Physics2D.Raycast( rayCastOrigin , Vector2.down );

            Gizmos.color = Color.blue;

            Gizmos.DrawSphere( rayCastOrigin , 0.1f );


            if( rayHit )
            {
                Gizmos.DrawLine( rayCastOrigin, rayHit.point );
            }

            //Ground checker confirm
            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere( groundCheckPosition , groundCheckPointSize );

        }

    }
}
