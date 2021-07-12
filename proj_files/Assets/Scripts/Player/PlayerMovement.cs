using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private BoxCollider2D thisCollider2D;
    private Animator animator;

    [Header("moving")]
    public float maxSpeed;
    public float accelerationSpeed;
    [Range(1.0f,0.0f)]
    public float decelerationSpeed;

    [Header("ground check")]
    public Vector2 sizeOfRayBox;
    public float distanceOfBoxCast;
    public LayerMask TerrainLayerMask;


    [Header("jumping settings")]
    public float jumpForce;
    [Range( 1f , 5f )]
    public float fallGravityMultiplier;
    [Range( 0.9f , 1.1f )]
    public float jumpingGravityScaler;

    public float GravityMultiplierThreshold;

    [Range( 0.0001f , 1f )]
    public float jumpingLinearDrag;

    public float jumpCooldownLimit;
    [SerializeField]


    private float jumpCooldown;
    private bool isJumping;

    private float currentLinearDrag = 1.0f;



    
    [HideInInspector] private float movementValue;
    [HideInInspector] private bool jumpBtnValue;
    [HideInInspector] private bool downInputValue;

    private bool isGrounded;

    public float MovementValue { get => movementValue; set => movementValue = value; }
    public bool JumpBtnValue { get => jumpBtnValue; set => jumpBtnValue = value; }
    public bool DownInputValue { get => downInputValue; set => downInputValue = value; }

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        thisCollider2D = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        
        MovePlayer();

    }

    private void FixedUpdate()
    {
        isGrounded = checkIfGrounded();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube( transform.position + Vector3.down * distanceOfBoxCast , sizeOfRayBox );
    }

    private bool checkIfGrounded()
    {
        if( Physics2D.BoxCast( transform.position , sizeOfRayBox , 0.0f , Vector2.down , distanceOfBoxCast , TerrainLayerMask ) )
        {
            currentLinearDrag = 1.0f;
            return true;

        }
        else
        {
            currentLinearDrag = jumpingLinearDrag;
            return false;
        }
    }
    
    public void MovePlayer()
    {
        float velocityY = rb2d.velocity.y;
        float velocityX = rb2d.velocity.x;


        if( MovementValue != 0 )
            rb2d.velocity =  new Vector2( Mathf.Clamp( velocityX + currentLinearDrag * MovementValue * accelerationSpeed * Time.deltaTime , -maxSpeed , maxSpeed ) , velocityY);
        else
            rb2d.velocity -= Mathf.Abs( velocityX ) > 0.1f ? Vector2.right * Mathf.Lerp( velocityX , 0.0f , decelerationSpeed ) * currentLinearDrag : Vector2.right * velocityX;


        Jump();

    }

    public void Jump()
    {
        if (jumpBtnValue)
        {
            if ( isGrounded && jumpCooldown <= 0f && !isJumping)
            {
                animator.SetBool( "isJumping" , true );
                if(movementValue == 0.0f)
                    rb2d.AddForce(Vector2.up*jumpForce*1000f);
                else
                    rb2d.AddForce(new Vector2(MovementValue,jumpForce).normalized * jumpForce * 1000f );

                currentLinearDrag = jumpingLinearDrag;
                jumpCooldown = jumpCooldownLimit;
                isJumping = true;
                
            }
        }

        if (!isGrounded)
        {
            if( rb2d.velocity.y < GravityMultiplierThreshold )
                rb2d.velocity += Physics2D.gravity * (fallGravityMultiplier - 1) * Time.deltaTime * 10f;
            else if( rb2d.velocity.y > 1.0f && jumpBtnValue )
                rb2d.velocity += Physics2D.gravity * (jumpingGravityScaler - 1) * Time.deltaTime * 10f;
        }
        else
        {
            isJumping = false;

        }

        if( isGrounded && jumpCooldown >= 0f && !isJumping)
            jumpCooldown -= Time.deltaTime;

    }
    

    
}
