using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( BoxCollider2D ) )]
public class CharacterController2D : RaycastController
{
    public struct CollisionsInfo
    {
        public bool top, bottom, left, right;
        public bool climbingSlope;
        public bool descendingSlope;
        public bool slidingDownSlope;
        public Collider2D phasingDownPlatform;

        public Vector2 slopeNormal;

        public float slopeAngle, previousSlopeAngle;
        public int faceDir;

        public void Reset()
        {
            top = bottom = left = right = climbingSlope = descendingSlope = slidingDownSlope = false;
            previousSlopeAngle = slopeAngle;
            slopeAngle = 0;
            slopeNormal = Vector2.zero;
        }
    }

    public CollisionsInfo collisions;


    [Space] [Header("Slope Options")]
    [SerializeField] private float maxSlopeAngle = 70.0f;

    [HideInInspector] public bool isGrounded;
    bool phaseDownKeyPressed;


    public float MaxClimbAngle { get => maxSlopeAngle; }


    [SerializeField] private bool isWallslidingWithNoXMovment = true;

    public override void Start()
    {
        base.Start();
        phaseDownKeyPressed = false;
        collisions.faceDir = 1;

    }

    internal void Move( Vector2 velocity , bool isOnPlatform = false)
    {
        UpdateRaycastOrigins();

        collisions.Reset();

        oldVelocity = velocity;


        if(velocity.y < 0f)
            DescendSlope( ref velocity );

        if(velocity.x != 0f)
        {
            collisions.faceDir = Math.Sign( velocity.x );
        }

        // for wall sliding if near wall
        if( velocity.x != 0f || isWallslidingWithNoXMovment )
            HorizontalCollisions( ref velocity );

        if( velocity.y != 0f )
            VerticalCollisions( ref velocity );

        transform.Translate( velocity );

        if(isOnPlatform)
        {
            collisions.bottom = true;
            isGrounded = true;
        }
    }

    public void SetPhasingDown(bool phaseDownKeyPressed_)
    {
        phaseDownKeyPressed = phaseDownKeyPressed_;
    }


    private void HorizontalCollisions( ref Vector2 velocity )
    {
        float directionX = collisions.faceDir;
        float raycastLength = Mathf.Abs( velocity.x ) + skinWidth;

        if(Mathf.Abs(velocity.x) < skinWidth)
        {
            raycastLength = 2f * skinWidth;
        }

        for( int i = 0 ; i < horizontalRayCount ; i++ )
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);

            RaycastHit2D hit = Physics2D.Raycast( rayOrigin , Vector2.right * directionX , raycastLength , collisionMask );

            Debug.DrawRay( rayOrigin , Vector2.right * directionX , Color.red );

            if( hit )
            {

                if(hit.distance == 0f )
                {
                    //this makes clipping possible and really consistent on ledges(needs diffrent solution)
                    continue;
                }

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (i == 0 && slopeAngle <= maxSlopeAngle)
                {

                    //if descending almost vertical slope character slows down at the transition moment , this if fixes it
                    if(collisions.descendingSlope)
                    {
                        collisions.descendingSlope = false;
                        velocity = oldVelocity;
                    }

                    float distanceToSlope = 0;
                    if(slopeAngle != collisions.previousSlopeAngle)
                    {
                        distanceToSlope = hit.distance - skinWidth;
                        velocity.x -= distanceToSlope * directionX;
                    }

                    ClimbSlope( ref velocity , slopeAngle , hit.normal );
                    velocity.x += distanceToSlope * directionX;
                }

                if(!collisions.climbingSlope || slopeAngle > maxSlopeAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    raycastLength = hit.distance;

                    if(collisions.climbingSlope)
                    {
                        velocity.y = Mathf.Tan(collisions.slopeAngle*Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    collisions.right = directionX == 1;
                    collisions.left = directionX == -1;
                }
            }

        }

    }


    private void VerticalCollisions( ref Vector2 velocity)
    {

        float directionY = Mathf.Sign( velocity.y );
        float raycastLength = Mathf.Abs( velocity.y ) + skinWidth;


        for( int i = 0 ; i < verticalRayCount ; i++ )
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);

            RaycastHit2D hit = Physics2D.Raycast( rayOrigin , Vector2.up * directionY , raycastLength , collisionMask );

            Debug.DrawRay( rayOrigin , Vector2.up * directionY , Color.red );

            if( hit )
            {
                if(hit.collider.CompareTag("PhaseUpward"))
                {
                    if(directionY == 1 || hit.distance <= 0f)
                        continue;
                    if(phaseDownKeyPressed)
                    {
                        continue;
                    }
                }

                velocity.y = (hit.distance - skinWidth) * directionY;
                raycastLength = hit.distance;

                if(collisions.climbingSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }

                collisions.bottom = directionY == -1;
                isGrounded = directionY == -1;

                collisions.top = directionY == 1;
            }

            //check if changing from slope to slope for smooth transition (might be  a good place to fix those 2 bugs - dont know yet)
            if(collisions.climbingSlope)
            {
                float directionX = Mathf.Sign( velocity.x );
                raycastLength = Mathf.Abs( velocity.x ) + skinWidth;
                Vector2 newRayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
                RaycastHit2D slopeHit = Physics2D.Raycast( newRayOrigin , Vector2.right * directionX , raycastLength , collisionMask );

                if( slopeHit )
                {
                    float slopeAngle = Vector2.Angle( slopeHit.normal , Vector2.up );
                    if( slopeAngle  != collisions.slopeAngle)
                    {
                        velocity.x = (slopeHit.distance - skinWidth) * directionX;
                        collisions.slopeAngle = slopeAngle;
                    }
                }

            }

        }

        //checking for new slope
        if (collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(velocity.x);
            raycastLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, raycastLength, collisionMask);
            if(hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if(slopeAngle!=collisions.slopeAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                    collisions.slopeNormal = hit.normal;

                }
            }
        }

    }

    private void ClimbSlope(ref Vector2 velocity, float slopeAngle, Vector2 slopeNormal)
    {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        if(velocity.y <= climbVelocityY)
        {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            collisions.bottom = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
            collisions.slopeNormal = slopeNormal;

        }

    }


    private void DescendSlope(ref Vector2 velocity)
    {

        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast( raycastOrigins.bottomLeft , Vector2.down , Mathf.Abs( velocity.y ) + skinWidth , collisionMask );
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast( raycastOrigins.bottomRight , Vector2.down , Mathf.Abs( velocity.y ) + skinWidth , collisionMask );

        if(maxSlopeHitLeft ^ maxSlopeHitRight)
        {
            SlideDownMAxSlope( maxSlopeHitLeft , ref velocity );
            SlideDownMAxSlope( maxSlopeHitRight , ref velocity ); 
        }

        if(!collisions.slidingDownSlope)
        {
            float directionX = Mathf.Sign( velocity.x );
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
            RaycastHit2D hit = Physics2D.Raycast( rayOrigin , Vector2.down , Mathf.Infinity , collisionMask );

            if( hit )
            {
                float slopeAngle = Vector2.Angle( hit.normal , Vector2.up );
                if( slopeAngle != 0f && slopeAngle <= maxSlopeAngle )
                {
                    if( Mathf.Sign( hit.normal.x ) == directionX )
                    {
                        if( hit.distance - skinWidth <= Mathf.Tan( slopeAngle * Mathf.Deg2Rad ) * Mathf.Abs( velocity.x ) )
                        {
                            float moveDistance = Mathf.Abs( velocity.x );
                            float descendVelocityY = Mathf.Sin( slopeAngle * Mathf.Deg2Rad ) * moveDistance;
                            velocity.x = Mathf.Cos( slopeAngle * Mathf.Deg2Rad ) * moveDistance * Mathf.Sign( velocity.x );
                            velocity.y -= descendVelocityY;

                            collisions.slopeAngle = slopeAngle;
                            collisions.descendingSlope = true;
                            collisions.bottom = true;
                            collisions.slopeNormal = hit.normal;

                        }
                    }
                }
            } 
        }

    }

    private void SlideDownMAxSlope( RaycastHit2D hit , ref Vector2 velocity )
    {
        if( hit )
        {
            float slopeAngle = Vector2.Angle( hit.normal , Vector2.up );

            if(slopeAngle > maxSlopeAngle)
            {
                velocity.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs(velocity.y) - hit.distance)/Mathf.Tan( slopeAngle * Mathf.Deg2Rad);

                collisions.slopeAngle = slopeAngle;
                collisions.slidingDownSlope = true;
                collisions.slopeNormal = hit.normal;
            }
        }
    }
}
