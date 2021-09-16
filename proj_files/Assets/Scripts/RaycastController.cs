using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace noGame.Collisions
{

    [RequireComponent( typeof( BoxCollider2D ) )]
    public class RaycastController : MonoBehaviour
    {
        protected struct RaycastOrigins
        {
            public Vector2 topLeft, topRight;
            public Vector2 bottomLeft, bottomRight;
        }

        public struct BoxCastOrigins
        {
            public Vector2 bottomCenter, topCenter;
            public Vector2 leftCenter, rightCenter;
        }

        //dependecies
        protected BoxCollider2D thisCollider;

        //struct deleration
        protected RaycastOrigins raycastOrigins;
        protected BoxCastOrigins boxCastOrigins;

        protected const float skinWidth = 0.015f;
        protected Vector3 oldVelocity;

        //bounds size
        protected float boundsWidth;
        protected float boundsHeight;


        //RAYS
        [Space]
        [Header( "Ray Options" )]
        [SerializeField] private float distanceBetwenHorizontalRays = 0.25f;
        [SerializeField] private float distanceBetwenVerticalRays = 0.25f;
        [SerializeField] protected LayerMask collisionMask;

        //ray count
        protected int horizontalRayCount = 4;
        protected int verticalRayCount = 4;

        //ray spasing for calculation
        protected float horizontalRaySpacing;
        protected float verticalRaySpacing;





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


        public void UpdateBoxCastOrigins()
        {
            Bounds bounds = thisCollider.bounds;
            bounds.Expand( skinWidth * -2f );

            boxCastOrigins.bottomCenter = new Vector2( bounds.center.x , bounds.min.y );
            boxCastOrigins.topCenter = new Vector2( bounds.center.x , bounds.max.y );
            boxCastOrigins.leftCenter = new Vector2( bounds.min.x , bounds.center.y );
            boxCastOrigins.rightCenter = new Vector2( bounds.max.x , bounds.center.y );

        }


        protected void CalculateRaySpacing()
        {
            Bounds bounds = thisCollider.bounds;
            bounds.Expand( skinWidth * -2f );

            boundsWidth = bounds.size.x;
            boundsHeight = bounds.size.y;

            horizontalRayCount = Mathf.RoundToInt( boundsHeight / distanceBetwenVerticalRays );
            horizontalRayCount = Mathf.RoundToInt( boundsWidth / distanceBetwenHorizontalRays );


            horizontalRayCount = Mathf.Clamp( horizontalRayCount , 2 , int.MaxValue );
            verticalRayCount = Mathf.Clamp( verticalRayCount , 2 , int.MaxValue );

            horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
            verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);

        }

    }
}
