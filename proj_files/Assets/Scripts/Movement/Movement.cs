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

        private Vector2 currentPositionOffset;
        private Vector2 nextPositionOffset;

        //move-settings
        public float maxSpeed;

        private float currentSpeed;
        private bool isMoving = false;
        private bool isJumping = false;


        public float CurrentSpeed { get => currentSpeed; }
        public Vector2 PrevPosition { get => currentPositionOffset; }
        public Vector2 NextPosition { get => nextPositionOffset; }
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
            print( thisCharacterController.velocity.x );
            thisCharacterController.Move( nextPositionOffset );
            currentPositionOffset = nextPositionOffset;
        }

        public void MoveInDirection(float direction) // (left, right)
        {
            if(direction != 0 && Mathf.Abs(thisCharacterController.velocity.x) <= maxSpeed)
            {
                nextPositionOffset += Vector2.right * direction * maxSpeed * Time.deltaTime; 
            }
            else
            {
                nextPositionOffset.x -= currentPositionOffset.x * 0.1f;
            }
        }

        public void Jump( float height)
        {
            
        }



    }
}
