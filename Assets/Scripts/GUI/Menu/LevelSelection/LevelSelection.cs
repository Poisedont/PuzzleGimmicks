using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelection : MonoBehaviour
{
    [SerializeField] LevelUI m_levelUI;
    [SerializeField] GameObject m_SettingPopup;
    [SerializeField] Text m_coinTxt;
    [SerializeField] Text m_liveTxt;
    [SerializeField] Text m_timeCountForNextLife;

    private List<LevelUI> m_listAllLevel;

    private DateTime m_checkUpdate = new DateTime();
    // Start is called before the first frame update
    void Start()
    {
        m_listAllLevel = new List<LevelUI>();
        SoundManager.PlayMusic(SoundDefine.k_Intro_Music);
        //Test
        //for(int i = 0; i< 1000; i++)
        //{
        //   LevelUI level = Instantiate(m_levelUI, m_levelUI.transform.parent, false);
        //   level.gameObject.SetActive(true);
        //   level.SetLevel(i);
        //   m_listAllLevel.Add(level);
        //}
        
    }

    // Update is called once per frame
    void Update()
    {
        if((DateTime.Now - m_checkUpdate).Seconds >= 1)
        {
            m_checkUpdate = DateTime.Now;
            UpdateTopBar();
        }
    }

    public void UpdateTopBar()
    {
        if (PlayerManager.Instance.m_PlayerLive < ProfileConfig.k_lifePlayerLimit && DateTime.Now < PlayerManager.Instance.m_nextLifeTime)
        {
            TimeSpan timeRemain = PlayerManager.Instance.m_nextLifeTime - DateTime.Now;
            m_timeCountForNextLife.text = (int)(timeRemain.TotalSeconds / 60) + ":" + (int)(timeRemain.TotalSeconds % 60);
        }
        else
        {
            m_timeCountForNextLife.text = "FULL";
        }
        m_coinTxt.text = PlayerManager.Instance.m_PlayerCoin.ToString();
        m_liveTxt.text = PlayerManager.Instance.m_PlayerLive.ToString();
    }

    public void SettingBtnClick()
    {
        m_SettingPopup.SetActive(true);
    }

    public void AddLive()
    {
        PlayerManager.Instance.m_PlayerLive++;

    }

    public void UnlockAlllevel()
    {
        PlayerManager.Instance.m_unlockAllLevel = !PlayerManager.Instance.m_unlockAllLevel;
        Mosframe.DynamicScrollView.refreshEvent.Invoke();
        for (int i = 0; i< m_listAllLevel.Count; i++)
        {
            m_listAllLevel[i].Unlock();
        }
        
    }
}
