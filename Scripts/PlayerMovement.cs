using System.Collections;
using UnityEngine;
using Slider = UnityEngine.UI.Slider;

public class PlayerMovement : MonoBehaviour
{
    [Header("Run")] public float maxSpeed = 5f;

    public float runAccelAmount = 2.5f;
    public float runDeccelAmount = 5;
    public float accelInAir = 0.65f;
    public float deccelInAir = 0.65f;
    public float MaxFallingSpeed;
    public float MaxGroundDelay = 0.2f;
    private float lastGroundDelay;

    [Header("Jump")] public float jumpForce = 10f;
    public float jumpInputBufferTime = 0.1f;
    public float coyoteTime = 0.1f;
    public float wallJumpX = 20f;
    public float wallJumpY = 20f;
    public float wallUpJump1 = 0.6f;
    public float wallUpJump2 = 0.7f;
    public float wallJumpLerp = 0.15f;


    [Header("Dash")] public int maxDashCount = 1;
    public float DashSpeed = 20f;
    public float DashEndSpeed = 15f;
    public float DashSleepTime = 0.05f;
    public float DashAttackTime = 0.15f;
    public float DashEndTime = 0.15f;
    public float dashEndRunLerp = 0.5f;
    public float DashRefillTime = 0.2f;

    [Header("Climb")] public float climbSpeed = 2f;

    public float maxClimbStamina = 100f;
    private float climbStamina;
    public float climbUpStamina = 20f;
    public float climbHoldingStamina = 10f;
    public float climbDownStamina = 0f;
    public float climbJumpStamina = 25f;


    public float climbJumpSpeed1 = 2f;
    public float climbJumpSpeed2 = 2f;
    public float climbJumpSpeed3 = 2f;

    [Header("Physics")] public float MovingPlatformVelocityBufferTime = 0.5f;
    
    #region Check Colliders

    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2();
    [SerializeField] private Transform frontCheck;
    [SerializeField] private Transform backCheck;
    [SerializeField] private Vector2 frontCheckSize;
    [SerializeField] private Vector2 backCheckSize;

    #endregion
    
    #region LAYERS

    [Header("Layers")] [SerializeField] private LayerMask flatFormLayerMask;
    [SerializeField] private LayerMask movingFlatFormLayerMask;
    [SerializeField] private LayerMask deadZoneLayerMask;

    #endregion
    
    [Header("SlideBar")] public Slider ClimbSlider;

    #region STATE PARAMETERS

    private bool isJumping;
    private bool isRunning;
    private bool isJumpCut;
    private bool isFalling;
    public bool isClimbing { get; private set; }
    private bool isReadyToClimb;
    private bool frontWall;
    private bool backWall;
    private bool isDashing;
    private bool isDashAttacking;
    private bool isWallJumping;
    private bool isEndClimbing;
    private bool isOnGround;

    private int DashCount;
    private int AddDashCount = 0;

    private Vector2 DashDir;

    private float NomalGravity;

    private Vector2 movingPlatformVelocity;
    private bool isOnMovingPlatform;
    private Vector2 movingPlatformVelocityStore;
    private float lastMovingPlatformStop;
    private bool isPlatformMoving;
    private bool isFrontDeadzone;

    #endregion

    #region INPUT PARAMETERS

    public float LastPressedJumpTime { get; private set; }
    public float LastPressedDashTime { get; private set; }
    public float LastOnGroundTime { get; private set; }

    #endregion

    private PlayerInput PlayerInput;
    private Rigidbody2D playerRigidbody2D;
    private PlayerAnimator playerAnimator;


    void Start()
    {
        PlayerInput = GetComponent<PlayerInput>();
        playerRigidbody2D = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<PlayerAnimator>();

        NomalGravity = playerRigidbody2D.gravityScale;
        ClimbSlider.maxValue = maxClimbStamina;
    }

    private void OnEnable()
    {
        isOnMovingPlatform = false;
    }

    private void Update()
    {
        #region TIMERS

        LastPressedJumpTime -= Time.deltaTime;
        LastPressedDashTime -= Time.deltaTime;
        LastOnGroundTime -= Time.deltaTime;
        lastMovingPlatformStop -= Time.deltaTime;
        lastGroundDelay -= Time.deltaTime;

        #endregion

        #region INPUT HANDLER

        if (PlayerInput.JumpButtonDown)
        {
            OnJumpInput();
        }

        if (PlayerInput.JumpButtonUp)
        {
            OnJumpUpInput();
        }

        if (PlayerInput.Dash)
        {
            OnDashInput();
        }

        if (PlayerInput.Climb && !isDashing)
        {
            OnClimbInput();
        }

        if (PlayerInput.ClimbUp)
        {
            OnClimbUpInput();
        }

        #endregion

        #region COLLISION CHECKS

        if (!isJumping || !isWallJumping)
        {
            if (Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, flatFormLayerMask))
            {
                if (!isClimbing && !isEndClimbing && lastGroundDelay < 0)
                {
                    LastOnGroundTime = coyoteTime;
                    isOnGround = true;
                    if (LastPressedDashTime < 0 && DashCount != 0)
                    {
                        DashCount = 0;
                    }
                }
            }
            else
            {
                if (isOnGround)
                {
                    if (Physics2D.OverlapBox(groundCheck.position,
                            new Vector2(groundCheckSize.x, groundCheckSize.y * 2f), 0, flatFormLayerMask) &&
                        !isJumping && !isWallJumping && !isClimbing && !isDashing)
                    {
                        LastOnGroundTime = coyoteTime;
                        isOnGround = true;
                    }
                    else
                    {
                        isOnGround = false;
                    }
                }
                else
                {
                    lastGroundDelay = MaxGroundDelay;
                }
            }

            if ((Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, movingFlatFormLayerMask)) ||
                (Physics2D.OverlapBox(frontCheck.position, frontCheckSize, 0, movingFlatFormLayerMask) && isClimbing))
            {
                isOnMovingPlatform = true;
                Collider2D pfcollider2D = null;
                if (Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, movingFlatFormLayerMask))
                {
                    pfcollider2D =
                        Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, movingFlatFormLayerMask);
                }
                else if (Physics2D.OverlapBox(frontCheck.position, frontCheckSize, 0, movingFlatFormLayerMask) &&
                         isClimbing)
                {
                    pfcollider2D =
                        Physics2D.OverlapBox(frontCheck.position, frontCheckSize, 0, movingFlatFormLayerMask);
                }

                if (pfcollider2D!=null)
                {
                    Rigidbody2D pfrigidbody2D = pfcollider2D.GetComponent<Rigidbody2D>();
                    if (pfrigidbody2D.velocity != Vector2.zero)
                    {
                        if (pfrigidbody2D.velocity.magnitude > movingPlatformVelocityStore.magnitude)
                        {
                            movingPlatformVelocityStore = pfrigidbody2D.velocity;
                        }

                        lastMovingPlatformStop = MovingPlatformVelocityBufferTime;
                        isPlatformMoving = true;
                    }
                    else if (pfrigidbody2D.velocity.magnitude < 0.2f)
                    {
                        isPlatformMoving = false;
                        if (PlayerInput.Xmove == 0 && PlayerInput.Ymove == 0 && !isJumping)
                        {
                            playerRigidbody2D.velocity = Vector2.zero;
                        }

                        if (lastMovingPlatformStop < 0)
                        {
                            movingPlatformVelocityStore = Vector2.zero;
                        }
                    }

                    movingPlatformVelocity = pfrigidbody2D.velocity;
                }
            }
            else
            {
                if (isOnMovingPlatform)
                {
                    if ((Physics2D.OverlapBox(groundCheck.position,
                             new Vector2(groundCheckSize.x, groundCheckSize.y * 2f), 0, flatFormLayerMask) &&
                         !isJumping && !isWallJumping && !isClimbing && !isDashing) ||
                        (Physics2D.OverlapBox(frontCheck.position, Vector2.right * 2.5f, 0, movingFlatFormLayerMask) &&
                         !isDashing && !isJumping && !isWallJumping))
                    {
                        isOnMovingPlatform = true;
                    }
                    else
                    {
                        isOnMovingPlatform = false;
                        isPlatformMoving = false;
                    }
                }
            }
        }

        if (Physics2D.OverlapBox(frontCheck.position, frontCheckSize, 0, flatFormLayerMask))
        {
            frontWall = true;
        }
        else
        {
            frontWall = false;
        }

        if (Physics2D.OverlapBox(backCheck.position, backCheckSize, 0, flatFormLayerMask))
        {
            backWall = true;
        }
        else backWall = false;

        if (Physics2D.OverlapBox(frontCheck.position, frontCheckSize, 0, deadZoneLayerMask))
        {
            isFrontDeadzone = true;
        }
        else isFrontDeadzone = false;

        #endregion

        #region STATE CHECK

        if (!isClimbing && !isEndClimbing)
        {
            if (LastOnGroundTime > 0)
            {
                if (!isWallJumping)
                {
                    climbStamina = maxClimbStamina;
                }

                isReadyToClimb = false;
            }

            if ((isJumping || isWallJumping) && playerRigidbody2D.velocity.y < 0)
            {
                isJumping = false;
                isWallJumping = false;
                isFalling = true;
            }

            if (LastOnGroundTime > 0 && !isJumping && !isWallJumping)
            {
                isJumpCut = false;
                isFalling = false;
            }
        }

        if (playerRigidbody2D.velocity.y < 0 && !isClimbing && !isOnGround)
        {
            isFalling = true;
        }
        else if (playerRigidbody2D.velocity.y >= 0 || isOnGround)
        {
            isFalling = false;
        }

        #endregion

        #region JUMP CHECKS

        if (!isDashAttacking)
        {
            if (CanJump() && LastPressedJumpTime > 0 && !isClimbing && !isEndClimbing)
            {
                isJumping = true;
                isWallJumping = false;
                isJumpCut = false;
                isFalling = false;
                LastPressedJumpTime = 0;

                Jump();
            }
            else if (CanWallJump() && LastPressedJumpTime > 0)
            {
                isWallJumping = true;
                isJumping = false;
                isJumpCut = false;
                isFalling = false;
                LastPressedJumpTime = 0;


                if ((frontWall && !playerAnimator.isLookingRight) || (backWall && playerAnimator.isLookingRight))
                {
                    StartCoroutine(nameof(WallJump), 1);
                }
                else if ((frontWall && playerAnimator.isLookingRight) || (backWall && !playerAnimator.isLookingRight))
                {
                    StartCoroutine(nameof(WallJump), -1);
                }
            }
        }

        if (isJumpCut)
        {
            JumpCut();
            isJumpCut = false;
        }

        #endregion

        #region CLIMB CHECK

        if (isClimbing || climbStamina < maxClimbStamina)
        {
            ClimbSlider.gameObject.SetActive(true);
            ClimbSlider.value = climbStamina;
        }
        else
        {
            ClimbSlider.gameObject.SetActive(false);
        }

        #endregion

        #region PLAYERANIMATOR PARAMETERS

        if (isOnMovingPlatform && !isClimbing)
        {
            playerAnimator.RunSpeed = playerRigidbody2D.velocity.x - movingPlatformVelocity.x;
        }
        else if (!isClimbing)
        {
            playerAnimator.RunSpeed = playerRigidbody2D.velocity.x;
        }
        else if (isClimbing)
        {
            playerAnimator.RunSpeed = 0;
        }

        playerAnimator.isFalling = isFalling;
        playerAnimator.isJumping = isJumping || isWallJumping;
        playerAnimator.isClimbing = isClimbing;
        playerAnimator.isOnMovingPlatform = isOnMovingPlatform;
        playerAnimator.DashCount = (1 - DashCount) + AddDashCount;

        #endregion

        #region GRAVITY

        if (playerRigidbody2D.velocity.y < 0)
        {
            playerRigidbody2D.velocity = new Vector2(playerRigidbody2D.velocity.x,
                Mathf.Clamp(playerRigidbody2D.velocity.y, -MaxFallingSpeed, 0));
        }

        #endregion
    }

    private void FixedUpdate()
    {
        if (!isDashing && !isClimbing && !isEndClimbing)
        {
            if (isWallJumping)
            {
                Move(wallJumpLerp);
            }
            else
            {
                Move(1);
            }
        }
        else if (isDashAttacking)
        {
            Move(dashEndRunLerp);
        }
    }

    #region MOVE

    private void Move(float lerpAmount)
    {
        float targetSpeed = PlayerInput.Xmove * maxSpeed;

        targetSpeed = Mathf.Lerp(playerRigidbody2D.velocity.x, targetSpeed, lerpAmount);

        float accelRate;

        if (LastOnGroundTime > 0)
        {
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelAmount : runDeccelAmount;
        }
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelAmount * accelInAir : runDeccelAmount * deccelInAir;

        if (Mathf.Abs(playerRigidbody2D.velocity.x) > Mathf.Abs(targetSpeed) &&
            Mathf.Sign(playerRigidbody2D.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f &&
            LastOnGroundTime < 0)
        {
            accelRate = 0f;
        }

        float speedDif;

        float movement;


        if (isOnMovingPlatform && isPlatformMoving && !isClimbing && !isJumping && !isWallJumping)
        {
            speedDif = targetSpeed + movingPlatformVelocity.x - playerRigidbody2D.velocity.x;
            movement = speedDif * accelRate;
            playerRigidbody2D.AddForce(movingPlatformVelocity + movement * Vector2.right, ForceMode2D.Force);
        }
        else
        {
            if (targetSpeed == 0 && !isJumping && !isClimbing && isOnMovingPlatform && !isPlatformMoving &&
                !isEndClimbing && lastMovingPlatformStop > 0 && !isWallJumping)
            {
                playerRigidbody2D.velocity = Vector2.zero;
            }
            else if (!isJumping && !isClimbing && isOnMovingPlatform && !isPlatformMoving &&
                     !isEndClimbing && lastMovingPlatformStop > 0 && !isWallJumping)
            {
                playerRigidbody2D.velocity = new Vector2(playerRigidbody2D.velocity.x, 0);
            }

            speedDif = targetSpeed - playerRigidbody2D.velocity.x;

            movement = speedDif * accelRate;

            playerRigidbody2D.AddForce(movement * Vector2.right, ForceMode2D.Force);

            if (Mathf.Abs(playerRigidbody2D.velocity.x) < 0.1f && PlayerInput.Xmove == 0)
            {
                playerRigidbody2D.velocity = new Vector2(0, playerRigidbody2D.velocity.y);
            }
        }
    }

    #endregion

    #region JUMP

    private void Jump()
    {
        if (lastMovingPlatformStop > 0)
        {
            lastMovingPlatformStop = 0;
            playerRigidbody2D.velocity = new Vector2(playerRigidbody2D.velocity.x, 0);
            playerRigidbody2D.AddForce(new Vector2(0, jumpForce) + movingPlatformVelocityStore, ForceMode2D.Impulse);
        }
        else
        {
            lastMovingPlatformStop = 0;
            playerRigidbody2D.velocity = new Vector2(playerRigidbody2D.velocity.x, 0);
            playerRigidbody2D.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        }
    }

    private void JumpCut()
    {
        playerRigidbody2D.velocity = new Vector2(playerRigidbody2D.velocity.x, playerRigidbody2D.velocity.y * 0.5f);
    }

    private IEnumerator WallJump(int dir)
    {
        isWallJumping = true;

        LastOnGroundTime = 0;
        bool straightWallJump = false;
        bool isMoving = false;


        if ((PlayerInput.Xmove == 0 && playerRigidbody2D.velocity.y >= 0) ||
            (isClimbing && playerAnimator.isLookingRight && PlayerInput.Xmove > 0) ||
            (isClimbing && !playerAnimator.isLookingRight && PlayerInput.Xmove < 0))
        {
            isClimbing = false;
            isReadyToClimb = false;
            SetPlayerGravityScale(NomalGravity);

            if (isOnMovingPlatform && lastMovingPlatformStop > 0)
            {
                lastMovingPlatformStop = 0;
                playerRigidbody2D.velocity = Vector2.zero;

                playerRigidbody2D.AddForce(
                    new Vector2(wallJumpX * dir * wallUpJump1, wallJumpY * wallUpJump1) + movingPlatformVelocityStore,
                    ForceMode2D.Impulse);
                straightWallJump = true;
                isMoving = true;
            }
            else
            {
                playerRigidbody2D.velocity = Vector2.zero;
                playerRigidbody2D.AddForce(new Vector2(wallJumpX * dir * wallUpJump1, wallJumpY * wallUpJump1),
                    ForceMode2D.Impulse);
                straightWallJump = true;
            }
        }
        else
        {
            isClimbing = false;
            isReadyToClimb = false;
            SetPlayerGravityScale(NomalGravity);

            playerRigidbody2D.velocity = Vector2.zero;
            playerRigidbody2D.AddForce(new Vector2(wallJumpX * dir, wallJumpY), ForceMode2D.Impulse);
        }

        if (straightWallJump)
        {
            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }

        if (straightWallJump)
        {
            if (isMoving)
            {
                playerRigidbody2D.AddForce(
                    new Vector2(0, wallJumpY), ForceMode2D.Impulse);
                climbStamina -= climbJumpStamina;
            }
            else
            {
                playerRigidbody2D.AddForce(new Vector2(0, wallJumpY * wallUpJump2), ForceMode2D.Impulse);
                climbStamina -= climbJumpStamina;
            }

            yield return new WaitForSeconds(0.2f);
        }

        isWallJumping = false;
    }

    #endregion

    #region CLIMB

    private void Climb(float a)
    {
        if (climbStamina > 0)
        {
            if (isOnMovingPlatform)
            {
                if (PlayerInput.Ymove > 0 && !isFrontDeadzone)
                {
                    playerRigidbody2D.velocity = new Vector2(0, (climbSpeed)) + movingPlatformVelocity * a;
                    climbStamina -= climbUpStamina * Time.deltaTime;
                }
                else if (PlayerInput.Ymove < 0)
                {
                    playerRigidbody2D.velocity = new Vector2(0, (-climbSpeed)) + movingPlatformVelocity * a;
                    climbStamina -= climbDownStamina * Time.deltaTime;
                }
                else if (PlayerInput.Ymove == 0 || isFrontDeadzone)
                {
                    playerRigidbody2D.velocity = new Vector2(0, 0) + movingPlatformVelocity * a;
                    climbStamina -= climbHoldingStamina * Time.deltaTime;
                }
            }
            else if (!isOnMovingPlatform)
            {
                if (movingPlatformVelocity == Vector2.zero)
                {
                    playerRigidbody2D.velocity = Vector2.zero;
                }

                if (PlayerInput.Ymove > 0 && !isFrontDeadzone)
                {
                    playerRigidbody2D.velocity = new Vector2(0, (climbSpeed));
                    climbStamina -= climbUpStamina * Time.deltaTime;
                }
                else if (PlayerInput.Ymove < 0)
                {
                    playerRigidbody2D.velocity = new Vector2(0, (-climbSpeed));
                    climbStamina -= climbDownStamina * Time.deltaTime;
                }
                else if (PlayerInput.Ymove == 0 || isFrontDeadzone)
                {
                    playerRigidbody2D.velocity = new Vector2(0, 0);
                    climbStamina -= climbHoldingStamina * Time.deltaTime;
                }
            }
        }
        else
        {
            isClimbing = false;
            SetPlayerGravityScale(NomalGravity);
        }
    }

    private IEnumerator ClimbJump()
    {
        if (lastMovingPlatformStop > 0)
        {
            lastMovingPlatformStop = 0;
            playerRigidbody2D.velocity = new Vector2(0, 1).normalized * climbJumpSpeed1 + movingPlatformVelocity;

            yield return new WaitForSeconds(0.1f);

            if (playerAnimator.isLookingRight)
            {
                playerRigidbody2D.velocity = new Vector2(1, 1).normalized * climbJumpSpeed2 + movingPlatformVelocity;
            }
            else if (!playerAnimator.isLookingRight)
            {
                playerRigidbody2D.velocity = new Vector2(-1, 1).normalized * climbJumpSpeed3 + movingPlatformVelocity;
            }
        }
        else if (!isOnMovingPlatform)
        {
            playerRigidbody2D.velocity = new Vector2(0, 1).normalized * climbJumpSpeed1;

            yield return new WaitForSeconds(0.1f);

            if (playerAnimator.isLookingRight)
            {
                playerRigidbody2D.velocity = new Vector2(1, 1).normalized * climbJumpSpeed2;
            }
            else if (!playerAnimator.isLookingRight)
            {
                playerRigidbody2D.velocity = new Vector2(-1, 1).normalized * climbJumpSpeed3;
            }
        }

        isEndClimbing = false;
    }

    private IEnumerator StickCloseToWall()
    {
        if (playerAnimator.isLookingRight)
        {
            playerRigidbody2D.velocity = Vector2.zero;
            if (isPlatformMoving)
            {
                playerRigidbody2D.AddForce(movingPlatformVelocity * 1.5f, ForceMode2D.Impulse);
            }
            else
            {
                playerRigidbody2D.AddForce(Vector2.right * 50f, ForceMode2D.Impulse);
            }
        }
        else
        {
            playerRigidbody2D.velocity = Vector2.zero;
            if (isPlatformMoving)
            {
                playerRigidbody2D.AddForce(movingPlatformVelocity * 1.5f, ForceMode2D.Impulse);
            }
            else
            {
                playerRigidbody2D.AddForce(Vector2.left * 50f, ForceMode2D.Impulse);
            }
        }


        yield return new WaitForSeconds(0.02f);
        isReadyToClimb = true;
    }

    #endregion

    #region DASH

    private IEnumerator Dash(Vector2 dir)
    {
        float startDashTime = Time.time;
        LastPressedDashTime = DashRefillTime;


        if (AddDashCount > 0)
        {
            AddDashCount--;
        }
        else
        {
            DashCount++;
        }

        isDashAttacking = true;

        SetPlayerGravityScale(0);

        while (Time.time - startDashTime <= DashAttackTime)
        {
            playerRigidbody2D.velocity = dir * DashSpeed;

            yield return null;
        }

        startDashTime = Time.time;

        isDashAttacking = false;

        SetPlayerGravityScale(NomalGravity);
        playerRigidbody2D.velocity = dir * DashEndSpeed;

        while (Time.time - startDashTime <= DashEndTime)
        {
            yield return null;
        }

        isDashing = false;
    }

    #endregion

    #region CHECK METHODS

    bool CanJump()
    {
        return LastOnGroundTime > 0 && !isJumping;
    }

    private bool CanJumpCut()
    {
        return isJumping && playerRigidbody2D.velocity.y > 0;
    }

    private bool CanDash()
    {
        return DashCount < maxDashCount + AddDashCount;
    }

    private bool CanClimb()
    {
        return frontWall;
    }

    private bool CanWallJump()
    {
        return LastPressedJumpTime > 0 && (frontWall || backWall) && LastOnGroundTime <= 0 && !isWallJumping;
    }

    private bool CanWallJumpcut()
    {
        return isWallJumping && playerRigidbody2D.velocity.y > 0;
    }

    #endregion

    #region INPUT CALLBACKS

    public void OnJumpInput()
    {
        LastPressedJumpTime = jumpInputBufferTime;
    }

    public void OnJumpUpInput()
    {
        if (CanJumpCut() || CanWallJumpcut())
        {
            isJumpCut = true;
        }
    }

    public void OnDashInput()
    {
        if (CanDash())
        {
            Sleep(DashSleepTime);

            isDashing = true;
            isJumping = false;
            isJumpCut = false;
            isClimbing = false;
            isFalling = false;

            if (PlayerInput.Xmove == 0 && PlayerInput.Ymove == 0)
            {
                if (playerAnimator.isLookingRight)
                {
                    DashDir = Vector2.right;
                    StartCoroutine(nameof(Dash), DashDir);
                }
                else
                {
                    DashDir = Vector2.left;
                    StartCoroutine(nameof(Dash), DashDir);
                }
            }
            else
            {
                DashDir = new Vector2(PlayerInput.Xmove, PlayerInput.Ymove).normalized;
                StartCoroutine(nameof(Dash), DashDir);
            }
        }
    }

    public void OnClimbInput()
    {
        if (CanClimb() && !isWallJumping && climbStamina > 0 || lastMovingPlatformStop > 0 && climbStamina > 0 &&
            Physics2D.OverlapBox(frontCheck.position, new Vector2(1.2f, 1.5f), 0, movingFlatFormLayerMask))
        {
            if (!isFrontDeadzone || isClimbing)
            {
                if (!isClimbing)
                {
                    SetPlayerGravityScale(0);


                    isDashing = false;
                    isJumping = false;
                    isJumpCut = false;
                    isClimbing = true;
                    isFalling = false;
                    StartCoroutine(nameof(StickCloseToWall));
                }

                if (isReadyToClimb)
                {
                    Climb(1f);
                }
            }
        }
        else if (lastMovingPlatformStop > 0 && climbStamina > 0 && PlayerInput.Ymove <= 0 &&
                 Physics2D.OverlapBox(frontCheck.position, Vector2.right * 2.5f, 0, movingFlatFormLayerMask) &&
                 !isWallJumping)
        {
            Climb(1.5f);
        }
        else if (isClimbing && !isEndClimbing && PlayerInput.Ymove > 0 && !isFrontDeadzone)
        {
            isClimbing = false;
            SetPlayerGravityScale(NomalGravity);
            isEndClimbing = true;
            StartCoroutine(nameof(ClimbJump));
        }
        else if (!CanClimb() || climbStamina <= 0)
        {
            isClimbing = false;
            SetPlayerGravityScale(NomalGravity);
        }
    }

    public void OnClimbUpInput()
    {
        if (isClimbing)
        {
            isClimbing = false;
            isReadyToClimb = false;
            SetPlayerGravityScale(NomalGravity);
        }
    }

    #endregion

    #region ETC

    private void SetPlayerGravityScale(float gravity)
    {
        playerRigidbody2D.gravityScale = gravity;
    }

    private void Sleep(float duration)
    {
        StartCoroutine(nameof(DoSleep), duration);
    }

    private IEnumerator DoSleep(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }

    #endregion

    #region FUNTION

    public void Restore(int maxDash, bool restoreClimbStamina)
    {
        DashCount = 0;
        AddDashCount = maxDash;
        if (restoreClimbStamina)
        {
            climbStamina = maxClimbStamina;
        }
    }

    public void SetVelocityZero()
    {
        playerRigidbody2D.velocity = Vector2.zero;
    }

    #endregion

    #region EDITOR METHODS

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(frontCheck.position, frontCheckSize);
        Gizmos.DrawWireCube(backCheck.position, backCheckSize);
    }

    #endregion
}