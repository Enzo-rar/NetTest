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
    public float vaultDetectionLength = 1f; // A qué distancia detecta el borde
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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = transform.localScale.y;
    }

    void Update()
    {
        if (isVaulting) return;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        if (state == MovementState.walking || state == MovementState.crouching || state == MovementState.sprinting)
        {
            rb.linearDamping = groundDrag;
        }
        else if (state == MovementState.sliding) rb.linearDamping = 1;
        else rb.linearDamping = 0;
        
        MyInput();
        SpeedControl();
        StateHandler();
        CheckVault();
        
        Debug.Log("Speed: " + rb.linearVelocity.magnitude);

        
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        hInput = Input.GetAxisRaw("Horizontal");
        vInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(KeyCode.Space) && readyToJump && isGrounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - 0.5f, transform.localPosition.z);
            playerHeight = 1;
        }

        if(Input.GetKeyUp(KeyCode.LeftControl))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + 0.5f, transform.localPosition.z);
            playerHeight = 2;
            if (isSliding)
            {
                slideEnd();
            }
        }
    }

    private void StateHandler()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (Input.GetKey(KeyCode.LeftControl) && flatVel.magnitude < slideThreshold)
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
            maxSpeed = maxCrouchSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftControl) && flatVel.magnitude >= slideThreshold && isGrounded)
        {
            state = MovementState.sliding;
            slideStart();
        }
        else if (isGrounded && Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.LeftControl))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
            maxSpeed = maxSprintSpeed;
        }
        else if (isGrounded && !Input.GetKey(KeyCode.LeftControl))
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
            // 1. Aislamos la velocidad horizontal actual y calculamos cuánta velocidad llevamos (momentum).
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            float currentSpeed = flatVel.magnitude;

            Vector3 wishDir = moveDirection.normalized;

            // 2. Solo rotamos la velocidad si el jugador está tocando las teclas de movimiento (WASD).
            if (wishDir.magnitude > 0)
            {
                // 3. Giramos la dirección actual hacia la dirección deseada.
                // Tu variable 'airMultiplier' ahora actuará como la "velocidad de giro" en el aire.
                // Te recomiendo valores entre 5 y 15 para giros muy bruscos.
                Vector3 newDirection = Vector3.RotateTowards(flatVel.normalized, wishDir, airMultiplier * Time.fixedDeltaTime, 0f).normalized;

                // 4. Aplicamos la nueva dirección multiplicada por la velocidad que ya teníamos.
                // ¡Esto es lo que conserva el 100% de tu momentum al girar 180 grados!
                rb.linearVelocity = new Vector3(newDirection.x * currentSpeed, rb.linearVelocity.y, newDirection.z * currentSpeed);

                // 5. (Opcional) Si saltas desde cero y vas muy lento, te permitimos acelerar un poco 
                // hasta tu moveSpeed normal para que el salto no se sienta inútil.
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
        // Lanzamos el rayo superior ligeramente por debajo de la altura máxima para no engancharnos en techos bajos
        Vector3 upperRayPos = transform.position + Vector3.up * (playerHeight * 0.5f - 0.1f);

        bool lowerHit = Physics.Raycast(lowerRayPos, orientation.forward, vaultDetectionLength, whatIsGround);
        bool upperHit = Physics.Raycast(upperRayPos, orientation.forward, vaultDetectionLength, whatIsGround);

        // Si hay una pared delante, pero espacio libre a la altura de la cabeza...
        if (lowerHit && !upperHit)
        {
            // 3º Rayo: Se adelanta hacia el hueco libre y mira hacia abajo para encontrar exactamente dónde está el suelo
            Vector3 downRayStart = upperRayPos + (orientation.forward * vaultDetectionLength);

            if (Physics.Raycast(downRayStart, Vector3.down, out RaycastHit downHit, playerHeight, whatIsGround))
            {
                // Iniciar la escalada calculada
                StartCoroutine(PerformVault(downHit.point));
            }
        }
    }

    private IEnumerator PerformVault(Vector3 targetLedgeFloor)
    {
        isVaulting = true;
        readyToVault = false;

        // 1. Apagamos las físicas del Rigidbody para evitar atascos
        rb.isKinematic = true;

        Vector3 startPos = transform.position;

        // 2. Calculamos dónde debe quedar nuestro cuerpo:
        // La altura del suelo que detectó + la mitad de nuestra altura + un pequeño empujoncito hacia adelante para entrar en la plataforma
        Vector3 targetPos = targetLedgeFloor + Vector3.up * (playerHeight * 0.5f + 0.1f) + orientation.forward * 0.6f;

        float timeElapsed = 0f;

        // 3. Interpolación (Lerp) para subir suavamente frame a frame
        while (timeElapsed < vaultDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, timeElapsed / vaultDuration);
            timeElapsed += Time.deltaTime;

            yield return null; // Espera al siguiente frame
        }

        // Aseguramos de que terminamos exactamente en el punto deseado
        transform.position = targetPos;

        // 4. Encendemos de nuevo las físicas y devolvemos el control
        rb.isKinematic = false;
        isVaulting = false;

        Invoke(nameof(ResetVault), vaultCooldown);
    }

    private void ResetVault()
    {
        readyToVault = true;
    }
}
