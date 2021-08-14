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
        [Range(0f,1f)]public float stoppingTimeScale;

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
            //thisCharacterController.Move( new Vector3(1f,0,0) );
        }

        private void Update()
        {

            thisCharacterController.Move( nextPositionOffset * Time.deltaTime );
            //transform.position += new Vector3(nextPositionOffset.x,0f,nextPositionOffset.y) * maxSpeed * Time.deltaTime;
            currentPositionOffset = nextPositionOffset;

        }

        public void MoveInDirection(float direction) // (left, right)
        {
            if( direction != 0 )
            {
                isMoving = true;
                nextPositionOffset = Vector2.right * direction * maxSpeed; 
            }
            else
            {
                isMoving = false;
                nextPositionOffset.x -= currentPositionOffset.x * (1f - stoppingTimeScale);
            }
        }

        public void HandleRotation(float direction)
        {
            if( isMoving )
            {

                transform.LookAt( transform.forward * direction );

            }
        }
        public void Jump( float height)
        {
             
        }
            



    }
}
