using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCameraZone : MonoBehaviour
{
    public GameObject virtualCam;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")&& !other.isTrigger)
        {
            virtualCam.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")&& !other.isTrigger)
        {
            virtualCam.SetActive(false);
        }
    }
    
    private void OnDrawGizmos()
    {
        PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D>();
        if (polygonCollider == null)
            return;

        Gizmos.color = Color.green;

        Vector2[] points = polygonCollider.points;
        Vector3[] worldPoints = new Vector3[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            worldPoints[i] = transform.TransformPoint(points[i]);
        }

        for (int i = 0; i < points.Length; i++)
        {
            int nextIndex = (i + 1) % points.Length;
            Gizmos.DrawLine(worldPoints[i], worldPoints[nextIndex]);
        }
    }
}
