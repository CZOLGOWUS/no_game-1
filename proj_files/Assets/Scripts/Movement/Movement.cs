using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using noGame.Character.MonoBehaviours;

namespace noGame.MovementBehaviour
{
    [RequireComponent( typeof( CharacterController2D ) )]
    public class Movement : MonoBehaviour
    {

        private CharacterController2D thisCharacterController;
        private Rigidbody2D thisRigidbody;
        private Collider2D thisCollider;

        public float acceleration;
        public float maxSpeed;
        [SerializeField] private float currentSpeed;


        public float CurrentSpeed { get => currentSpeed; }


        private void Start()
        {
            thisRigidbody = thisCharacterController.ThisRigidBody;
            thisCollider = thisCharacterController.ThisCollider;
        }

        public void Move(float direction) // (left, right)
        {
            thisRigidbody.MovePosition( Vector2.right * direction * acceleration);
        }

        public void Jump(Vector2 direction, float height)
        {

        }



    }
}
