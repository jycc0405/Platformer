using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance
    {
        get
        {
            if (m_instance==null)
            {
                m_instance = FindObjectOfType<GameManager>();
            }

            return m_instance;
        }
    }

    private static GameManager m_instance;
    
    public bool isGameover { get; private set; }

    private void Awake()
    {
        if (instance!=this)
        {
            Destroy(gameObject);
        }
    }

    private Vector3 SpawnPoint;
    private GameObject player;

    private void Start()
    {
        player=GameObject.FindWithTag("Player");
        FindObjectOfType<PlayerFunction>().OnDeath += Respawn;
    }

    public void SetRespawn(Vector3 SpawnPoint)
    {
        this.SpawnPoint = SpawnPoint;
    }

    private void Respawn()
    {
        player.GetComponent<PlayerMovement>().enabled = false;
        player.transform.position = SpawnPoint;
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<PlayerMovement>().SetVelocityZero();

    }
}
