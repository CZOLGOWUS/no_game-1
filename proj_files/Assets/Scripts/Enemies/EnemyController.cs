using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public List<Transform> waypoints;
    private Rigidbody2D rb;

    internal int nextWaypointIndex;
    internal float timerOnWaypoint;
    internal int numberOfWaypoints;

    internal int lookDirection;

    public float speed;
    public float radiusOfSlowingDown;
    public float timeToWaitOnWaypoint;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

    }

    private void OnEnable()
    {
        StartCoroutine( FollowPath() );
    }

    private void Update()
    {
        
    }

    private IEnumerator FollowPath()
    {
        transform.position = waypoints[0].position;
        nextWaypointIndex = 1;


        while(true)
        {
            transform.position = Vector3.MoveTowards( transform.position , waypoints[nextWaypointIndex].position , speed * Time.deltaTime );

            if(transform.position == waypoints[nextWaypointIndex].position)
            {
                nextWaypointIndex = (++nextWaypointIndex) % waypoints.Count;

                yield return new WaitForSeconds(timeToWaitOnWaypoint);

                TurnTo( waypoints[nextWaypointIndex] );
            }

            yield return null;

        }
    }


    private void TurnTo(Transform target)
    {
        if( target.position.x - transform.position.x > 0f )
            transform.eulerAngles = new Vector3( transform.eulerAngles.x , 0f , transform.eulerAngles.z );
        else
            transform.eulerAngles = new Vector3( transform.eulerAngles.x , -180f , transform.eulerAngles.z );

    }

    private void OnTriggerEnter2D( Collider2D collision )
    {
        if( collision.gameObject.CompareTag( "Player" ) )
        {
            collision.transform.GetComponent<Rigidbody2D>().AddForce( (collision.transform.position - transform.position).normalized * 10000 , ForceMode2D.Impulse );
        }
    }
}
