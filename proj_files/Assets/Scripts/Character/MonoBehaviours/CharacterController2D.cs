using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using noGame.Collisions;

namespace noGame.Characters
{
    [RequireComponent( typeof( BoxCollider2D ) )]
    public class CharacterController2D : RaycastController
    {
        public struct CollisionsInfo
        {
            //collisions
            public bool top, bottom, left, right;

            //slope states
            public bool climbingSlope;
            public bool descendingSlope;
            public bool slidingDownSlope;

            public Collider2D phasingDownPlatform;

            //horizontal collision angles
            public Vector2 slopeNormal;
            public float slopeAngle, previousSlopeAngle;
            
            //direction of actor
            public int faceDirection;

            public void Reset()
            {
                top = bottom = left = right = climbingSlope = descendingSlope = slidingDownSlope = false;
                previousSlopeAngle = slopeAngle;
                slopeAngle = 0;
                slopeNormal = Vector2.zero;
            }

            public void SetSlopeAngle( float angle , Vector2 normal )
            {
                previousSlopeAngle = slopeAngle;
                slopeAngle = angle;
                slopeNormal = normal;
            }

        }

        //struct declaration
        public CollisionsInfo collisions;


        [Space]
        [Header( "Slope Options" )]
        [SerializeField] private float maxSlopeAngle = 70.0f;


        public float MaxClimbAngle { get => maxSlopeAngle; }


        //helping variables
        private List<RaycastHit2D> boxCastResults = new List<RaycastHit2D>();
        private ContactFilter2D boxCastContactFilter = new ContactFilter2D();





        public override void Start()
        {
            base.Start();

            collisions.faceDirection = 1;
            boxCastContactFilter.SetLayerMask( collisionMask );

        }

        internal void Move( Vector2 velocity , bool isOnPlatform = false )
        {
            #region SetUp

            UpdateRaycastOrigins();
            UpdateBoxCastOrigins();

            collisions.Reset();
            oldVelocity = velocity;

            #endregion

            HandleDescendSlope( ref velocity );

            SetFacingDirection( velocity );

            HandleHorizontalCollisions( ref velocity );
            HandleVerticalCollisions( ref velocity );

            //Final, Actual Move Function
            FinalMove( velocity );

            HandlePlatformGroundedState( isOnPlatform );

        }


        private void HandleDescendSlope( ref Vector2 velocity )
        {
            if( velocity.y < 0f )
            {
                RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast( raycastOrigins.bottomLeft , Vector2.down , Mathf.Abs( velocity.y ) + SkinWidth , collisionMask );
                RaycastHit2D maxSlopeHitRight = Physics2D.Raycast( raycastOrigins.bottomRight , Vector2.down , Mathf.Abs( velocity.y ) + SkinWidth , collisionMask );

                //if only one of these two hit
                if( maxSlopeHitLeft ^ maxSlopeHitRight )
                    if( maxSlopeHitLeft )
                        SlideDownMaxSlope( maxSlopeHitLeft , ref velocity );
                    else
                        SlideDownMaxSlope( maxSlopeHitRight , ref velocity );

                if( !collisions.slidingDownSlope )
                {
                    float directionX = Mathf.Sign( velocity.x );
                    Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;

                    RaycastHit2D hit = Physics2D.Raycast( rayOrigin , Vector2.down , Mathf.Infinity , collisionMask );

                    if( hit )
                    {
                        float slopeAngle = Vector2.Angle( hit.normal , Vector2.up );
                        collisions.SetSlopeAngle( slopeAngle , hit.normal );

                        if( slopeAngle != 0f &&
                            slopeAngle <= maxSlopeAngle &&
                            Mathf.Sign( hit.normal.x ) == directionX &&
                            hit.distance - SkinWidth <= Mathf.Tan( slopeAngle * Mathf.Deg2Rad ) * Mathf.Abs( velocity.x ) )
                        {

                            float moveDistance = Mathf.Abs( velocity.x );
                            float descendVelocityY = Mathf.Sin( slopeAngle * Mathf.Deg2Rad ) * moveDistance;

                            velocity.x = Mathf.Cos( slopeAngle * Mathf.Deg2Rad ) * moveDistance * Mathf.Sign( velocity.x );
                            velocity.y -= descendVelocityY;

                            collisions.descendingSlope = true;
                            collisions.bottom = true;
                        }
                    }
                }
            }

        }


        private void SetFacingDirection( Vector2 velocity )
        {
            if( velocity.x != 0f )
            {
                collisions.faceDirection = Math.Sign( velocity.x );
            }
        }


        private void SlideDownMaxSlope( RaycastHit2D hit , ref Vector2 velocity )
        {
            if( hit )
            {
                float slopeAngle = Vector2.Angle( hit.normal , Vector2.up );
                collisions.SetSlopeAngle( slopeAngle , hit.normal );

                if( slopeAngle > maxSlopeAngle )
                {
                    velocity.x = Mathf.Sign( hit.normal.x ) * (Mathf.Abs( velocity.y ) - hit.distance) / Mathf.Tan( slopeAngle * Mathf.Deg2Rad );
                    collisions.slidingDownSlope = true;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }


        private void HandleHorizontalCollisions( ref Vector2 velocity )
        {

            int directionX = collisions.faceDirection;
            //2*x cuz of box cast
            float raycastLength = Mathf.Abs( velocity.x ) + SkinWidth * 2f;
            //previous largest angle of the wall hit this frame in for loop
            float prevAngle = 0f;

            //get cast origin dependent on direction of the next theorical position
            Vector2 boxRayOrigin = (directionX == -1) ?
                boxCastOrigins.leftCenter :
                boxCastOrigins.rightCenter;


            Vector2 boxCastSize = new Vector2( SkinWidth , boundsHeight - SkinWidth );

            Physics2D.BoxCast( boxRayOrigin , boxCastSize , 0f , Vector2.right * directionX , boxCastContactFilter , boxCastResults , raycastLength );

            #region debuging
            float debugBoxLength = 0.1f;
            Debug.DrawRay( boxRayOrigin + Vector2.up * boundsHeight * 0.5f , Vector2.right * debugBoxLength * collisions.faceDirection , Color.red );
            Debug.DrawRay( boxRayOrigin - Vector2.up * boundsHeight * 0.5f , Vector2.right * debugBoxLength * collisions.faceDirection , Color.red );
            Debug.DrawRay( boxRayOrigin + new Vector2(collisions.faceDirection * debugBoxLength , boundsHeight * 0.5f) , Vector2.down * boundsHeight , Color.red );
            #endregion

            for( int i = 0 ; i < boxCastResults.Count ; i++ )
            {

                RaycastHit2D hit = boxCastResults[i];
                float slopeAngle = Vector2.Angle( hit.normal , Vector2.up );

                if( prevAngle < slopeAngle )
                {
                    if( hit.collider.CompareTag( "Phasing" ) )
                        continue;

                    collisions.SetSlopeAngle( slopeAngle , hit.normal );

                    //calculate slope displacemant angle if less than max Angle
                    HandleSlopeClimbing( ref velocity , slopeAngle );

                    if( !collisions.climbingSlope || slopeAngle > maxSlopeAngle )
                    {
                        velocity.x = hit.distance * directionX;

                        collisions.left = directionX == -1;
                        collisions.right = directionX == 1;
                    }
                    else
                    {
                        //adjust if slope that is before Actor is too steep
                        velocity.y = Mathf.Tan( collisions.slopeAngle * Mathf.Deg2Rad ) * Mathf.Abs( velocity.x );
                    }

                    prevAngle = slopeAngle;
                }

            }

        }

        /// <summary>
        /// /
        /// change the velocity Vector depending on The slope that we are clmbing
        /// /
        /// </summary>
        /// <param name="velocity">velocity vector</param>
        /// <param name="slopeAngle">angle of the slope that we want to climb</param>
        private void HandleSlopeClimbing( ref Vector2 velocity , float slopeAngle )
        {
            if( slopeAngle > maxSlopeAngle )
                return;

            float moveDistance = Mathf.Abs( velocity.x );
            float climbVelocityY = Mathf.Sin( slopeAngle * Mathf.Deg2Rad ) * moveDistance;

            if( velocity.y <= climbVelocityY )
            {
                velocity.y = climbVelocityY;
                velocity.x = Mathf.Cos( slopeAngle * Mathf.Deg2Rad ) * moveDistance * Mathf.Sign( velocity.x );
                collisions.bottom = true;
                collisions.climbingSlope = true;

            }
            else
            {
                collisions.descendingSlope = false;
            }

        }
        

        private void HandleVerticalCollisions( ref Vector2 velocity )
        {
            if( velocity.y != 0f )
            {
                float directionY = Mathf.Sign( velocity.y );
                float raycastLength = Mathf.Abs( velocity.y ) + SkinWidth;


                for( int i = 0 ; i < verticalRayCount ; i++ )
                {
                    Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                    rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);

                    RaycastHit2D hit = Physics2D.Raycast( rayOrigin , Vector2.up * directionY , raycastLength , collisionMask );

                    //print( collisions.phasingDownPlatform );

                    if( hit )
                    {

                        Debug.DrawRay( rayOrigin , Vector2.up * directionY , Color.red );

                        if( hit.collider.CompareTag( "Phasing" ) )
                        {
                            if( directionY == 1 || hit.distance <= 0f || hit.collider == collisions.phasingDownPlatform )
                                continue;
                        }
                        
                        if(collisions.bottom)
                        {
                            collisions.phasingDownPlatform = null;
                        }


                        velocity.y = (hit.distance - SkinWidth) * directionY;
                        raycastLength = hit.distance;

                        if( collisions.climbingSlope )
                        {
                            velocity.x = velocity.y / Mathf.Tan( collisions.slopeAngle * Mathf.Deg2Rad ) * Mathf.Sign( velocity.x );
                        }

                        collisions.bottom = directionY == -1;
                        collisions.top = directionY == 1;

                    }

                    //check if changing from slope to slope and if so then change velocity vector for smooth transition
                    if( collisions.climbingSlope )
                    {
                        float directionX = Mathf.Sign( velocity.x );
                        raycastLength = Mathf.Abs( velocity.x ) + SkinWidth;

                        Vector2 newRayOrigin = ((directionX == -1) ? 
                            raycastOrigins.bottomLeft : 
                            raycastOrigins.bottomRight) + Vector2.up * velocity.y;

                        RaycastHit2D slopeHit = Physics2D.Raycast( newRayOrigin , Vector2.right * directionX , raycastLength , collisionMask );

                        if( slopeHit )
                        {
                            float slopeAngle = Vector2.Angle( slopeHit.normal , Vector2.up );
                            if( slopeAngle != collisions.slopeAngle )
                            {
                                velocity.x = (slopeHit.distance - SkinWidth) * directionX;
                                collisions.SetSlopeAngle( slopeAngle , hit.normal );
                            }
                        }

                    }

                }

            }

        }

        private void FinalMove( Vector2 velocity )
        {
            transform.position += new Vector3( velocity.x , velocity.y );
        }

        private void HandlePlatformGroundedState( bool isOnPlatform )
        {
            if( isOnPlatform )
            {
                collisions.bottom = true;
            }
        }

        public void PhaseThroughtPlatform(Collider2D platform)
        {
            collisions.phasingDownPlatform = platform;
        }



    }
}
