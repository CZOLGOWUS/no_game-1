using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using noGame.Character.MonoBehaviours;
using System;

namespace noGame.Character.MonoBehaviours
{

    public class CharacterController2D : MonoBehaviour
    {

        private Rigidbody2D thisRigidBody;

        private Vector2 currentPosition;
        private Vector2 nextPosition;
        private Vector2 velocity;
        private Vector2 vel = Vector2.zero;

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
        [Tooltip( "distance of GroundHitRay, that is used for making sure that we will not levitate above ground if position is not precise" )]
        [SerializeField] private float groundHitRaySafeDistance = 3.1f;

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

        [Space]
        //Collisions
        private CapsuleCollider2D thisCapsuleCollider;


        private Vector2 nextVelocityVector;

        //unorginized
        private RaycastHit2D groundHit;
        private Vector2 currentGravity;
        private float currentSpeed;
        private bool isMoving = false;
        private bool isJumping = false;
        private bool isGrounded = false;

        //helping vars
        Vector2 penetrationVector = new Vector2();
        Vector2 collisionPos = new Vector2();

        Vector2 testA = new Vector2();
        Vector2 testB = new Vector2();

        public float CurrentSpeed { get => currentSpeed; }
        public Vector2 PrevPosition { get => currentPosition; }
        public Vector2 NextPosition { get => nextPosition; }
        public bool IsMoving { get => isMoving; }
        public bool IsJumping { get => isJumping; }
        public float TestX;

        private void Start()
        {
            thisCapsuleCollider = GetComponent<CapsuleCollider2D>();
            thisRigidBody = GetComponent<Rigidbody2D>();
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

            HandleCollision();
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
                if(currentGravity.magnitude <= 100f)
                    currentGravity += gravity * Time.deltaTime;
            }
            else
            {
                currentGravity = Vector2.zero;
            }
        }

        private void FixedUpdate()
        {
            //print( currentGravity );
        }

        private void Move()
        {
            Vector2 temp = velocity + currentGravity;

            //Debug.Log("Velocity " + temp + "   Grounded: "+isGrounded);

            //transform.position += transform.TransformDirection( temp  ) * Time.deltaTime;

            thisRigidBody.position += temp * Time.deltaTime;

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
                        thisRigidBody.position = Vector2.Lerp(
                        thisRigidBody.position ,
                        new Vector2(
                            thisRigidBody.position.x ,
                            groundHit.point.y + heightOfPlayer / 2f
                        ) ,
                        smoothGroundedTransitionTime
                        );
                    }
                    else
                    {

                        thisRigidBody.position = ( new Vector2( thisRigidBody.position.x , groundHit.point.y + heightOfPlayer * 0.5f ));

                    }

                    break;

                }
            }
            
            if( numberOfCollisions <= 1 && groundHit.distance <= groundHitRaySafeDistance )
            {
                if( colls[0] != null )
                {
                    RaycastHit2D slopeHit = Physics2D.Raycast(
                        thisRigidBody.position + originOfGroundRayCast ,
                        Vector2.down,
                        groundHitRaySafeDistance,
                        terrainLayerMask
                        );

                    //Debug.Log((Vector3)originOfGroundRayCast + " --> " + transform.TransformPoint((Vector3)originOfGroundRayCast));

                    if( slopeHit.transform != colls[0].transform )
                    {
                        isGrounded = false;
                        return;
                    }
                }
            }

        }

        private void HandleCollision()
        {

            Collider2D[] overlapColliders = new Collider2D[4];

            int numberOfOverlaps = Physics2D.OverlapBoxNonAlloc( 
                new Vector2( thisRigidBody.position.x, thisRigidBody.position.y) + thisCapsuleCollider.offset ,
                thisCapsuleCollider.size ,
                transform.rotation.z ,
                overlapColliders ,
                terrainLayerMask 
                );

            for( int i = 0 ; i < numberOfOverlaps ; i++ )
            {
                if( overlapColliders[i] != thisCapsuleCollider )
                {
                    print( i );


                    ColliderDistance2D colliderOverlapInfo = Physics2D.Distance( thisCapsuleCollider , overlapColliders[i] );

                    //Debug.Log( "distance in distance" + colliderOverlapInfo.distance );
                    //Debug.Log("distance in B-A"+ (colliderOverlapInfo.pointB.x - colliderOverlapInfo.pointA.x) );

                    testA = colliderOverlapInfo.pointA;
                    testB = colliderOverlapInfo.pointB;


                    thisRigidBody.position +=  colliderOverlapInfo.normal * colliderOverlapInfo.distance;
                }
                
            }

        }



        public void Jump()
        {
            //if(_isGrounded)
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
            if ( rayHit )
                Gizmos.DrawLine( rayCastOrigin, rayHit.point );

            //Ground checker confirm
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere( groundCheckPosition , groundCheckPointSize );


            Gizmos.color = Color.green;
            Gizmos.DrawLine( transform.TransformPoint( originOfGroundRayCast ), transform.TransformPoint(originOfGroundRayCast)+Vector3.down*5f );


            //hits on colliders
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere( testA, 0.03f );


            //hits on colliders
            Gizmos.color = Color.green;
            Gizmos.DrawSphere( testB, 0.03f );


            //where player is being "teleported"
            Gizmos.DrawSphere( collisionPos + penetrationVector , 0.1f );

            Gizmos.DrawLine( collisionPos + penetrationVector , collisionPos );
        }

    }
}
