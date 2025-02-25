using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestoreItem : MonoBehaviour, IItem
{
    public int RestoreDash;
    public bool RestoreCilmbStamina;
    public float ItemDelay;

    private SpriteRenderer _spriteRenderer;
    private bool IDelay;
    public void Use(GameObject target)
    {
        if (!IDelay)
        {
            PlayerMovement movement = target.GetComponent<PlayerMovement>();
            if (movement!=null)
            {
                movement.Restore(RestoreDash,RestoreCilmbStamina);
            }

            IDelay = true;
            _spriteRenderer.color=Color.red;
            StartCoroutine(nameof(Item));
            
        }
    }

    public void Start()
    {
        IDelay = false;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private IEnumerator Item()
    {
        while (IDelay)
        {
            yield return new WaitForSeconds(ItemDelay);
            _spriteRenderer.color=Color.blue;
            IDelay = false;
        }
    }
}
