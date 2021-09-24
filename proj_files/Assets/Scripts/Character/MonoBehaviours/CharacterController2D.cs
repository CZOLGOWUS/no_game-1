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
            //print( "velocity in the beggining : " + velocity.y );
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
            //if (Mathf.Abs(velocity.x) < 0.0001f)
            //    velocity.x = 0f;
            //if (Mathf.Abs(velocity.y) < 0.0001f)
            //    velocity.y = 0f;
            //print("VELOCITY BEFORE but fixed: " + velocity.x + "  " + velocity.y);

            HandleDescendSlope( ref velocity );

            //set facing direction
            if ( velocity.x != 0f )
            {
                collisions.faceDirection = Math.Sign( velocity.x );
            }

            HandleHorizontalCollisions( ref velocity );
            HandleVerticalCollisionsBox( ref velocity );
            //HandleVerticalCollisions( ref velocity );
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
                    if (maxSlopeHitLeft)
                        SlideDownMaxSlope(maxSlopeHitLeft, ref velocity);
                    else
                        SlideDownMaxSlope(maxSlopeHitRight, ref velocity); 
                } 

                print("isSliding: "+collisions.isSlidingDownSlope);
                if( !collisions.isSlidingDownSlope )
                {
                    float directionX = Mathf.Abs( velocity.x );
                    Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;

                    RaycastHit2D hit = Physics2D.Raycast( rayOrigin , Vector2.down , Mathf.Infinity , collisionMask );

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
                print("Slope angle: "+ slopeAngle);
                Debug.DrawLine(hit.point, hit.point + hit.normal, Color.magenta);

                if ( slopeAngle > maxSlopeAngle )
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


            //get cast origin dependent on direction of the next theorical position
            Vector2 boxRayOrigin = (directionX == -1) ? boxCastOrigins.leftCenter : boxCastOrigins.rightCenter;

            Vector2 boxCastSize = new Vector2( SkinWidth , boundsHeight - SkinWidth * 2f );


            //cast the box
            Physics2D.BoxCast( boxRayOrigin , boxCastSize , 0f , Vector2.right * directionX , boxCastContactFilter , boxCastResults , boxCastLength );

            #region debuging
            Debug.DrawRay( boxRayOrigin + Vector2.up * boundsHeight * 0.5f , Vector2.right * boxCastLength * collisions.faceDirection , Color.red );
            Debug.DrawRay( boxRayOrigin - Vector2.up * boundsHeight * 0.5f , Vector2.right * boxCastLength * collisions.faceDirection , Color.red );
            Debug.DrawRay( boxRayOrigin + new Vector2( collisions.faceDirection * boxCastLength , boundsHeight * 0.5f ) , Vector2.down * boundsHeight , Color.red );
            #endregion

            for( int i = 0 ; i < boxCastResults.Count ; i++ )
            {

                RaycastHit2D hit = boxCastResults[i];
                //float distance = Mathf.Abs(hit.point.x - boxRayOrigin.x); // causes a lot of bugs!
                float distance = hit.distance;
                float slopeAngle = Vector2.Angle( hit.normal , Vector2.up );

                if( hit.collider.CompareTag( platfromTag ) )
                    continue;

                collisions.SetSlopeAngle( slopeAngle , hit.normal );

                //calculate slope displacemant angle if less than max Angle
                HandleSlopeAscending( ref velocity , slopeAngle );

                if( !collisions.isAscendingSlope || slopeAngle > maxSlopeAngle )
                {
                    velocity.x = distance * directionX;

                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                }
                else
                {
                    //adjust if slope that is before Actor is too steep
                    velocity.y = Mathf.Tan( collisions.slopeAngle * Mathf.Deg2Rad ) * Mathf.Abs( velocity.x );
                }
            }

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

            float raycastLength = Mathf.Abs( velocity.y ) + SkinWidth * 2f;

            for( int i = 0 ; i < verticalRayCount ; i++ )
            {
                //get ray origin of the n-th ray depending on direction
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);

                //cast the n-th ray
                RaycastHit2D hit = Physics2D.Raycast( rayOrigin , Vector2.up * directionY , raycastLength , collisionMask );


                if( hit )
                {
                    #region debbuging
                    Debug.DrawRay( rayOrigin , Vector2.up * directionY * raycastLength , Color.red );
                    #endregion

                    raycastLength = hit.distance;

                    if( collisions.bottom )
                        collisions.ResetPhasingPlatformState();

                    //phase thruogh if:
                    if( hit.collider.CompareTag( platfromTag ) && (directionY == 1 || hit.distance <= 0f || hit.collider == collisions.phasingDownPlatform) )
                        continue;

                    AdjustVelocityToWalkingSurface( ref velocity , directionY , hit.distance);

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

        RaycastHit2D[] debugVerHit;
        private void HandleVerticalCollisionsBox( ref Vector2 velocity )
        {
            
            float directionY = Mathf.Sign( velocity.y - skinWidth );

            float boxCastDistance = Mathf.Abs( velocity.y ) + skinWidth;

            Vector2 boxCastSize = new Vector2( boundsWidth - 2f * skinWidth , skinWidth );
            Vector2 castOrigin = (directionY == -1) ? boxCastOrigin.bottomCenter : boxCastOrigin.topCenter;

            boxCastResults.Clear();
            RaycastHit2D[] allColisions = Physics2D.BoxCastAll(castOrigin, boxCastSize, 0f, Vector2.up * directionY, boxCastDistance, collisionMask);
            debugVerHit = allColisions;

            #region debbuging
            Debug.DrawRay(castOrigin + Vector2.right * boundsWidth / 2f, Vector2.up * directionY * boxCastDistance, Color.blue);
            Debug.DrawRay(castOrigin - Vector2.right * boundsWidth / 2f + Vector2.up * directionY * boxCastDistance, Vector2.right * boundsWidth, Color.blue);
            Debug.DrawRay(castOrigin - Vector2.right * boundsWidth / 2f, Vector2.up * directionY * boxCastDistance, Color.blue);
            #endregion

            if (allColisions != null && allColisions.Length>0)
            {
                boxCastResults.AddRange(allColisions);
            }

            //print("COUNT OF COLLISIONS: "+ boxCastResults.Count);
            for ( int i = 0 ; i < boxCastResults.Count ; i++ )
            {
                RaycastHit2D hit = boxCastResults[i];
                //float distance = Mathf.Abs(castOrigin.y  - hit.point.y); // causes a lot of bugs!
                float distance = hit.distance;

                Debug.DrawLine(castOrigin, hit.point, Color.green);


                if( collisions.bottom )
                    collisions.ResetPhasingPlatformState();

                //phase thruogh if:
                if( hit.collider.CompareTag( platfromTag ) && (directionY == 1 || distance <= 0f || hit.collider == collisions.phasingDownPlatform) )
                    continue;

                AdjustVelocityToWalkingSurface( ref velocity , directionY , distance );

                collisions.bottom = directionY == -1;
                collisions.top = directionY == 1;



                //check if changing from slope to slope and if so then change velocity vector for smooth transition
                if( CheckForToSlopeTransition( ref velocity , hit , out RaycastHit2D slopeHit ) )
                {
                    AdjustVelocityToNewSlope(ref velocity , hit , slopeHit );
                }

            }

        }



        private void AdjustVelocityToWalkingSurface( ref Vector2 velocity , float directionY , float distance )
        {
            velocity.y = (distance - SkinWidth) * directionY;
            if( collisions.isAscendingSlope )
            {
                velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
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
            //transform.position += new Vector3( velocity.x , velocity.y );
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


        private void OnDrawGizmos()
        {
            //if(debugVerHit!=null)
            //    for(int i =0; i<debugVerHit.Length; i++)
            //        Gizmos.DrawSphere( debugVerHit[i].point , 0.05f);
        }

    }
}
