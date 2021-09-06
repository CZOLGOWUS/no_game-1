using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( BoxCollider2D ) )]
public class RaycastController : MonoBehaviour
{
    protected struct RaycastOrigins
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


    protected BoxCollider2D thisCollider;
    protected RaycastOrigins raycastOrigins;


    [Space]
    [Header( "Ray Options" )]
    [SerializeField] protected int horizontalRayCount = 4;
    [SerializeField] protected int verticalRayCount = 4;
    [SerializeField] protected LayerMask terrainMask;


    public CollisionsInfo collisions;


    protected Vector3 oldVelocity;

    protected float horizontalRaySpacing;
    protected float verticalRaySpacing;
    protected const float skinWidth = 0.015f;


    public virtual void Start()
    {
        thisCollider = GetComponent<BoxCollider2D>();

        CalculateRaySpacing();
    }


    protected void UpdateRaycastOrigins()
    {
        Bounds bounds = thisCollider.bounds;
        bounds.Expand( skinWidth * -2f );

        raycastOrigins.bottomLeft = new Vector2( bounds.min.x , bounds.min.y );
        raycastOrigins.bottomRight = new Vector2( bounds.max.x , bounds.min.y );
        raycastOrigins.topLeft = new Vector2( bounds.min.x , bounds.max.y );
        raycastOrigins.topRight = new Vector2( bounds.max.x , bounds.max.y );
    }


    protected void CalculateRaySpacing()
    {
        Bounds bounds = thisCollider.bounds;
        bounds.Expand( skinWidth * -2f );

        horizontalRayCount = Mathf.Clamp( horizontalRayCount , 2 , int.MaxValue );
        verticalRayCount = Mathf.Clamp( verticalRayCount , 2 , int.MaxValue );

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);

    }


}
