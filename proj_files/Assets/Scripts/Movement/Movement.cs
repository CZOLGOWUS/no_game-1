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

        [Header ("Movement Settings")]
        [Tooltip ("Maximal speed at which the character can move sideways")]
        public float maxSpeed;
        [Tooltip("Height of jump")]
        public float jumpHeight;
        [Tooltip("Rate at which the character stop siedeways movement")]
        [Range(0f,1f)]public float stoppingTimeScale;
        [Tooltip("Force of gravity")]
        public float gravity;

        private float currentSpeed;
        private bool isMoving = false;
        private bool isJumping = false;
        private bool isGrounded = false;
        public float CurrentSpeed { get => currentSpeed; }
        public Vector2 PrevPosition { get => currentPositionOffset; }
        public Vector2 NextPosition { get => nextPositionOffset; }
        public bool IsMoving { get => isMoving; set => isMoving = value; }
        public bool IsJumping { get => isJumping; set => isJumping = value; }

        [Header("Ground Detection")]
        [Tooltip("Transform of the base of the character")]
        [SerializeField] private Transform groundDetector;
        [Tooltip("Length of the ray detecting ground")]
        public float rayLength;

        private int groundMask = 1 << 3; //this will make raycast collide only with terrain (which is layer number 3)
        

        private void Start()
        {
            thisCharacterController = GetComponent<CharacterController>();
            //thisCharacterController.Move( new Vector3(1f,0,0) );
        }

        private void Update()
        {
            nextPositionOffset.y -= gravity * Time.deltaTime;
            


            thisCharacterController.Move( nextPositionOffset * Time.deltaTime );
            //transform.position += new Vector3(nextPositionOffset.x,0f,nextPositionOffset.y) * maxSpeed * Time.deltaTime;
            currentPositionOffset = nextPositionOffset;
            if (!isGrounded && Physics.Raycast(groundDetector.position, Vector3.down, rayLength, groundMask))
            {
                Debug.Log("RAYCAST HIT");
                if (nextPositionOffset.y < 0) nextPositionOffset.y = 0.0f;
                isGrounded = true;
            }
        }

        public void MoveInDirection(float direction) // (left, right)
        {
            if( direction != 0 )
            {
                isMoving = true;
                //nextPositionOffset = Vector2.right * direction * maxSpeed; // resetuje prêdkoœc spadania
                nextPositionOffset.x = direction * maxSpeed;
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

        public void Jump(/*float height*/)
        {
            if(isGrounded)
            {
                //nextPositionOffset.y += height;
                float jumpInitialVelocity = Mathf.Sqrt(2 * gravity * jumpHeight);
                nextPositionOffset.y = jumpInitialVelocity;
                isGrounded = false;
            }
        }
    }
}
