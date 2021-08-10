using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace noGame.Character.MonoBehaviours
{
    [RequireComponent( typeof( Rigidbody2D ) )]
    [RequireComponent( typeof( Collider2D ) )]
    public class CharacterController2D : MonoBehaviour
    {
        private Rigidbody2D thisRigidBody;
        private Collider2D thisCollider;

        private void Start()
        {
            thisRigidBody = GetComponent<Rigidbody2D>();
            thisCollider = GetComponent<Collider2D>();
        }
    }
}
