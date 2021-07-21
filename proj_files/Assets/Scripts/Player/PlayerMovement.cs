using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private BoxCollider2D col2D;
    private PlayerInput playerAction;


    [Header( "moving settings" )]
    public float maxSpeed;
    public float accelerationSpeed;
    [Range( 1.0f , 0.0f )] public float decelerationSpeed;


    [Header( "ground check" )] public Vector2 sizeOfRayBox;
    public float distanceOfBoxCast;
    public LayerMask TerrainLayerMask;


    [Header( "jumping settings" )]
    public float jumpForce;
    [Range( 1f , 5f )] public float fallGravityMultiplier;
    [Range( 0.7f , 1.1f )] public float jumpingGravityScaler;
    public float GravityMultiplierThreshold;
    [Range( 0.0001f , 1f )] public float jumpingLinearDrag;

    public float jumpCooldownLimit;
    [SerializeField]
    private float jumpCooldown;
    private bool isJumping;
    private bool isJumpBtnHeldInAir;

    private float currentLinearDrag = 1.0f;




    private float movementValue;
    private bool jumpBtnValue;
    private bool downInputValue;

    private bool isGrounded;

    public bool DownInputValue { get => downInputValue; set => downInputValue = value; }


    private void Awake()
    {
        playerAction = GetComponent<PlayerInput>();
        rb2d = GetComponent<Rigidbody2D>();
        col2D = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        playerAction.actions["walk"].performed += value => movementValue = value.ReadValue<float>();

        playerAction.actions["jump"].performed += Jump;
        playerAction.actions["jump"].canceled += _ => jumpBtnValue = false;

        playerAction.actions["down"].performed += _ => { } ;

    }

    private void Update()
    {
        isGrounded = checkIfGrounded();
        MovePlayer();
        FallingImplematation();

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube( transform.position + Vector3.down * distanceOfBoxCast , sizeOfRayBox );
    }

    private bool checkIfGrounded()
    {
        if( Physics2D.BoxCast( transform.position , sizeOfRayBox , 0.0f , Vector2.down , distanceOfBoxCast , TerrainLayerMask ) && Mathf.Abs( rb2d.velocity.y ) <= 0.01f )
        {
            currentLinearDrag = 1.0f;

            if( isJumpBtnHeldInAir && !jumpBtnValue )
                isJumpBtnHeldInAir = false;

            return true;
        }
        else
        {
            currentLinearDrag = jumpingLinearDrag;
            isJumpBtnHeldInAir = jumpBtnValue;
            return false;
        }
    }

    public void MovePlayer()
    {
        float velocityY = rb2d.velocity.y;
        float velocityX = rb2d.velocity.x;

        if( movementValue != 0 )
        {
            rb2d.velocity = new Vector2( Mathf.Clamp( velocityX + currentLinearDrag * movementValue * accelerationSpeed * Time.deltaTime , -maxSpeed , maxSpeed ) , velocityY );
            RotatePlayer();
        }
        else
            rb2d.velocity -= Mathf.Abs( velocityX ) > 0.1f ? Vector2.right * Mathf.Lerp( velocityX , 0.0f , decelerationSpeed ) * currentLinearDrag : Vector2.right * velocityX;
    }

    public void Jump( InputAction.CallbackContext value )
    {

        jumpBtnValue = value.performed;

        if( jumpBtnValue && isGrounded && jumpCooldown <= 0f && !isJumping && !isJumpBtnHeldInAir )
        {

            if( movementValue == 0.0f )
                rb2d.AddForce( Vector2.up * jumpForce , ForceMode2D.Impulse );
            else
                rb2d.AddForce( new Vector2( movementValue , jumpForce ).normalized * jumpForce , ForceMode2D.Impulse );

            currentLinearDrag = jumpingLinearDrag;
            jumpCooldown = jumpCooldownLimit;
            isJumping = true;

        }

    }

    private void FallingImplematation()
    {
        if( !isGrounded )
        {
            if( rb2d.velocity.y < GravityMultiplierThreshold )
                rb2d.velocity += Physics2D.gravity * (fallGravityMultiplier - 1) * Time.deltaTime * 10f;
            else if( rb2d.velocity.y > 1.0f && jumpBtnValue )
                rb2d.velocity += Physics2D.gravity * (jumpingGravityScaler - 1) * Time.deltaTime * 10f;
        }

        if( isGrounded )
        {
            isJumping = false;
        }

        if( isGrounded && jumpCooldown >= 0f && !isJumping )
        {
            jumpCooldown -= Time.deltaTime;
        }
    }

    public void RotatePlayer() //might be a problem later - todo : make it not invasive
    {
        if( rb2d.velocity.x < 0f )
            transform.eulerAngles = Vector3.up * 180f;
        else if( rb2d.velocity.x > 0f )
            transform.eulerAngles = Vector3.up * 0f;
    }

}
