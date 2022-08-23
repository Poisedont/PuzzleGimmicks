using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupControl : Singleton<PopupControl>
{
    public List<GameObject> m_listPopup = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenPopup(string name)
    {
        for(int i = 0; i < m_listPopup.Count; i++)
        {
            if (m_listPopup[i].name.Equals(name))
            {
                m_listPopup[i].SetActive(true);
            }
            else
            {
                m_listPopup[i].SetActive(false);
            }
        }

        if(name.Equals("GameOver"))
        {
            StartCoroutine(UpdateTargetInGameOverPopup());
        }

        if (name.Equals("ShowTargetStartLevel"))
        {
            StartCoroutine(UpdateTargetInShowPopupStartLevel());
        }
    }

    public bool IsPopupShowing()
    {
        for (int i = 0; i < m_listPopup.Count; i++)
        {
            if (m_listPopup[i].active)
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator UpdateTargetInGameOverPopup()
    {
        yield return new WaitForEndOfFrame();
        GameOver.m_callSetTarget.Invoke();
    }

    IEnumerator UpdateTargetInShowPopupStartLevel()
    {
        yield return new WaitForEndOfFrame();
        ShowTargetStartLevel.m_callSetTarget.Invoke();
    }
}
