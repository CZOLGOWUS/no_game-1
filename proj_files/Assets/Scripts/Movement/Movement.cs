using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using noGame.Character.MonoBehaviours;

namespace noGame.MovementBehaviour
{
    [RequireComponent(typeof(CharacterController))]
    public class Movement : MonoBehaviour
    {

        //dependecies
        private CharacterController thisCharacterController;

        private Vector2 currentPosition;
        private Vector2 nextPosition;

        //move-settings
        public float maxSpeed;

        private float currentSpeed;
        private bool isMoving = false;
        private bool isJumping = false;


        public float CurrentSpeed { get => currentSpeed; }
        public Vector2 PrevPosition { get => currentPosition; }
        public Vector2 NextPosition { get => nextPosition; }
        public bool IsMoving { get => isMoving; set => isMoving = value; }
        public bool IsJumping { get => isJumping; set => isJumping = value; }

        private void Start()
        {
            thisCharacterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            
        }

        private void FixedUpdate()
        {
            thisCharacterController.Move( nextPosition );
            currentPosition = nextPosition;
        }

        public void Move(float direction) // (left, right)
        {
            if(direction != 0)
            {
                nextPosition += Vector2.right * direction * maxSpeed * Time.deltaTime; 
            }
            else
            {
                nextPosition.x -= currentPosition.x;
            }
        }

        public void Jump( float height)
        {
            
        }



    }
}
