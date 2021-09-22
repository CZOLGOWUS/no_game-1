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
        [Tooltip("X : force added to velocity on X axis ; Y : height of wall jump")]
        [SerializeField] private Vector2 wallJumpClimb;
        [Tooltip("X : force added to velocity on X axis ; Y : height of wall jump")]
        [SerializeField] private Vector2 wallJumpOff;
        [Tooltip("X : force added to velocity on X axis ; Y : height of wall jump")]
        [SerializeField] private Vector2 wallJumpAway;

        [Header( "SLopes" )]
        [SerializeField] private Vector2 slidingJump;

        private float wallJumpClimbInitialYVelocity; 
        private float wallJumpOffInitialYVelocity; 
        private float wallJumpAwayInitialYVelocity;
        private float slopeSlidingJumpYVelocity;


        private float timeToWallUnstick;

        private int wallDirX;
        private bool isWallSlliding;

        private float velocityXSmooth;

        private float gravity;
        private float initialJumpVelocity = 10f;
        private bool isJumping = false;

        private float movementInput;
        private bool isDownKeyPressed;
        [SerializeField] private bool isJumpPressed = false; //button

        private Vector2 currentVelocity;
        private Vector2 nextVelocity;

        private bool isFalling;
        private float maxCharacterPositionOffset = 100f;

        //for jump to work as intended for now
        [SerializeField] private Vector2 autoMove;


        float countTime;


        private void Awake()
        {
            thisCharacterController = GetComponent<CharacterController2D>();

            SetupJumpVariales();

        }

        private void FixedUpdate()
        {
            movementInput = Mathf.Clamp(movementInput + autoMove.x,-1f,1f);

            HandleInputSmoothing();


            HandleWallJumping();

            HandleJumping( isWallSlliding ,wallDirX );

            //methods using verlet integration
            #region setup
            nextVelocity.x = currentVelocity.x;
            nextVelocity.y = currentVelocity.y;
            #endregion
            HandleGravity();

            thisCharacterController.Move( nextVelocity * Time.deltaTime );



            HandleVerticalImpactVelocity();

        }

        private void SetupJumpVariales()
        {
            gravity = -(2 * jumpHeight) / Mathf.Pow( timeToJumpApex , 2 );
            initialJumpVelocity = ((2 * jumpHeight) / (timeToJumpApex));

            print( "Gravity: " + gravity + " jump vel: " + initialJumpVelocity );

            //climb
            float timeToApex = Mathf.Sqrt( (-2f * wallJumpClimb.y) / gravity );
            wallJumpClimbInitialYVelocity = (2f * wallJumpClimb.y) / timeToApex + wallSlideSpeedMax;

            //away
            timeToApex = Mathf.Sqrt( (-2f * wallJumpAway.y) / gravity );
            wallJumpAwayInitialYVelocity = (2f * wallJumpAway.y) / timeToApex + wallSlideSpeedMax;

            //off
            timeToApex = Mathf.Sqrt( (-2f * wallJumpOff.y) / gravity );
            wallJumpOffInitialYVelocity = (2f * wallJumpOff.y) / timeToApex + wallSlideSpeedMax;
            
            //slide Jump
            timeToApex = Mathf.Sqrt( (-2f * wallJumpOff.y) / gravity );
            wallJumpOffInitialYVelocity = (2f * wallJumpOff.y) / timeToApex + wallSlideSpeedMax;

        }


        private void HandleWallJumping()
        {
            if( thisCharacterController.collisions.left )
                wallDirX = -1;
            else if( thisCharacterController.collisions.right )
                wallDirX = 1;

            isWallSlliding = false;

            if( (thisCharacterController.collisions.left || thisCharacterController.collisions.right) && !thisCharacterController.isGrounded && currentVelocity.y < 0f )
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
            float accelerationTime = (thisCharacterController.isGrounded) ? accelerationTimeGrounded : accelerationTimeAirborn;

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

            if( !isFalling && isJumpPressed )
            {
                ApplyYVelocityVerlet( gravity * Time.deltaTime );
            }
            else if( Mathf.Abs(currentVelocity.y) < maxCharacterPositionOffset )
            {
                ApplyYVelocityVerlet( (isJumpPressed ? 1f : fallGravityMultiplier ) * gravity * Time.deltaTime );
            }
            else
            {
                currentVelocity.y = Mathf.Clamp( currentVelocity.y , -maxCharacterPositionOffset , maxCharacterPositionOffset );
            }

        }


        private void HandleJumping( bool isWallSliding , int wallDirX )
        {
            if( !isJumping && isJumpPressed )
            {
                if( isWallSliding )
                {
                    isJumping = true;

                    if( wallDirX == movementInput ) //jump climb
                    {
                        AddForce( ref currentVelocity  , -wallDirX * wallJumpClimb.x , wallJumpClimbInitialYVelocity );
                    }
                    else if( wallDirX == 0 ) //jump away
                    {
                        AddForce( ref currentVelocity , -wallDirX * wallJumpAway.x , wallJumpAwayInitialYVelocity );
                    }
                    else //if input is oposite to the wall (jump off)
                    {
                        AddForce( ref currentVelocity , -wallDirX * wallJumpOff.x , wallJumpOffInitialYVelocity );
                    }

                }

                if( thisCharacterController.collisions.isSlidingDownSlope )
                {
                    //Add jumping similar to wall jumping or regular jumping
                }


                if( thisCharacterController.isGrounded )
                {

                    if( thisCharacterController.collisions.isSlidingDownSlope )
                    {
                        if( Mathf.Sign( movementInput ) == -wallDirX ) //not jumping againts max slope (or you should jump, since we have wall jumping?)
                        {
                            Vector2 scaler = Vector2.Scale( slidingJump , thisCharacterController.collisions.slopeNormal );

                            AddForce(ref currentVelocity, scaler );
                        }
                    }
                    else
                    {
                        currentVelocity.y = initialJumpVelocity;

                        isJumping = true;
                    }
                }

            }
            else if( (isJumping && !isJumpPressed && thisCharacterController.isGrounded) || isWallSliding )
            {
                isJumping = false;
            }
        }


        private void HandleVerticalImpactVelocity()
        {
            if( thisCharacterController.collisions.top || thisCharacterController.isGrounded )
            {
                if( thisCharacterController.collisions.isSlidingDownSlope )
                {
                    AddYForce( ref currentVelocity , thisCharacterController.collisions.slopeNormal.y * -gravity * Time.deltaTime );
                }
                else
                {
                    currentVelocity.y = 0f;
                }
            }
        }


        private void CheckIfStandingOnPhasePlatform()
        {
            if( thisCharacterController.isGrounded )
            {
                RaycastHit2D hitLeft = Physics2D.Raycast( thisCharacterController.raycastOrigin.bottomLeft + Vector2.down * thisCharacterController.SkinWidth * 2f , Vector2.down );
                RaycastHit2D hitRight = Physics2D.Raycast( thisCharacterController.raycastOrigin.bottomRight + Vector2.down * thisCharacterController.SkinWidth * 2f , Vector2.down );

                if( hitLeft && hitLeft.collider.CompareTag( thisCharacterController.PlatformTag ) )
                {
                    thisCharacterController.PhaseThroughtPlatform( hitLeft.collider );
                }
                else if( hitRight && hitRight.collider.CompareTag( thisCharacterController.PlatformTag ) )
                {
                    thisCharacterController.PhaseThroughtPlatform( hitRight.collider );
                }
            }
        }


        // public methods
        public void AddForce( ref Vector2 velocity , Vector2 force )
        {
            velocity.x += force.x;
            velocity.y += force.y;
        }


        public void AddForce( ref Vector2 velocity , float forceX , float forceY )
        {
            velocity.x += forceX;
            velocity.y += forceY;
        }


        public void AddYForce( ref Vector2 velocity , float force )
        {
            velocity.y += force;
        }


        public void AddXForce( ref Vector2 velocity , float force )
        {
            velocity.x += force;
        }


        /// <summary>
        /// Apply force(Velocity on Y axis) to this body on this frame using Verlet Integration
        /// </summary>
        public void ApplyYVelocityVerlet( float force )
        {
            float prevYVelocity = currentVelocity.y;
            currentVelocity.y = currentVelocity.y + force;
            nextVelocity.y = (prevYVelocity + currentVelocity.y) * 0.5f;
        }


        //input events
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




    }
}
