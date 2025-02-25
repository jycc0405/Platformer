using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<UIManager>();
            }

            return m_instance;
        }
        
    }

    private static UIManager m_instance;

    public GameObject startEvent;
    public GameObject endingEvent;

    public void StartEvent()
    {
        startEvent.SetActive(true);
    }

    public void CloseStartEvent()
    {
        startEvent.SetActive(false);
    }

    public void EndingEvent()
    {
        endingEvent.SetActive(true);
    }
    
    public void CloseEndingEvent()
    {
        Application.Quit();
    }
    
    void Start()
    {
        StartEvent();
    }

}
