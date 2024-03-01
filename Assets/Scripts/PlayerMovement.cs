using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Movement")]
    [SerializeField] private float velPower = 0.96f;
    [SerializeField] private float maxSpeed = 9f;
    [SerializeField] private float runAccelAmount = 13f;
    [SerializeField] private float runDeccelAmount = 16f;
    [SerializeField] private float frictionAmount = 0.22f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 13f;
    [SerializeField] private float jumpInputBufferTime;

    [Header("Checks")]
    [SerializeField] private bool isJumping;
    [SerializeField] private bool isJumpCut;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.49f, 0.03f);

    [Header("Layers & Tags")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Falling Movement")]
    [SerializeField] private float maxFastFallSpeed;
    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float fastFallGravityMult;
    [SerializeField] private float fallGravityMult;
    [SerializeField] private float jumpCutGravityMult;
    [SerializeField] private float gravityScale;

    public Rigidbody2D playerRb { get; private set; }
    private Vector2 moveInput;
    private float lastOnGroundTime;
    private float lastPressedJumpTime;

    void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
    }

    void Update()
    {
        lastOnGroundTime -= Time.deltaTime;
        lastPressedJumpTime -= Time.deltaTime;

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, groundLayer))
            lastOnGroundTime = 0.1f;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnJumpInput();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;
            OnJumpUpInput();
        }

        if (CanJump() && lastPressedJumpTime > 0)
        {
            isJumping = true;
            isJumpCut = false;
            Jump();
        }

        if (playerRb.velocity.y < 0 && moveInput.y < 0)
        {
            SetGravityScale(gravityScale * fastFallGravityMult);
            playerRb.velocity = new Vector2(playerRb.velocity.x, Mathf.Max(playerRb.velocity.y, -maxFastFallSpeed));
        }
        else if (playerRb.velocity.y < 0)
        {
            SetGravityScale(gravityScale * fallGravityMult);
            playerRb.velocity = new Vector2(playerRb.velocity.x, Mathf.Max(playerRb.velocity.y, -maxFallSpeed));
        }
        else if (isJumpCut)
        {
            SetGravityScale(gravityScale * fastFallGravityMult);
            playerRb.velocity = new Vector2(playerRb.velocity.x, Mathf.Max(playerRb.velocity.y, -maxFastFallSpeed));
        }
        else
        {
            SetGravityScale(gravityScale);
        }

    }

    void FixedUpdate()
    {
        Run();
    }

    private void Run()
    {
        float targetSpeed = moveInput.x * maxSpeed;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelAmount : runDeccelAmount;

        float speedDif = targetSpeed - playerRb.velocity.x;

        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        playerRb.AddForce(movement * Vector2.right);

        if (lastOnGroundTime > 0 && Mathf.Abs(Input.GetAxisRaw("Horizontal")) < 0.01f)
        {
            float amount = Mathf.Min(Mathf.Abs(playerRb.velocity.x), Mathf.Abs(frictionAmount));

            amount *= Mathf.Sign(playerRb.velocity.x);

            playerRb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }
    }

    public void OnJumpInput()
    {
        lastPressedJumpTime = jumpInputBufferTime;
    }

    private void OnJumpUpInput()
    {
        if (CanJumpCut())
        {
            isJumpCut = true;
        }
    }

    private void Jump()
    {
        lastPressedJumpTime = 0;
        lastOnGroundTime = 0;

        float force = jumpForce;

        if (playerRb.velocity.y < 0)
            force -= playerRb.velocity.y;

        playerRb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    private bool CanJumpCut()
    {
        return isJumping && playerRb.velocity.y > 0;
    }

    private bool CanJump()
    {
        return lastOnGroundTime > 0 && !isJumping;
    }

    public void SetGravityScale(float scale)
    {
        playerRb.gravityScale = scale;
    }
}
