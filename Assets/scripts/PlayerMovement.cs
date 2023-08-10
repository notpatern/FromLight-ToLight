using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerMovement : MonoBehaviour
{
    [Header("movement")]
    public float naturalMoveSpeed;
    float moveSpeed;
    public Transform orientation;
    float hInput;
    float vInput;
    public float jumpStrength;
    public float wallJumpStrength;
    public float sideWallJumpStrength;
    public float maxGroundSpeed;
    public float doubleJumpStrength;
    bool canDoubleJump;
    bool hasDoubleJumped;
    bool exitingWall;
    float exitingTimer;
    public float exitingTime;
    bool wallRight;
    bool wallLeft;
    bool wallRunning;
    public float wallRunForce;
    public float wallRunSpeed;
    public float wallDistance;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    public float airMaxSpeed;
    public float airMultiplier;
    bool isClimbing;
    bool wallFront;
    private RaycastHit wallFrontHit;

    [Header("slide movement")]
    public float slideSpeed;
    public float slideJumpStrength;
    bool isSliding;
    bool hasSlid;
    bool isCrouched;
    public float maxSlideSpeed;
    public float crouchHeight;
    public float slideDrag;
    public float slideFOV;
    bool slidy;

    [Header("Sounds")]
    public AudioSource jumpSound;
    public AudioSource stepsSound;
    public AudioSource slideSound;
    public AudioSource wallrunSound;

    [Header("references")]
    public float playerHeight;
    public LayerMask isGround;
    bool grounded;
    public float groundDrag;
    public PlayerCam pc;
    public float maxFov;
    public float maxTilt;
    public LayerMask isWall;
    public Transform Player;


    Vector3 moveDirection;
    Vector3 slideDir;
    float maxSpeed;

    public Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // rb.freezeRotation = true;
        canDoubleJump = false;
        hasDoubleJumped = false;
        wallRunning = false;
        hasSlid = false;
        isCrouched = false;
        isSliding = false;
        slidy = false;
        moveSpeed = naturalMoveSpeed;
        airMultiplier = 1;
    }

// Update is called once per frame
    void Update()
    {
        ChecksGrounded();
        if (exitingWall)
        {
            StopWallRun();
        }

        if (grounded)
        {
            airMultiplier = 1;

            if (isSliding)
            {
                rb.drag = slideDrag;
                maxSpeed = maxSlideSpeed;
            }
            else if (!isSliding) {rb.drag = groundDrag; maxSpeed = maxGroundSpeed;}
            
            canDoubleJump = false;
            hasDoubleJumped = false;
            
        }
        else if (hasDoubleJumped == false && grounded == false)
        {
            rb.drag = 0;
            airMultiplier = 0.5f;
            canDoubleJump = true;
            maxSpeed = airMaxSpeed;
        }
        else
        {
            airMultiplier = 0.5f;
            maxSpeed = airMaxSpeed;
            rb.drag = 0;
        }
        if (wallRunning)
        {
            airMultiplier = 1;
            canDoubleJump = true;
        }
        ExitingWallTimerDecrease();
        PlayerInput();
        State();
        CheckForWall();
        

    }

    private void FixedUpdate()
    {
        MovePlayer();
        ControlPlayerSpeed();
        if (!isSliding && hasSlid)
        {
            SlideMovement(slideDir);
        }
        if (isCrouched)
        {
            CrouchMovement();
        }
        if (wallRunning)
        {
            WallRunningMovement();
        }
        if (isClimbing)
        {
            ClimbingMovement();
        }
    }

    private void State()
    {
        Vector3 currentSpeed = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (grounded)
        {
            maxSpeed = maxGroundSpeed; 
        }
        if (!grounded && (wallLeft || wallRight) && vInput != 0 /* && hInput != 0*/ && !exitingWall)
        {
            if (!wallRunning)
            {
                StartWallRun();
            }
        }
        else
        {
            if (wallRunning)
            {
                StopWallRun();
                canDoubleJump = true;
            }
        }
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl)))
        {
            slidy = true;
            if (grounded && slidy && !hasSlid && !isCrouched && currentSpeed.magnitude > 1.6f)
            {
                StartSlide();
            }
            if (!hasSlid && slidy && currentSpeed.magnitude <= 1.6f && grounded && !isCrouched)
            {
                StartCrouch();
            }
        }
        if ((Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.LeftControl)) || !grounded)
        { 
            StopSlide();
            StopCrouch();
        }
        if (hasSlid && currentSpeed.magnitude <= 1.6f && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl)) && grounded && !isCrouched)
        {
            slideDir = new Vector3(0f, 0f, 0f);
            StopSlide();
            StartCrouch(); 
        }
        if (wallFront && vInput > 0)
        {
            if (!isClimbing)
            {
                StartClimbing();
            }
        }
        else
        {
            if (isClimbing)
            {
                StopClimbing();
            }
        }
    }

    private void StartCrouch()
    {
        isCrouched = true;
        Player.localScale = new Vector3(Player.localScale.x, crouchHeight, Player.localScale.z);
        rb.AddForce(Vector3.down * 0.3f, ForceMode.Impulse);
    }

    private void CrouchMovement()
    {
        moveSpeed = naturalMoveSpeed * 0.4f;
    }

    private void StopCrouch()
    {
        isCrouched = false;
        Player.localScale = new Vector3(Player.localScale.x, 1, Player.localScale.z);
        moveSpeed = naturalMoveSpeed;
    }

    private void StartSlide()
    {
        slideSound.Play();
        hasSlid = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Player.localScale = new Vector3(Player.localScale.x, crouchHeight, Player.localScale.z);
        rb.AddForce(Vector3.down * 0.3f, ForceMode.Impulse);
        slideDir = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        pc.ChangeFOV(slideFOV, 0.25f);
        
    }

    private void SlideMovement(Vector3 slideD)
    {
        rb.AddForce(slideD.normalized * 1.5f * slideSpeed , ForceMode.Impulse) ;
        isSliding = true;
    }

    private void StopSlide()
    {    
        slidy = false;
        isSliding = false ;
        hasSlid = false;
        Player.localScale = new Vector3(Player.localScale.x, 1, Player.localScale.z);
        pc.ChangeFOV(90f, 0.25f) ;
    }


    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallDistance, isWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallDistance, isWall);
        wallFront = Physics.Raycast(transform.position, orientation.forward, out wallFrontHit, wallDistance, isWall);
    }


    private void ChecksGrounded()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, isGround);
    }

    private void StartWallRun()
    {
        wallRunning = true;
        pc.ChangeFOV(maxFov, 0.25f);
        if (wallLeft)
        { pc.ChangeCamTilt(-maxTilt); }
        else if (wallRight)
        { pc.ChangeCamTilt(maxTilt); }
        
    }

    private void StopWallRun()
    {
        wallRunning = false;
        rb.useGravity = true;
        pc.ChangeFOV(90f, 0.25f);
        pc.ChangeCamTilt(0f);
    }

    private void StartClimbing()
    {
        isClimbing = true;
        rb.useGravity = false;
    }

    private void ClimbingMovement()
    {
        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        Vector3 wallUp = wallFrontHit.normal;

        Vector3 wallDirUp = Vector3.Cross(wallUp, transform.forward);

        rb.AddForce(wallDirUp * 10f, ForceMode.Force);
    }

    private void StopClimbing()
    {
        isClimbing = false;
        rb.useGravity = true;
    }

    private void WallRunningMovement()
    {
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude && vInput > 0)
        {
            wallForward = -wallForward;
        }
        else if ((orientation.forward - wallForward).magnitude < (orientation.forward - -wallForward).magnitude && vInput < 0)
        {
            wallForward = -wallForward;
        }

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);
    }

    private void PlayerInput()
    {
            hInput = Input.GetAxisRaw("Horizontal"); 
            vInput = Input.GetAxisRaw("Vertical");
    
        if (grounded && Input.GetKeyDown(KeyCode.Space) && !canDoubleJump)
        {
            Jump(jumpStrength);
        }
        if (canDoubleJump && !grounded && Input.GetKeyDown(KeyCode.Space))
        {
            Jump(doubleJumpStrength);
            hasDoubleJumped = true;
            canDoubleJump = false;
        }
        if (wallRunning && Input.GetKeyDown(KeyCode.Space))
        {
            WallJump();
        }
    }

    private void ExitingWallTimerDecrease()
    {
        if (exitingWall == true)
        {
            if (exitingTimer > 0)
            {
                exitingTimer -= Time.deltaTime;
            }
            if (exitingTimer <= 0)
            {
                exitingWall = false;
            }
        }
    }

    private void Jump(float strength)
    {
        jumpSound.Play();
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 slideForce = new Vector3(0f, 0f, 0f);
        if (isSliding)
        {
            slideForce = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        }
        rb.AddForce(transform.up * strength, ForceMode.Impulse);
        rb.AddForce(slideForce.normalized * slideJumpStrength, ForceMode.Impulse);
        
    }

    private void WallJump()
    {
        exitingWall = true;
        exitingTimer = exitingTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 forceToApply = transform.up * wallJumpStrength + wallNormal * sideWallJumpStrength;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(forceToApply, ForceMode.Impulse);
    }

    private void MovePlayer()
    {
        if (!isSliding || !grounded)
        {
            moveDirection = orientation.forward * vInput + orientation.right * hInput;
            rb.AddForce(moveDirection.normalized * moveSpeed * airMultiplier * 10f, ForceMode.Force);
        }
    }

    private void ControlPlayerSpeed()
    {
        Vector3 horVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (wallRunning)
        {
            maxSpeed = wallRunSpeed;
        }

        if (horVel.magnitude > maxSpeed)
        {
            Vector3 limitedVel = horVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }
}