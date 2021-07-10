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
    public float speed;

    [Header("ground check")]
    public Vector2 sizeOfRayBox;
    public float distanceOfBoxCast;
    public LayerMask TerrainLayerMask;


    [Header("jumping settings")]
    public float jumpForce;
    public float gravityMultiplier;
    private float lowerGravityScaler;
    //to do : make it modifiable in editor and fix it overal(fuked up jumping at the begining)
    public float jumpingLinearDrag = 1.0f;


    
    [HideInInspector]public float movementValue;
    [HideInInspector]public bool jumpBtnValue;
    [HideInInspector]public bool downInputValue;

    private bool isAirborn = false;
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
        
        Jump();
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
            isAirborn = !isAirborn;
            return true;
        }
        else
        {
            isAirborn = !isAirborn;
            return false;
        }
    }
    
    public void MovePlayer()
    {
        float velocityY = rb2d.velocity.y;
        rb2d.velocity = new Vector2(movementValue*speed * (jumpingLinearDrag) , velocityY);
    }

    public void Jump()
    {
        if (jumpBtnValue)
        {
            if (!isAirborn && isGrounded)
            {
                rb2d.AddForce(Vector2.up*jumpForce);
                isAirborn = true;
                isGrounded = false;
                jumpingLinearDrag = 0.2f;
                return;
            }
        }

        if ( isAirborn && !isGrounded)
        {
            if( rb2d.velocity.y < 0.0f )
                rb2d.velocity += Vector2.down * gravityMultiplier * Time.deltaTime;
            else if( rb2d.velocity.y > 0.0f && jumpBtnValue )
                rb2d.velocity += Vector2.up * lowerGravityScaler * Time.deltaTime;
        }
        else
        {
            jumpingLinearDrag = 1.0f;
        }
        
    }
    

    
}
