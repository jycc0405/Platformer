using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public float carameStopScale=0.01f;

    public float smoothtime = 1f;

    public float setTime = 0.1f;
    
    public float offset = 0.1f;

    private GameObject player;
    private Rigidbody2D playerRigidbody2D;
    
    public bool isCameraMoving { get; private set; }

    public Vector3 CurrentPosiont { get; private set; }
    
    
    public static CameraManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<CameraManager>();
            }

            return m_instance;
        }
    }

    private static CameraManager m_instance;
    // Start is called before the first frame update
    void Start()
    {
        CurrentPosiont = transform.position;
        player = GameObject.FindWithTag("Player");
        playerRigidbody2D = player.GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void Update()
    {
        if (setTime>0)
        {
            setTime -= Time.deltaTime;
        }
    }

    public void SetCameraPosition(Vector2 position)
    {
        if (setTime<0&&!isCameraMoving)
        {
            StartCoroutine(moveCamera(new Vector3(position.x,position.y,transform.position.z)));
        }

    }

    private IEnumerator moveCamera(Vector3 target)
    {
        isCameraMoving = true;
        Vector3 velocity = Vector3.zero;
        Time.timeScale = carameStopScale;
        if (Mathf.Approximately(target.x,transform.position.x)&& target.y>transform.position.y)
        {
            if (playerRigidbody2D.velocity.y<40f)
            {
                playerRigidbody2D.velocity = new Vector2(playerRigidbody2D.velocity.x, 0);
                playerRigidbody2D.AddForce(Vector2.up* 45f,ForceMode2D.Impulse);
            }
        }
        while (Vector3.Distance(target,transform.position)>offset)
        {
            transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, smoothtime*carameStopScale);
            yield return null;
        }

        transform.position = new Vector3(target.x,target.y,transform.position.z);
        isCameraMoving = false;
        Time.timeScale = 1;
        yield return new WaitForSeconds(0.2f);
        if (transform.position.x-30f>player.transform.position.x-2f|| transform.position.x+30f<player.transform.position.x+2f)
        {
            if (player.transform.position.x<transform.position.x)
            {
                playerRigidbody2D.velocity=Vector2.zero;
                Vector3 diff = new Vector3(target.x-29f, player.transform.position.y, 0) - player.transform.position;
                player.transform.Translate(diff,Space.World);
            }
            else
            {
                playerRigidbody2D.velocity=Vector2.zero;
                Vector3 diff = new Vector3(target.x+29f, player.transform.position.y, 0) - player.transform.position;
                player.transform.Translate(diff,Space.World);
            }
        }
    }
}
