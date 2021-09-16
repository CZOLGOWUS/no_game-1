using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using noGame.Collisions;
using noGame.Characters;

public class PlatformController : RaycastController
{
    private struct PassengerMovement
    {
        public Transform transform;
        public Vector3 velocity;
        public bool isOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement( Transform transform , Vector3 velocity , bool isOnPlatform , bool moveBeforePlatform)
        {
            this.transform = transform;
            this.velocity = velocity;
            this.isOnPlatform = isOnPlatform;
            this.moveBeforePlatform = moveBeforePlatform;
        }

    }

    [SerializeField] private bool isCyclic;
    [Range( 0f , 2f )] [SerializeField] private float easeSpeed;
    [SerializeField] private float waitTime;
    private float nextMoveTime;

    [SerializeField] private float speed;
    private int fromWaypointIndex;
    private float percentBetwenWaypoints;

    [SerializeField] private Vector3[] localWaypoints;
    private Vector3[] globalWaypoints;

    private List<PassengerMovement> passengerMovements = new List<PassengerMovement>();

    private Dictionary<Transform , CharacterController2D> passangerDictionary = new Dictionary<Transform , CharacterController2D>();


    [SerializeField] private LayerMask passengerMask;
    private Vector3 velocity;
    private HashSet<Transform> movedPassangers = new HashSet<Transform>();


    public override void Start()
    {
        base.Start();

        globalWaypoints = new Vector3[localWaypoints.Length];
        for(int i = 0 ; i < localWaypoints.Length ;i++ )
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }

    }

    private void Update()
    {


    }

    private void FixedUpdate()
    {
        UpdateRaycastOrigins();

        velocity = CalculatePlatformMovement();

        CalculatePassangersDisplacment( velocity );
        MovePassengers( true );

        transform.Translate( velocity );
        
        MovePassengers( false );

        passengerMovements.Clear();

    }


    private Vector3 CalculatePlatformMovement( )
    {

        if( Time.time < nextMoveTime )
            return Vector3.zero;

        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;

        float distanceBetweenWayPoints = Vector3.Distance( globalWaypoints[fromWaypointIndex] , globalWaypoints[toWaypointIndex] );

        percentBetwenWaypoints = Mathf.Clamp01( percentBetwenWaypoints + Time.fixedDeltaTime * speed / distanceBetweenWayPoints);
        float easedPrecentBetwenWaypoints = Ease( percentBetwenWaypoints );

        Vector3 newPos = Vector3.Lerp( globalWaypoints[fromWaypointIndex] , globalWaypoints[toWaypointIndex] , easedPrecentBetwenWaypoints );

        if(percentBetwenWaypoints >= 1f)
        {
            fromWaypointIndex %= globalWaypoints.Length;
            percentBetwenWaypoints = 0f;
            fromWaypointIndex++;

            if( !isCyclic )
            {
                if( fromWaypointIndex >= globalWaypoints.Length - 1 )
                {
                    fromWaypointIndex = 0;
                    System.Array.Reverse( globalWaypoints );
                }
            }

            nextMoveTime = Time.time + waitTime;

        }

        return newPos - transform.position;

    }


    private void MovePassengers(bool beforeMovePlatform)
    {
        foreach(PassengerMovement passenger in passengerMovements)
        {
            if(!passangerDictionary.ContainsKey(passenger.transform))
            {
                passangerDictionary.Add( passenger.transform , passenger.transform.GetComponent<CharacterController2D>() );
            }

            if(passenger.moveBeforePlatform == beforeMovePlatform)
            {
                passangerDictionary[passenger.transform].Move( passenger.velocity ,passenger.isOnPlatform);
            }
        }
    }


    private void CalculatePassangersDisplacment(Vector3 velocity)
    {


        float directionX = Mathf.Sign( velocity.x );
        float directionY = Mathf.Sign( velocity.y );
        
        //vertical moving handling
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


                if(hit && !(hit.distance <= 0f))
                {
                    if( !movedPassangers.Contains(hit.transform) )
                    {
                        movedPassangers.Add( hit.transform );

                        float pushX = (directionY == 1) ? velocity.x : 0f;
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

                        passengerMovements.Add( new PassengerMovement(
                                                    hit.transform,
                                                    new Vector3(pushX,pushY),
                                                    directionY == 1,
                                                    true
                                                 ));
                    }

                }

            }
            movedPassangers.Clear();
        }

        //horizontal moving handling
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

                if( hit && !(hit.distance <= 0f) )
                {
                    if( !movedPassangers.Contains( hit.transform ) )
                    {
                        movedPassangers.Add( hit.transform );

                        float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
                        float pushY = -skinWidth;

                        passengerMovements.Add( new PassengerMovement(
                                                    hit.transform ,
                                                    new Vector3( pushX , pushY ) ,
                                                    false ,
                                                    true
                                                 ));
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


                if( hit && !(hit.distance <= 0f) )
                {
                    if( !movedPassangers.Contains( hit.transform ) )
                    {
                        movedPassangers.Add( hit.transform );

                        float pushX = velocity.x;
                        float pushY = velocity.y;

                        passengerMovements.Add( new PassengerMovement(
                                                    hit.transform ,
                                                    new Vector3( pushX , pushY ) ,
                                                    true ,
                                                    false
                                                 ));
                    }

                }

            }
            movedPassangers.Clear();
        }


    }


    float Ease(float x)
    {
        float a = easeSpeed + 1f;
        return Mathf.Pow( x , a ) / (Mathf.Pow( x , a ) + Mathf.Pow( 1 - x , a ));
    }


    private void OnDrawGizmos()
    {
        if(localWaypoints != null)
        {
            Gizmos.color = Color.blue;

            Vector3 globalWayPointPosTemp;
            for( int i = 0 ; i < localWaypoints.Length ; i++ )
            {
                globalWayPointPosTemp =(Application.isPlaying)? globalWaypoints[i] : localWaypoints[i] + transform.position;
                Gizmos.DrawLine( globalWayPointPosTemp - Vector3.up * 0.2f , globalWayPointPosTemp + Vector3.up * 0.2f );
                Gizmos.DrawLine( globalWayPointPosTemp - Vector3.left * 0.2f , globalWayPointPosTemp + Vector3.left * 0.2f );
            }

        }
    }

}
