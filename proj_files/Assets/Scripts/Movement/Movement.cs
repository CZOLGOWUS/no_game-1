using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace noGame.MovementBehaviour
{
    [RequireComponent( typeof( Rigidbody2D ) , typeof( Collider2D ) )]
    public class Movement : MonoBehaviour
    {

        private Rigidbody2D thisRigidBody;
        private Collider2D thisCollider;

        private void Start()
        {
            thisRigidBody = GetComponent<Rigidbody2D>();
            thisCollider = GetComponent<Collider2D>();
        }

        private void FixedUpdate()
        {

        }

    }
}
