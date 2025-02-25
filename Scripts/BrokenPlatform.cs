using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BrokenPlatform : MonoBehaviour
{
    public float BrokenTime=3f;
    public float repairTime=2f;

    private float startTime;
    private bool broking;
    private Tilemap SpriteRenderer;
    
    private PlayerMovement _playerMovement;
    // Start is called before the first frame update
    void Start()
    {
        startTime = 0;
        broking = false;
        SpriteRenderer = GetComponent<Tilemap>();

    }

    // Update is called once per frame
    void Update()
    {
        if ((Time.time>startTime+BrokenTime)&&broking)
        {
            broking = false;
            StartCoroutine(nameof(Repair));
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _playerMovement = other.gameObject.GetComponent<PlayerMovement>();
            if (((other.contacts[0].normal.y<-0.7f)||((other.contacts[0].normal.x>0.7f||other.contacts[0].normal.x<-0.7f)&&_playerMovement.isClimbing))&&!broking)
            {
                broking = true;
                startTime = Time.time;
            }
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (broking)
            {
                startTime = -BrokenTime;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (broking)
            {
                startTime = -BrokenTime;
            }
        }
    }


    private IEnumerator Repair()
    {
        SpriteRenderer.color = new Color(255, 255, 255, 0.5f);
        gameObject.GetComponent<TilemapCollider2D>().enabled = false;
        yield return new WaitForSeconds(repairTime);
        gameObject.GetComponent<TilemapCollider2D>().enabled = true;
        SpriteRenderer.color = new Color(255, 255, 255, 1);
    }
    
}
