using UnityEngine;
using System.Collections;


public class PlayerMovement : MonoBehaviour
{
    private float moveSpeed;
    private float maxSpeed;
    
    [Header("Movement")]
    public float walkSpeed;
    public float sprintSpeed;
    public float maxWalkSpeed;
    public float maxSprintSpeed;


    [Header("Crouching")]
    public float crouchYScale;
    public float startYScale;
    public float crouchSpeed;
    public float maxCrouchSpeed;

    [Header("Sliding")]
    public float maxSlideSpeed;
    public float slideThreshold;
    public float slideSpeed;
    public float slidePush;
    public bool isSliding = false;

    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;


    public float groundDrag;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool isGrounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Vaulting")]
    public float vaultDuration = 0.2f;
    public float vaultDetectionLength = 1f;
    public float vaultCooldown = 0.5f;
    private bool readyToVault = true;
    public bool isVaulting = false;

    public Transform orientation;

    float hInput, vInput;
    

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;

    public enum MovementState
    {
        walking,
        sprinting,
        air,
        crouching,
        sliding
    }

    
    private IPlayerInputProvider inputProvider;
    private bool isCrouchHeld;
    private bool isSprintHeld;

    
    private bool wasJumping;
    private bool wasCrouching;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = transform.localScale.y;

        inputProvider = GetComponentInParent<IPlayerInputProvider>();


    }

    void Update()
    {
        if (isVaulting) return;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        if (state == MovementState.walking || state == MovementState.crouching || state == MovementState.sprinting)
            rb.linearDamping = groundDrag;
        else if (state == MovementState.sliding)
            rb.linearDamping = 1;
        else
            rb.linearDamping = 0;

        MyInput();
        SpeedControl();
        StateHandler();
        CheckVault();

        if (Input.GetKeyDown(KeyCode.P))
        {
            CSVMetricsLogger.Instance.LogDesincronizacionMovimiento(100, "Player_2", Vector3.zero, new Vector3(0, 0, 1));
            Debug.Log("Dato guardado de prueba.");
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        if (inputProvider == null) return;

        
        PlayerInputData input = inputProvider.GetInput();

        hInput = input.Move.x;
        vInput = input.Move.y;
        isSprintHeld = input.Sprint;

        
        if (input.Jump && !wasJumping) TryJump();
        wasJumping = input.Jump;

        
        isCrouchHeld = input.Crouch;
        if (isCrouchHeld && !wasCrouching) StartCrouch();
        else if (!isCrouchHeld && wasCrouching) StopCrouch();
        wasCrouching = isCrouchHeld;
    }


    private void TryJump()
    {
        if (readyToJump && isGrounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void StartCrouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - 0.5f, transform.localPosition.z);
        playerHeight = 1;
    }

    private void StopCrouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + 0.5f, transform.localPosition.z);
        playerHeight = 2;
        if (isSliding) slideEnd();
    }

    private void StateHandler()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (isCrouchHeld && flatVel.magnitude < slideThreshold)
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
            maxSpeed = maxCrouchSpeed;
        }
        else if (isCrouchHeld && flatVel.magnitude >= slideThreshold && isGrounded)
        {
            state = MovementState.sliding;
            slideStart();
        }
        else if (isGrounded && isSprintHeld && !isCrouchHeld)
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
            maxSpeed = maxSprintSpeed;
        }
        else if (isGrounded && !isCrouchHeld)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
            maxSpeed = maxWalkSpeed;
        }
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * vInput + orientation.right * hInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.angularVelocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        else if (isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!isGrounded)
        {
            
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            float currentSpeed = flatVel.magnitude;

            Vector3 wishDir = moveDirection.normalized;

           
            if (wishDir.magnitude > 0)
            {
                
                Vector3 newDirection = Vector3.RotateTowards(flatVel.normalized, wishDir, airMultiplier * Time.fixedDeltaTime, 0f).normalized;

                
                rb.linearVelocity = new Vector3(newDirection.x * currentSpeed, rb.linearVelocity.y, newDirection.z * currentSpeed);

               
                if (currentSpeed < moveSpeed)
                {
                    rb.AddForce(wishDir * moveSpeed * 5f, ForceMode.Force);
                }
            }
        }

        rb.useGravity = !OnSlope();
    }
    
    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (flatVel.magnitude > maxSpeed && isGrounded)
            {
                Vector3 limitedVel = flatVel.normalized * maxSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private void slideStart()
    {
        if (!isSliding)
        {
            moveSpeed = slideSpeed;
            maxSpeed = maxSlideSpeed;
            Vector3 slideDir = orientation.forward * vInput + orientation.right * hInput;
            rb.AddForce(slideDir.normalized * slidePush, ForceMode.Impulse);
            isSliding = true;
        }
    }

    private void slideEnd()
    {
        isSliding = false;
    }

    private void CheckVault()
    {
        if (isGrounded || !readyToVault || isVaulting) return;

        Vector3 lowerRayPos = transform.position;
        
        Vector3 upperRayPos = transform.position + Vector3.up * (playerHeight * 0.5f - 0.1f);

        bool lowerHit = Physics.Raycast(lowerRayPos, orientation.forward, vaultDetectionLength, whatIsGround);
        bool upperHit = Physics.Raycast(upperRayPos, orientation.forward, vaultDetectionLength, whatIsGround);

        
        if (lowerHit && !upperHit)
        {
            
            Vector3 downRayStart = upperRayPos + (orientation.forward * vaultDetectionLength);

            if (Physics.Raycast(downRayStart, Vector3.down, out RaycastHit downHit, playerHeight, whatIsGround))
            {
                
                StartCoroutine(PerformVault(downHit.point));
            }
        }
    }

    private IEnumerator PerformVault(Vector3 targetLedgeFloor)
    {
        isVaulting = true;
        readyToVault = false;

        
        rb.isKinematic = true;

        Vector3 startPos = transform.position;

        
        Vector3 targetPos = targetLedgeFloor + Vector3.up * (playerHeight * 0.5f + 0.1f) + orientation.forward * 0.6f;

        float timeElapsed = 0f;

       
        while (timeElapsed < vaultDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, timeElapsed / vaultDuration);
            timeElapsed += Time.deltaTime;

            yield return null; 
        }

        
        transform.position = targetPos;

        
        rb.isKinematic = false;
        isVaulting = false;

        Invoke(nameof(ResetVault), vaultCooldown);
    }

    private void ResetVault()
    {
        readyToVault = true;
    }
}
