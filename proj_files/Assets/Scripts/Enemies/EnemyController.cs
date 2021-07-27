using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public List<Transform> waypoints;
    private Rigidbody2D rb;

    private Transform playerTransform;
    private PlayerManager playerMenagerSC;

    internal int nextWaypointIndex;
    internal float timerOnWaypoint;
    internal int numberOfWaypoints;

    internal int lookDirection;

    public float speed;
    public float radiusOfSlowingDown;
    public float timeToWaitOnWaypoint;

    [Header( "guarding options" )]
    public Light viewRangeLight;
    public float viewDistance;
    private float viewAngle;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMenagerSC = GameObject.FindGameObjectWithTag( "Player" ).GetComponent<PlayerManager>();
        playerTransform = GameObject.FindGameObjectWithTag( "Player" ).transform;
    }


    private void OnEnable()
    {
        viewAngle = viewRangeLight.spotAngle;

        StartCoroutine( FollowPath() );
    }


    private void Update()
    {
        bool isPlayerDetected = IsPlayerVisible();
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
        float angleTOTurn = Mathf.Atan2( (target.position.x - transform.position.x) , (target.position.y - transform.position.y) ) * Mathf.Rad2Deg - 90;
        transform.localRotation = Quaternion.AngleAxis(angleTOTurn,Vector3.up);
    }


    private bool IsPlayerVisible()
    {
        RaycastHit2D lineToPlayer = Physics2D.Linecast( transform.position , playerTransform.position );

        if( lineToPlayer.distance < 0)
        {
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle( transform.right , directionToPlayer );

            if(angleBetweenGuardAndPlayer < viewAngle/2f)
            {

                if(lineToPlayer.collider != null )
                {
                    print( "player detected" );
                    return true;
                }
            }

        }

        return false;
    }

    private void OnCollisionEnter2D( Collision2D collision )
    {
        if( collision.gameObject.CompareTag( "Player" ) && !playerMenagerSC.IsPlayerInvincible() )
        {
            //collision.transform.GetComponent<Rigidbody2D>().AddForce( (collision.transform.position - transform.position).normalized * 10000 , ForceMode2D.Impulse );
            playerMenagerSC.TakeHit(this.gameObject);
        }
    }
}
