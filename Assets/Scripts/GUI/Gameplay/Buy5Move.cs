using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Buy5Move : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ExitBtnClick()
    {
        if (PlayerManager.Instance && PlayerManager.Instance.m_PlayerLive > 0)
        {
            PopupControl.Instance.OpenPopup("GameOver");
        }
        else
        {
            gameObject.SetActive(false);
            SceneManager.LoadScene("Menu");
        }

    }


}
