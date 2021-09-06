using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController
{
    public Vector3 move;


    [SerializeField] private LayerMask passengerMask;
    private Vector3 velocity;
    private HashSet<Transform> movedPassangers = new HashSet<Transform>();


    public override void Start()
    {
        base.Start();
    }

    private void Update()
    {


    }

    private void FixedUpdate()
    {
        UpdateRaycastOrigins();

        velocity = move * Time.deltaTime;

        MovePassengers( velocity );
        transform.Translate( velocity );
        
    }

    private void MovePassengers(Vector3 velocity)
    {


        float directionX = Mathf.Sign( velocity.x );
        float directionY = Mathf.Sign( velocity.y );

        if( velocity.y != 0 )
        {
            float raycastLength = Mathf.Abs( velocity.y ) + skinWidth;

            for( int i = 0 ; i < verticalRayCount ; i++ )
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i );

                //syncs transforms before casting the ray for 
                //Physics2D.SyncTransforms();

                RaycastHit2D hit = Physics2D.Raycast( rayOrigin , Vector2.up * directionY , raycastLength , passengerMask );


                if(hit)
                {
                    if( !movedPassangers.Contains(hit.transform) )
                    {
                        movedPassangers.Add( hit.transform );

                        float pushX = (directionY == 1) ? velocity.x : 0f;
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

                        hit.transform.Translate( new Vector3( pushX , pushY ) ); 
                    }

                }

            }
            movedPassangers.Clear();
        }


        if(velocity.x != 0)
        {

            float raycastLength = Mathf.Abs( velocity.x ) + skinWidth;

            for( int i = 0 ; i < horizontalRayCount ; i++ )
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRayCount * i);

                //syncs transforms before casting the ray for 
                //Physics2D.SyncTransforms();

                RaycastHit2D hit = Physics2D.Raycast( rayOrigin , Vector2.right * directionX , raycastLength , passengerMask );

                if( hit )
                {
                    if( !movedPassangers.Contains( hit.transform ) )
                    {
                        movedPassangers.Add( hit.transform );

                        float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
                        float pushY = 0f;

                        hit.transform.Translate( new Vector3( pushX , pushY ) );
                    }

                }

            }

            movedPassangers.Clear();


        }

        if(directionY == -1 || velocity.y == 0f && velocity.x != 0f)
        {
            float raycastLength = skinWidth * 2f;

            for( int i = 0 ; i < verticalRayCount ; i++ )
            {
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);

                //syncs transforms before casting the ray for 
                //Physics2D.SyncTransforms();

                RaycastHit2D hit = Physics2D.Raycast( rayOrigin , Vector2.up , raycastLength , passengerMask );


                if( hit )
                {
                    if( !movedPassangers.Contains( hit.transform ) )
                    {
                        movedPassangers.Add( hit.transform );

                        float pushX = velocity.x;
                        float pushY = velocity.y;

                        hit.transform.Translate( new Vector3( pushX , pushY ) );
                    }

                }

            }
            movedPassangers.Clear();
        }

    }

}
