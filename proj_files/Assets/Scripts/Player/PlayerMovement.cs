using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private BoxCollider2D thisCollider2D;

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

    public float GravityMultiplierThreshold;
    [Range( 1f , 5f )]
    public float lowerGravityScaler;

    [Range( 0.0001f , 1f )]
    public float jumpingLinearDrag;

    public float jumpCooldownLimit;
    [SerializeField]
    private float jumpCooldown;


    private float currentLinearDrag = 1.0f;



    
    [HideInInspector]public float movementValue;
    [HideInInspector]public bool jumpBtnValue;
    [HideInInspector]public bool downInputValue;

    private bool isGrounded;

    public float JumpForce
    {
        get => jumpForce;
        set => jumpForce = value;
    }
    
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        thisCollider2D = GetComponent<BoxCollider2D>();
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
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public void MovePlayer()
    {
        float velocityY = rb2d.velocity.y;
        float velocityX = rb2d.velocity.x;

        if( movementValue != 0 )
            rb2d.velocity =  new Vector2( Mathf.Clamp( velocityX + movementValue * accelerationSpeed * currentLinearDrag * Time.deltaTime , -maxSpeed , maxSpeed ),velocityY);
        else
            rb2d.velocity -= Mathf.Abs( velocityX ) > 0.1f ? Vector2.right * Mathf.Lerp( velocityX , 0.0f , decelerationSpeed ) * currentLinearDrag : Vector2.right * velocityX;


        Jump();

    }

    public void Jump()
    {
        if (jumpBtnValue)
        {
            if ( isGrounded && jumpCooldown <= 0f)
            {
                rb2d.AddForce(Vector2.up*jumpForce*1000f);
                currentLinearDrag = jumpingLinearDrag;
                jumpCooldown = jumpCooldownLimit;
                return;
            }
        }

        if (!isGrounded)
        {
            if( rb2d.velocity.y < GravityMultiplierThreshold )
                rb2d.velocity += Physics2D.gravity * (fallGravityMultiplier - 1) * Time.deltaTime * 10f;
            else if( rb2d.velocity.y > 1.0f && jumpBtnValue )
                rb2d.velocity += -Physics2D.gravity * (lowerGravityScaler - 1) * Time.deltaTime * 10f;
        }
        else
        {
            currentLinearDrag = 1.0f;

        }

        if( isGrounded )
            jumpCooldown -= Time.deltaTime;

    }
    

    
}
