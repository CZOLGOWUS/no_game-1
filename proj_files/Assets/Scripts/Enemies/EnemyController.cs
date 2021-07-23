using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    private void OnCollisionEnter2D( Collision2D collision )
    {
        if( collision.gameObject.CompareTag( "Player" ) )
        {
            collision.transform.GetComponent<Rigidbody2D>().AddForce( (collision.transform.position - transform.position).normalized * 10000 , ForceMode2D.Impulse );
        }
    }
}
