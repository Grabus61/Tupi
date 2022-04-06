using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour
{
    [SerializeField] private InputController input = null;

    [SerializeField, Range(0f, 10f)] private float jumpHeight = 3f;
    [SerializeField, Range(0, 5)] private int maxAirJumps = 0;
    [SerializeField, Range(0f, 10f)] private float downwardMovementMultiplier = 3f;
    [SerializeField, Range(0f, 10f)] private float upwardMovementMultiplier = 1.7f;

    private Rigidbody2D body;
    private Ground ground;
    private Vector2 velocity;

    private int jumpPhase;
    private float defaultGravityScale;

    private bool desiredJump;
    private bool jumped;
    private bool onGround;

    [Space][Header("Optimizations")]
    //CoyoteJump & Jump buffer
    [SerializeField, Range(0f,0.4f)] private float coyoteTime = 0.1f;
    private float coyoteTimeCounter;

    [SerializeField, Range(0f,0.4f)] private float jumpBufferTime = 0.1f;
    private float jumpBufferCounter;
    

    private void Awake() {
        body = GetComponent<Rigidbody2D>();
        ground = GetComponent<Ground>();

        defaultGravityScale = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        desiredJump |= input.RetrieveJumpInput();

        if(desiredJump)
        {
            jumpBufferCounter = jumpBufferTime;
            desiredJump = false;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
        {
            onGround = ground.GetOnGround();
            velocity = body.velocity;

            if (onGround)
            {
                jumpPhase = 0;
                coyoteTimeCounter = coyoteTime;
            }
            else{
                coyoteTimeCounter -= Time.deltaTime;
            }

            JumpAction();

            if (body.velocity.y > 0)
            {
                body.gravityScale = upwardMovementMultiplier;
            }
            else if (body.velocity.y < 0)
            {
                body.gravityScale = downwardMovementMultiplier;
            }
            else if(body.velocity.y == 0)
            {
                body.gravityScale = defaultGravityScale;
            }

            body.velocity = velocity;
        }

    private void JumpAction()
        {
            if ((coyoteTimeCounter > 0 && jumpBufferCounter > 0) || (jumpPhase < maxAirJumps && jumped))
            {
                //jumped = true;
                jumpPhase += 1;
                jumpBufferCounter = 0;
                coyoteTimeCounter = 0;
                float jumpSpeed = Mathf.Sqrt(-2f * Physics2D.gravity.y * jumpHeight);
                if (velocity.y > 0f)
                {
                    jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
                }
                else if (velocity.y < 0f)
                {
                    jumpSpeed += Mathf.Abs(body.velocity.y);
                }

                velocity.y += jumpSpeed;
                Debug.Log(jumpPhase);
            }
        }
}
