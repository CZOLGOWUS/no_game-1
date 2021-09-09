using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( BoxCollider2D ) )]
public class CharacterController2D : RaycastController
{

    [Space] [Header("Slope Options")]
    [SerializeField] private float maxClimbAngle = 70.0f;
    [SerializeField] private float maxDescendAngle = 70.0f;

    [HideInInspector] public bool isGrounded;


    public float MaxClimbAngle { get => maxClimbAngle; }


    [SerializeField] private bool isWallslidingWithNoXMovment = true;

    public override void Start()
    {
        base.Start();
    }

    internal void Move( Vector2 velocity , bool isOnPlatform = false)
    {
        UpdateRaycastOrigins();

        collisions.Reset();

        oldVelocity = velocity;

        if(velocity.x != 0f)
        {
            collisions.faceDir = Math.Sign( velocity.x );
        }

        if(velocity.y < 0f)
            DescendSlope( ref velocity );

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

            RaycastHit2D hit = Physics2D.Raycast( rayOrigin , Vector2.right * directionX , raycastLength , terrainMask );

            Debug.DrawRay( rayOrigin , Vector2.right * directionX , Color.red );

            if( hit )
            {

                if(hit.distance == 0f)
                {
                    continue;
                }

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (i == 0 && slopeAngle <= maxClimbAngle)
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
                    ClimbSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlope * directionX;
                }

                if(!collisions.climbingSlope || slopeAngle > maxClimbAngle)
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


    private void VerticalCollisions( ref Vector2 velocity )
    {

        float directionY = Mathf.Sign( velocity.y );
        float raycastLength = Mathf.Abs( velocity.y ) + skinWidth;


        for( int i = 0 ; i < verticalRayCount ; i++ )
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);

            RaycastHit2D hit = Physics2D.Raycast( rayOrigin , Vector2.up * directionY , raycastLength , terrainMask );

            Debug.DrawRay( rayOrigin , Vector2.up * directionY , Color.red );

            if( hit )
            {

                if((hit.collider.CompareTag("PhaseUpward") && directionY == 1) || hit.distance <= 0f)
                {
                    continue;
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
                RaycastHit2D slopeHit = Physics2D.Raycast( newRayOrigin , Vector2.right * directionX , raycastLength , terrainMask );

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
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, raycastLength, terrainMask);
            if(hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if(slopeAngle!=collisions.slopeAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }

    }

    private void ClimbSlope(ref Vector2 velocity, float slopeAngle)
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
        }
        
    }


    private void DescendSlope(ref Vector2 velocity)
    {
        float directionX = Mathf.Sign( velocity.x );
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity,terrainMask);

        if(hit)
        {
            float slopeAngle = Vector2.Angle( hit.normal , Vector2.up );
            if(slopeAngle != 0f && slopeAngle <= maxDescendAngle)
            {
                if(Mathf.Sign(hit.normal.x) == directionX)
                {
                    if( hit.distance - skinWidth <= Mathf.Tan( slopeAngle * Mathf.Deg2Rad ) * Mathf.Abs( velocity.x ))
                    {
                        float moveDistance = Mathf.Abs( velocity.x );
                        float descendVelocityY = Mathf.Sin( slopeAngle * Mathf.Deg2Rad ) * moveDistance;
                        velocity.x = Mathf.Cos( slopeAngle * Mathf.Deg2Rad ) * moveDistance * Mathf.Sign( velocity.x );
                        velocity.y -= descendVelocityY;

                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.bottom = true;
                    }
                }
            }
        }

    }

}
