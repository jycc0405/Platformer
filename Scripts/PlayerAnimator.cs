using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Rigidbody2D playerRigidbody;

    private Animator Animator;

    private PlayerInput PlayerInput;
    
    public float RunSpeed { private get; set; }

    public bool isJumping { private get; set; }

    public bool isFalling { private get; set; }

    public bool isClimbing { private get; set; }
    
    public bool isOnMovingPlatform { private get; set; }

    public bool isLookingRight { get; set; }
    public int DashCount { private get; set; }

    [SerializeField] private GameObject playerSprite;

    private SpriteRenderer SpriteRenderer;

    private static readonly int Run = Animator.StringToHash("Run");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int Fall = Animator.StringToHash("Fall");
    private static readonly int Climb = Animator.StringToHash("Climb");
    private static readonly int ClimbSpeed = Animator.StringToHash("ClimbSpeed");

    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        PlayerInput = GetComponent<PlayerInput>();
        SpriteRenderer = playerSprite.GetComponent<SpriteRenderer>();
        Animator = playerSprite.GetComponent<Animator>();
        if (transform.rotation == Quaternion.Euler(0, 0, 0))
        {
            isLookingRight = true;
        }
        else isLookingRight = false;
    }

    // Update is called once per frame
    void Update()
    {
        if ((PlayerInput.Xmove > 0 && !isLookingRight && playerRigidbody.velocity.x > -10f && !isClimbing&& !isOnMovingPlatform) ||(isOnMovingPlatform&&PlayerInput.Xmove > 0&&!isClimbing&& !isLookingRight))
        {
            playerSprite.transform.rotation = Quaternion.Euler(0, 0, 0);
            isLookingRight = true;
        }
        else if ((PlayerInput.Xmove < 0 && isLookingRight && playerRigidbody.velocity.x < 10f && !isClimbing&& !isOnMovingPlatform)||(isOnMovingPlatform&&PlayerInput.Xmove < 0&&!isClimbing&& isLookingRight))
        {
            playerSprite.transform.rotation = Quaternion.Euler(0, 180, 0);
            isLookingRight = false;
        }
        
        Animator.SetFloat(Run, Mathf.Abs(RunSpeed));

        Animator.SetBool(Jump, isJumping);

        Animator.SetBool(Fall, isFalling);

        if (isClimbing && !Animator.GetBool(Climb))
        {
            Animator.SetBool(Climb, isClimbing);
        }
        else if (!isClimbing && Animator.GetBool(Climb))
        {
            Animator.SetBool(Climb, isClimbing);
        }

        if (isClimbing)
        {
            SpriteRenderer.flipX = true;
            Animator.SetFloat(ClimbSpeed, playerRigidbody.velocity.y);
        }
        else
        {
            SpriteRenderer.flipX = false;
        }

        switch (DashCount)
        {
            case 0: SpriteRenderer.color=Color.cyan;
                break;
            case 1: SpriteRenderer.color=Color.white;
                break;
            case 2 : SpriteRenderer.color=Color.green;
                break;
        }
    }
}