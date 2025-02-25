using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RespawnZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        GameManager.instance.SetRespawn(this.transform.position);
    }
}
