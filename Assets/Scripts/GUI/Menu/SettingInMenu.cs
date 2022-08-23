using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingInMenu : MonoBehaviour
{
    [SerializeField] Image m_SFXOnImg;
    [SerializeField] Image m_SFXOffImg;
    [SerializeField] Image m_BGMOnImg;
    [SerializeField] Image m_BGMOffImg;

    // Start is called before the first frame update
    void Start()
    {
        if(PlayerManager.Instance.m_SFXEnable)
        {
            m_SFXOnImg.gameObject.SetActive(true);
            m_SFXOffImg.gameObject.SetActive(false);
        }
        else
        {
            m_SFXOnImg.gameObject.SetActive(false);
            m_SFXOffImg.gameObject.SetActive(true);
        }

        if (PlayerManager.Instance.m_BGMEnable)
        {
            m_BGMOnImg.gameObject.SetActive(true);
            m_BGMOffImg.gameObject.SetActive(false);
        }
        else
        {
            m_BGMOnImg.gameObject.SetActive(false);
            m_BGMOffImg.gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ExitBtnClick()
    {
        gameObject.SetActive(false);
    }

    public void SFXBtnClick()
    {
        PlayerManager.Instance.m_SFXEnable = !PlayerManager.Instance.m_SFXEnable;
        if (PlayerManager.Instance.m_SFXEnable)
        {
            m_SFXOnImg.gameObject.SetActive(true);
            m_SFXOffImg.gameObject.SetActive(false);
        }
        else
        {
            m_SFXOnImg.gameObject.SetActive(false);
            m_SFXOffImg.gameObject.SetActive(true);
        }
        PlayerManager.Instance.SaveGame();
    }

    public void BGMBtnClick()
    {
        PlayerManager.Instance.m_BGMEnable = !PlayerManager.Instance.m_BGMEnable;
        if (PlayerManager.Instance.m_BGMEnable)
        {
            m_BGMOnImg.gameObject.SetActive(true);
            m_BGMOffImg.gameObject.SetActive(false);
            SoundManager.PlayMusic(SoundDefine.k_Intro_Music);
        }
        else
        {
            m_BGMOnImg.gameObject.SetActive(false);
            m_BGMOffImg.gameObject.SetActive(true);
            SoundManager.StopMusic();
        }
        PlayerManager.Instance.SaveGame();
    }

    public void ChangeNotification(Toggle toggle)
    {
       if(toggle.isOn)
        {

        }
       else
        {

        }
        PlayerManager.Instance.SaveGame();
    }
}
