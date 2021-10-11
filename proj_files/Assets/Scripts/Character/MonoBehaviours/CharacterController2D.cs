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
            public bool isAscendingSlope;
            public bool isDescendingSlope;
            public bool isSlidingDownSlope;

            public Collider2D phasingDownPlatform;

            //horizontal collision angles
            public Vector2 slopeNormal;
            public float slopeAngle, previousSlopeAngle;

            //direction of actor
            public int faceDirection;

            public void ResetPhasingPlatformState()
            {
                phasingDownPlatform = null;
            }

            public void Reset()
            {
                top = bottom = left = right = isAscendingSlope = isDescendingSlope = isSlidingDownSlope = false;
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
        [SerializeField] private float maxSlideUpSlopeAngle = 90f;


        //dependencies
        [Tooltip( "tag platforms with this tag that are supposed to be \"phaseable\"" )]
        [SerializeField] private string platfromTag = "Platform";


        //helping variables
        private List<RaycastHit2D> boxCastResults = new List<RaycastHit2D>();
        private ContactFilter2D boxCastContactFilter = new ContactFilter2D();


        public float MaxClimbAngle { get => maxSlopeAngle; }
        public string PlatformTag { get => platfromTag; }
        public bool isGrounded { get => collisions.bottom; }






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

            #endregion

            HandleCollisions( ref velocity );

            FinalMove( velocity );

            HandlePlatformGroundedState( isOnPlatform );

        }


        private void HandleCollisions( ref Vector2 velocity )
        {
            HandleDescendSlope( ref velocity );

            //set facing direction
            if( velocity.x != 0f )
            {
                collisions.faceDirection = Math.Sign( velocity.x );
            }

            HandleHorizontalCollisions( ref velocity );

            HandleVerticalCollisions( ref velocity );
        }


        private void HandleDescendSlope( ref Vector2 velocity )
        {
            if( velocity.y < 0f )
            {
                RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast( raycastOrigins.bottomLeft , Vector2.down , Mathf.Abs( velocity.y ) + SkinWidth , collisionMask );
                RaycastHit2D maxSlopeHitRight = Physics2D.Raycast( raycastOrigins.bottomRight , Vector2.down , Mathf.Abs( velocity.y ) + SkinWidth , collisionMask );

                //if only one of these two hit
                if( maxSlopeHitLeft ^ maxSlopeHitRight )
                {
                    if( maxSlopeHitLeft )
                        SlideDownMaxSlope( maxSlopeHitLeft , ref velocity );
                    else
                        SlideDownMaxSlope( maxSlopeHitRight , ref velocity );
                }
                //if two were ghit pick one with shorter distance
                else if( maxSlopeHitLeft && maxSlopeHitRight )
                {
                    if( maxSlopeHitLeft.distance < maxSlopeHitRight.distance )
                        SlideDownMaxSlope( maxSlopeHitLeft , ref velocity );
                    else
                        SlideDownMaxSlope( maxSlopeHitRight , ref velocity );
                }

                if( !collisions.isSlidingDownSlope )
                {
                    int directionX = Math.Sign( velocity.x );
                    Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;

                    RaycastHit2D hit = Physics2D.Raycast( rayOrigin , Vector2.down , Mathf.Abs( velocity.y ) + SkinWidth , collisionMask );

                    if( hit )
                    {
                        float slopeAngle = Vector2.Angle( hit.normal , Vector2.up );
                        collisions.SetSlopeAngle( slopeAngle , hit.normal );

                        if( IsDescendingSlope( velocity , directionX , hit , slopeAngle ) )
                        {
                            AdjustVelocityToSlopeDescending( ref velocity , slopeAngle );

                            collisions.isDescendingSlope = true;
                            collisions.bottom = true;
                        }
                    }
                }
            }

        }


        private bool IsDescendingSlope( Vector2 velocity , float directionX , RaycastHit2D hit , float slopeAngle )
        {
            return
                (slopeAngle != 0f) &&
                (slopeAngle <= maxSlopeAngle) &&
                (Mathf.Sign( hit.normal.x ) == directionX) &&
                hit.distance - SkinWidth <= Mathf.Tan( slopeAngle * Mathf.Deg2Rad ) * Mathf.Abs( velocity.x ); // vy/vx * vx ?
        }


        private void AdjustVelocityToSlopeDescending( ref Vector2 velocity , float slopeAngle )
        {
            float moveDistance = Mathf.Abs( velocity.x );
            float descendVelocityY = Mathf.Sin( slopeAngle * Mathf.Deg2Rad ) * moveDistance;

            velocity.x = Mathf.Cos( slopeAngle * Mathf.Deg2Rad ) * moveDistance * Mathf.Sign( velocity.x );
            velocity.y -= descendVelocityY;
        }


        private void SlideDownMaxSlope( RaycastHit2D hit , ref Vector2 velocity )
        {
            if( hit )
            {
                float slopeAngle = Vector2.Angle( hit.normal , Vector2.up );
                collisions.SetSlopeAngle( slopeAngle , hit.normal );

                #region draw wall normal
                Debug.DrawLine( hit.point , hit.point + hit.normal , Color.magenta );
                #endregion

                if( slopeAngle > maxSlopeAngle )
                {
                    velocity.x = Mathf.Sign( hit.normal.x ) * (Mathf.Abs( velocity.y ) - hit.distance) / Mathf.Tan( slopeAngle * Mathf.Deg2Rad );
                    collisions.isSlidingDownSlope = true;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }


        private void HandleHorizontalCollisions( ref Vector2 velocity )
        {

            int directionX = collisions.faceDirection;
            //2*x cuz of box cast
            float boxCastLength = Mathf.Abs( velocity.x ) + 2.0f * SkinWidth;


            //box cast setup
            Vector2 boxRayOrigin = (directionX == -1) ? boxCastOrigins.leftCenter : boxCastOrigins.rightCenter;
            Vector2 boxCastSize = new Vector2( SkinWidth , boundsHeight - SkinWidth * 2f );


            //cast the box
            Physics2D.BoxCast( boxRayOrigin , boxCastSize , 0f , Vector2.right * directionX , boxCastContactFilter , boxCastResults , boxCastLength );

            //sorts array from Min angle to Max angle
            boxCastResults.Sort( ( hit1 , hit2 ) => { return hit2.normal.y.CompareTo( hit1.normal.y ); } );

            #region debuging
            Debug.DrawRay( boxRayOrigin + Vector2.up * boundsHeight * 0.5f , Vector2.right * boxCastLength * collisions.faceDirection , Color.red );
            Debug.DrawRay( boxRayOrigin - Vector2.up * boundsHeight * 0.5f , Vector2.right * boxCastLength * collisions.faceDirection , Color.red );
            Debug.DrawRay( boxRayOrigin + new Vector2( collisions.faceDirection * boxCastLength , boundsHeight * 0.5f ) , Vector2.down * boundsHeight , Color.red );
            #endregion

            for( int i = 0 ; i < boxCastResults.Count ; i++ )
            {
                RaycastHit2D hit = boxCastResults[i];
                float distance = hit.distance;
                float slopeAngle = Vector2.Angle( hit.normal , Vector2.up );

                if( hit.collider.CompareTag( platfromTag ) )
                    continue;

                collisions.SetSlopeAngle( slopeAngle , hit.normal );

                //calculate slope displacemant(velocity vector) angle if less than max Angle
                HandleSlopeAscending( ref velocity , slopeAngle );

                //check if terain hit is a wall or angle of the wall is too much too climb
                if( !collisions.isAscendingSlope || slopeAngle > maxSlopeAngle )
                    SnapToHorizontalHit( ref velocity , directionX , distance , slopeAngle );

            }

        }

        private void SnapToHorizontalHit( ref Vector2 velocity , int directionX , float distanceToHit , float slopeAngle )
        {
            velocity.x = distanceToHit * directionX;

            //if horizontal wall hit and is maxSlope or verticalWall then adjust y velocity
            if( collisions.isAscendingSlope && collisions.previousSlopeAngle != slopeAngle )
            {
                velocity.y = Mathf.Sin( slopeAngle * Mathf.Deg2Rad ) * velocity.x * directionX;
            }

            collisions.left = directionX == -1;
            collisions.right = directionX == 1;
        }




        /// <summary>
        /// 
        /// calculate new slope displacemant angle if less than max Angle
        ///
        /// </summary>
        /// <param name="velocity">velocity vector</param>
        /// <param name="slopeAngle">angle of the slope that we want to climb</param>
        private void HandleSlopeAscending( ref Vector2 velocity , float slopeAngle )
        {
            if( slopeAngle > maxSlopeAngle || slopeAngle == 0f )
            {
                return;
            }

            float moveDistance = Mathf.Abs( velocity.x );
            float climbVelocityY = Mathf.Sin( slopeAngle * Mathf.Deg2Rad ) * moveDistance;

            if( velocity.y <= climbVelocityY )
            {
                velocity.y = climbVelocityY;
                velocity.x = Mathf.Cos( slopeAngle * Mathf.Deg2Rad ) * moveDistance * Mathf.Sign( velocity.x );
                collisions.bottom = true;
                collisions.isAscendingSlope = true;

            }

        }


        private void HandleVerticalCollisions( ref Vector2 velocity )
        {

            float directionY = Mathf.Sign( velocity.y );

            float raycastLength = Mathf.Abs( velocity.y ) + SkinWidth;

            for( int i = 0 ; i < verticalRayCount ; i++ )
            {
                //get ray origin of the n-th ray depending on direction
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);

                //cast the n-th ray
                RaycastHit2D hit = Physics2D.Raycast( rayOrigin , Vector2.up * directionY , raycastLength , collisionMask );

                #region debbuging
                Debug.DrawRay( rayOrigin , Vector2.up * directionY * raycastLength , Color.red );
                #endregion

                if( hit )
                {
                    raycastLength = hit.distance;

                    if( collisions.bottom )
                        collisions.ResetPhasingPlatformState();

                    //phase thruogh if:
                    if( hit.collider.CompareTag( platfromTag ) && (directionY == 1 || hit.distance <= 0f || hit.collider == collisions.phasingDownPlatform) )
                        continue;

                    SnapToSurface( ref velocity , directionY , hit );

                    collisions.bottom = directionY == -1;
                    collisions.top = directionY == 1;

                }

                //check if changing from slope to slope and if so then change velocity vector for smooth transition
                if( CheckForToSlopeTransition( ref velocity , hit , out RaycastHit2D slopeHit ) )
                {
                    AdjustVelocityToNewSlope( ref velocity , hit , slopeHit );
                }

            }
        }


        private void SnapToSurface( ref Vector2 velocity , float directionY , RaycastHit2D hit )
        {
            velocity.y = (hit.distance - SkinWidth) * directionY;

            if( collisions.isAscendingSlope )
            {
                velocity.x = velocity.y / Mathf.Tan( collisions.slopeAngle * Mathf.Deg2Rad ) * Mathf.Sign( velocity.x );
            }

        }


        private bool CheckForToSlopeTransition( ref Vector2 velocity , RaycastHit2D hit , out RaycastHit2D slopeHit )
        {
            if( collisions.isAscendingSlope )
            {
                slopeHit = RaycastForNewSlope( velocity );

                if( slopeHit )
                    return true;

            }

            slopeHit = hit;
            return false;
        }


        private RaycastHit2D RaycastForNewSlope( Vector2 velocity )
        {
            RaycastHit2D slopeHit;
            float directionX = Mathf.Sign( velocity.x );
            float raycastLength = Mathf.Abs( velocity.x ) + SkinWidth;

            Vector2 newRayOrigin = ((directionX == -1) ?
                raycastOrigins.bottomLeft :
                raycastOrigins.bottomRight) + Vector2.up * velocity.y;

            slopeHit = Physics2D.Raycast( newRayOrigin , Vector2.right * directionX , raycastLength , collisionMask );

            return slopeHit;
        }


        private void AdjustVelocityToNewSlope( ref Vector2 velocity , RaycastHit2D hit , RaycastHit2D slopeHit )
        {
            int directionX = Math.Sign( velocity.x );
            float slopeAngle = Vector2.Angle( slopeHit.normal , Vector2.up );

            if( slopeAngle != collisions.slopeAngle )
            {
                velocity.x = (slopeHit.distance - SkinWidth) * directionX;
                collisions.SetSlopeAngle( slopeAngle , hit.normal );
            }

        }


        private void FinalMove( Vector2 velocity )
        {
            transform.Translate( velocity );
        }


        private void HandlePlatformGroundedState( bool isOnPlatform )
        {
            if( isOnPlatform )
            {
                collisions.bottom = true;
            }
        }


        public void PhaseThroughtPlatform( Collider2D platform )
        {
            collisions.phasingDownPlatform = platform;
        }

    }
}
