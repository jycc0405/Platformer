using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFunction : MonoBehaviour
{
    
    public bool dead { get; protected set; }

    public event Action OnDeath;

    public GameObject PlayermovementGameObject;

    protected void OnEnable()
    {
        dead = false;
        
    }

    public virtual void Die()
    {
        if (OnDeath!=null)
        {
            OnDeath();
        }

        //dead = true;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!dead)
        {
            IItem item = other.GetComponent<IItem>();

            if (item!=null)
            {
                item.Use(PlayermovementGameObject);
            }

            if (other.CompareTag("DeathZone"))
            {
                Die();
            }
            
        }
    }
}
