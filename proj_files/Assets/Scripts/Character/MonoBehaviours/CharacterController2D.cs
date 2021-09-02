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

        public void Reset()
        {
            top = bottom = left = right = false;
        }
    }

    BoxCollider2D thisCollider;
    RaycastOrigins raycastOrigins;

    public LayerMask terrainMask;

    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;


    [HideInInspector] public bool isGrounded;
    public CollisionsInfo collisions;

    private float horizontalRaySpacing;
    private float verticalRaySpacing;
    private const float skinWidth = 0.015f;


    private void Start()
    {
        thisCollider = GetComponent<BoxCollider2D>();

        CalculateRaySpacing();
    }


    internal void Move( Vector3 velocity )
    {
        UpdateRaycastOrigins();

        collisions.Reset();

        if( velocity.x != 0 )
            HorizontalCollisions( ref velocity );

        if( velocity.y != 0 )
            VerticalCollisions( ref velocity );

        transform.Translate( velocity );
    }


    void HorizontalCollisions( ref Vector3 velocity )
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
                velocity.x = (hit.distance - skinWidth) * directionX;
                raycastLength = hit.distance;

                collisions.right = directionX == 1;
                collisions.left = directionX == -1;

            }

        }

    }


    void VerticalCollisions( ref Vector3 velocity )
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

                collisions.bottom = directionY == -1;
                isGrounded = directionY == -1;

                collisions.top = directionY == 1;

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
