using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;
    private PlayerMovement _playerMovement;

    public float startSpeed=10f;
    public float backSpeed=2f;

    private bool doMoving;
    private bool isMoving;
    private bool isMovingBack;

    private bool wait;
    private Vector2 wayVector;
    
    [SerializeField] private Transform StartPoint;
    [SerializeField] private Transform GoalPoint;
    
    // Start is called before the first frame update
    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Awake()
    {
        isMoving = false;
        isMovingBack = false;
        doMoving = false;
        wait = false;
        StartPoint.position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (doMoving)
        {
            if (isMoving && !isMovingBack)
            {
                wayVector = GoalPoint.position - transform.position;
                MovePosiont(startSpeed);
                if (wayVector.magnitude < 0.4f)
                {
                    isMoving = false;
                    _rigidbody2D.MovePosition(GoalPoint.position);
                    _rigidbody2D.velocity = Vector2.zero;
                }
            }
            else if (!wait&&!isMoving)
            {

                wait = true;
                StartCoroutine(nameof(waittime));

            }
            else if (isMovingBack)
            {
                wayVector = StartPoint.position - transform.position;
                MovePosiont(backSpeed);
                if (wayVector.magnitude < 0.4f)
                {
                    isMoving = false;
                    isMovingBack = false;
                    _rigidbody2D.MovePosition(StartPoint.position);
                    _rigidbody2D.velocity = Vector2.zero;
                    doMoving = false;
                }

            }
        }
    }

    void MovePosiont(float speed)
    {
        _rigidbody2D.velocity = wayVector.normalized * (speed);
    }
    

    private IEnumerator waittime()
    {
        yield return new WaitForSeconds(3f);
        isMovingBack = true;
    }


    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _playerMovement = other.gameObject.GetComponent<PlayerMovement>();
            if (!isMoving&&!isMovingBack&&!doMoving)
            {
                if ((other.contacts[0].normal.y<-0.7f)||((other.contacts[0].normal.x>0.7f||other.contacts[0].normal.x<-0.7f)&&_playerMovement.isClimbing))
                {
                    doMoving = true;
                    isMoving = true;
                    wait = false;
                }
            }

        }
        
    }
}
