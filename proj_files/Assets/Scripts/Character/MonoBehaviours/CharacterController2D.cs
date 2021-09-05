using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( BoxCollider2D ) )]
public class CharacterController2D : MonoBehaviour
{
    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    public struct CollisionsInfo
    {
        public bool top, bottom, left, right;
        public bool climbingSlope;
        public bool descendingSlope;
        public float slopeAngle, previousSlopeAngle;

        public void Reset()
        {
            top = bottom = left = right = climbingSlope = descendingSlope = false;
            previousSlopeAngle = slopeAngle;
            slopeAngle = 0;
        }
    }

    BoxCollider2D thisCollider;
    RaycastOrigins raycastOrigins;

    public LayerMask terrainMask;

    [Space] [Header("Ray Options")]
    [SerializeField] private int horizontalRayCount = 4;
    [SerializeField] private int verticalRayCount = 4;

    [Space] [Header("Slope Options")]
    [SerializeField] private float maxClimbAngle = 70.0f;
    [SerializeField] private float maxDescendAngle = 70.0f;

    [HideInInspector] public bool isGrounded;
    public CollisionsInfo collisions;


    private Vector3 oldVelocity;


    private float horizontalRaySpacing;
    private float verticalRaySpacing;
    private const float skinWidth = 0.015f;

    public float MaxClimbAngle { get => maxClimbAngle; }

    private void Start()
    {
        thisCollider = GetComponent<BoxCollider2D>();

        CalculateRaySpacing();
    }


    internal void Move( Vector3 velocity )
    {
        UpdateRaycastOrigins();

        collisions.Reset();

        oldVelocity = velocity;

        if(velocity.y < 0f)
            DescendSlope( ref velocity );

        if( velocity.x != 0f )
            HorizontalCollisions( ref velocity );

        if( velocity.y != 0f )
            VerticalCollisions( ref velocity );

        transform.Translate( velocity );
    }


    private void HorizontalCollisions( ref Vector3 velocity )
    {
        float directionX = Mathf.Sign( velocity.x );
        float raycastLength = Mathf.Abs( velocity.x ) + skinWidth;


        for( int i = 0 ; i < horizontalRayCount ; i++ )
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);

            RaycastHit2D hit = Physics2D.Raycast( rayOrigin , Vector2.right * directionX , raycastLength , terrainMask );

            Debug.DrawRay( rayOrigin , Vector2.right * directionX * raycastLength , Color.red );

            if( hit )
            {
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

                if(!collisions.climbingSlope || collisions.slopeAngle > maxClimbAngle)
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


    private void VerticalCollisions( ref Vector3 velocity )
    {

        float directionY = Mathf.Sign( velocity.y );
        float raycastLength = Mathf.Abs( velocity.y ) + skinWidth;


        for( int i = 0 ; i < verticalRayCount ; i++ )
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);

            RaycastHit2D hit = Physics2D.Raycast( rayOrigin , Vector2.up * directionY , raycastLength , terrainMask );

            Debug.DrawRay( rayOrigin , Vector2.up * directionY * raycastLength , Color.red );

            if( hit )
            {
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

    private void ClimbSlope(ref Vector3 velocity, float slopeAngle)
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


    private void DescendSlope(ref Vector3 velocity)
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


    private void UpdateRaycastOrigins()
    {
        Bounds bounds = thisCollider.bounds;
        bounds.Expand( skinWidth * -2f );

        raycastOrigins.bottomLeft = new Vector2( bounds.min.x , bounds.min.y );
        raycastOrigins.bottomRight = new Vector2( bounds.max.x , bounds.min.y );
        raycastOrigins.topLeft = new Vector2( bounds.min.x , bounds.max.y );
        raycastOrigins.topRight = new Vector2( bounds.max.x , bounds.max.y );
    }


    private void CalculateRaySpacing()
    {
        Bounds bounds = thisCollider.bounds;
        bounds.Expand( skinWidth * -2f );

        horizontalRayCount = Mathf.Clamp( horizontalRayCount , 2 , int.MaxValue );
        verticalRayCount = Mathf.Clamp( verticalRayCount , 2 , int.MaxValue );

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);

    }

}
