using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



namespace noGame.Characters
{
    [RequireComponent( typeof( CharacterController2D ) )]
    public class PlayerCharacterController2D : MonoBehaviour
    {
        private CharacterController2D thisCharacterController;

        [Header( "Movement" )]
        [SerializeField] private float moveSpeed = 10f;
        [Range( 0f , 3f )]
        [SerializeField] private float accelerationTimeAirborn = .2f;
        [Range( 0f , 3f )]
        [SerializeField] private float accelerationTimeGrounded = .2f;

        [Header( "Jumping" )]
        [SerializeField] private float jumpHeight = 4f;
        [SerializeField] private float timeToJumpApex = 0.3f;
        [SerializeField] private float fallGravityMultiplier = 2.0f;

        [Header( "WallJumping" )]
        [SerializeField] private float wallSlideSpeedMax = 3f;
        [SerializeField] private float wallStickTime = .2f;
        [Space]
        [SerializeField] private Vector2 wallJumpClimb;
        [SerializeField] private Vector2 wallJumpOff;
        [SerializeField] private Vector2 wallJumpAway;

        [Header( "SLopes" )]
        [SerializeField] private Vector2 slidingJump;

        private float timeToWallUnstick;

        private int wallDirX;
        private bool isWallSlliding;

        private float velocityXSmooth;

        private float gravity;
        private float initialJumpVelocity = 10f;
        private bool isJumping = false;

        private float movementInput;
        [SerializeField] private bool isJumpPressed = false; //button
        private bool isDownKeyPressed = false; //button

        private Vector2 currentVelocity;
        private Vector2 appliedVelocity;

        private bool isFalling;
        private float maxCharacterPositionOffset = 100f;

        //for jump to work as intended for now
        [SerializeField] private Vector2 autoMove;

        private void Awake()
        {
            thisCharacterController = GetComponent<CharacterController2D>();

            SetupJumpVariales();

        }


        private void FixedUpdate()
        {

            //this is here for walljumping to work, i know... dumb implementation
            HandleInputSmoothing();

            HandleWallJumping();


            HandleGravity();
            HandleJumping( isWallSlliding , thisCharacterController.collisions.slidingDownSlope , wallDirX );

            appliedVelocity.x = currentVelocity.x;
            appliedVelocity.y = currentVelocity.y;

            thisCharacterController.Move( (autoMove + appliedVelocity) * Time.fixedDeltaTime );

            HandleTopBottomCollisionsWhileAirborn();

        }

        private void SetupJumpVariales()
        {
            gravity = -(2 * jumpHeight) / Mathf.Pow( timeToJumpApex , 2 );
            initialJumpVelocity = (2 * jumpHeight) / (timeToJumpApex);
            //initialJumpVelocity = Mathf.Abs( gravity * timeToJumpApex );

            print( "Gravity: " + gravity + " jump vel: " + initialJumpVelocity );
        }


        private void HandleTopBottomCollisionsWhileAirborn()
        {
            if( thisCharacterController.collisions.top || thisCharacterController.collisions.bottom )
            {
                if( thisCharacterController.collisions.slidingDownSlope )
                {
                    ApplyVelocity( thisCharacterController.collisions.slopeNormal.y * -gravity * Time.deltaTime  * Vector2.up );
                }
                else
                    currentVelocity.y = 0f;
            }
        }

        private void HandleWallJumping()
        {
            if( thisCharacterController.collisions.left )
                wallDirX = -1;
            else if( thisCharacterController.collisions.right )
                wallDirX = 1;

            isWallSlliding = false;

            if( (thisCharacterController.collisions.left || thisCharacterController.collisions.right) && !thisCharacterController.collisions.bottom && currentVelocity.y < 0f )
            {
                isWallSlliding = true;

                if( currentVelocity.y < -wallSlideSpeedMax )
                {
                    currentVelocity.y = -wallSlideSpeedMax;
                }

                if( timeToWallUnstick > 0f )
                {
                    //this should not be happening here
                    velocityXSmooth = 0f;

                    currentVelocity.x = 0f;

                    if( movementInput != wallDirX && movementInput != 0f )
                        timeToWallUnstick -= Time.deltaTime;
                    else
                        timeToWallUnstick = wallStickTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }


            }
        }

        private void HandleInputSmoothing()
        {
            float targetVelocityX = movementInput * moveSpeed;
            float accelerationTime = (thisCharacterController.collisions.bottom) ? accelerationTimeGrounded : accelerationTimeAirborn;

            //smooth input, if collides with a wall (aka head on collision) then kill momentum
            if( thisCharacterController.collisions.slopeAngle <= thisCharacterController.MaxClimbAngle )
            {
                currentVelocity.x = Mathf.SmoothDamp( currentVelocity.x , targetVelocityX , ref velocityXSmooth , accelerationTime );
            }
            else
            {
                currentVelocity.x = Mathf.SmoothDamp( 0f , targetVelocityX , ref velocityXSmooth , accelerationTime );
            }
        }


        private void HandleGravity()
        {
            isFalling = currentVelocity.y <= 0f;

            //this implementation causes wall jumping to be a bit undesirable
            if( !isFalling && isJumpPressed )
            {
                ApplyVelocity( gravity * Time.deltaTime * Vector2.up );
            }
            else if(Mathf.Abs(currentVelocity.y) < maxCharacterPositionOffset )
            {
                ApplyVelocity( gravity * fallGravityMultiplier * Time.deltaTime * Vector2.up );
            }
            else
            {
                currentVelocity.y = Mathf.Clamp( currentVelocity.y , -maxCharacterPositionOffset , maxCharacterPositionOffset );
            }

        }


        private void HandleJumping( bool isWallSliding , bool isOnTooSteepSlope , int wallDirX )
        {
            if( !isJumping && isJumpPressed )
            {
                if( isWallSliding )
                {
                    isJumping = true;

                    //TODO: implement Verlet velocity calculation
                    if( wallDirX == movementInput )
                    {
                        ApplyVelocity( new Vector2( -wallDirX * wallJumpClimb.x, wallJumpClimb.y) );
                    }
                    else if( wallDirX == 0 )
                    {
                        ApplyVelocity( new Vector2( -wallDirX * wallJumpOff.x , wallJumpOff.y ) );
                    }
                    else //if input is oposite to the wall (jump away)
                    {
                        ApplyVelocity( new Vector2( -wallDirX * wallJumpAway.x , wallJumpAway.y ) );
                    }

                }

                if( isOnTooSteepSlope )
                {
                    //Add jumping similar to wall jumping or regular jumping
                }


                if( thisCharacterController.collisions.bottom )
                {
                    if( thisCharacterController.collisions.slidingDownSlope )
                    {
                        if( Mathf.Sign( movementInput ) == -wallDirX ) //not jumping againts max slope (or you should jump, since we have wall jumping?)
                        {
                            ApplyVelocity( new Vector2(
                                slidingJump.x * thisCharacterController.collisions.slopeNormal.x ,
                                slidingJump.y * thisCharacterController.collisions.slopeNormal.y ) );
                        }
                    }
                    else
                    {
                        ApplyYVelocity( initialJumpVelocity );

                        isJumping = true;
                    }
                }

            }
            else if( (isJumping && !isJumpPressed && thisCharacterController.collisions.bottom) || isWallSliding )
            {
                isJumping = false;
            }
        }


        public void OnMovement( InputAction.CallbackContext ctx )
        {
            movementInput = ctx.ReadValue<float>();
        }


        public void OnJump( InputAction.CallbackContext ctx )
        {
            if( ctx.performed || ctx.started )
            {
                isJumpPressed = true;
            }
            else if( ctx.canceled )
            {
                isJumpPressed = false;
            }
        }

        public void OnDownKey( InputAction.CallbackContext ctx )
        {
            if( ctx.performed )
            {
                isDownKeyPressed = true;

                CheckIfStandingOnPhasePlatform();

            }
            else if( ctx.canceled )
            {
                isDownKeyPressed = false;
            }
        }

        private void CheckIfStandingOnPhasePlatform()
        {
            if( thisCharacterController.collisions.bottom )
            {
                RaycastHit2D hitLeft = Physics2D.Raycast( thisCharacterController.raycastOrigin.bottomLeft + Vector2.down * thisCharacterController.SkinWidth * 2f , Vector2.down );
                RaycastHit2D hitRight = Physics2D.Raycast( thisCharacterController.raycastOrigin.bottomRight + Vector2.down * thisCharacterController.SkinWidth * 2f , Vector2.down );

                if( hitLeft && hitLeft.collider.CompareTag( thisCharacterController.PlatformTag )  )
                {
                    thisCharacterController.PhaseThroughtPlatform( hitLeft.collider );
                }
                else if( hitRight && hitRight.collider.CompareTag( thisCharacterController.PlatformTag ) )
                {
                    thisCharacterController.PhaseThroughtPlatform( hitRight.collider );
                }
            }
        }



        /// <summary>
        /// Apply force(Velocity) to this body on this frame using Verlet Integration
        /// </summary>
        public void ApplyVelocity( Vector2 force )
        {
            Vector2 prevVelocity = currentVelocity;
            currentVelocity = currentVelocity + force;
            appliedVelocity = (prevVelocity + currentVelocity) * 0.5f;

        }

        public void ApplyYVelocity( float force )
        {
            float prevYVelocity = currentVelocity.y;
            currentVelocity.y = currentVelocity.y + force;
            appliedVelocity.y = (prevYVelocity + currentVelocity.y) * 0.5f;

        }

        public void ApplyXVelocity( float force )
        {
            float prevXVelocity = currentVelocity.x;
            currentVelocity.x = currentVelocity.x + force;
            appliedVelocity.x = (prevXVelocity + currentVelocity.x) * 0.5f;

        }

    }
}
