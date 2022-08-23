using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    [SerializeField] Text m_levelTxt;
    [SerializeField] Button m_levelButton;
    [SerializeField] Image[] stars = new Image[3];
    [SerializeField] Sprite[] buttonSprite = new Sprite[2];
    [SerializeField] Animator m_animator;

    [SerializeField] GameObject m_NotEnoughStarPopup;
    private int m_level = 0;
    // Start is called before the first frame update
    void Start()
    {
        //m_level = 0;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetLevel(int pLevel)
    {
        m_level = pLevel;
        m_levelTxt.text = (m_level + 1).ToString();

        string file = "Levels/" + (m_level+1);
        TextAsset text = Resources.Load(file) as TextAsset;
 
        if ((m_level <= PlayerManager.Instance.m_currentLevel || PlayerManager.Instance.m_unlockAllLevel)&& text)
        {
            m_levelButton.GetComponent<Image>().sprite = buttonSprite[1]; //Button on
            m_levelButton.interactable = true;
        }
        else
        {
            m_levelButton.GetComponent<Image>().sprite = buttonSprite[0]; //Button off
            m_levelButton.interactable = false;
        }

        if(m_level == PlayerManager.Instance.m_currentLevel)
        {
            m_animator.Play("CurrentLevelAmin");
        }
        else
        {
            m_animator.Play("Idle");
        }

        int numberStars = PlayerManager.Instance.m_levelResultStarsArray[pLevel];
        if (numberStars > 0)
        {
            for(int i = 0; i< numberStars; i++)
            {
                stars[i].gameObject.SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].gameObject.SetActive(false);
            }
        }
    }

    public int GetLevel()
    {
        return m_level;
    }

    public void LevelBtnCLick()
    {
        if (PlayerManager.Instance.m_PlayerLive > 0)
        {
            SoundManager.StopMusic();
            PlayerManager.Instance.m_levelSelected = GetLevel();
            SceneManager.LoadScene("Game");
        }
        else
        {
            m_NotEnoughStarPopup.SetActive(true);
        }
    }

    public void Unlock()
    {
        string file = "Levels/" + m_level;
        TextAsset text = Resources.Load(file) as TextAsset;
        if (text)
        {
            m_levelButton.GetComponent<Image>().sprite = buttonSprite[1]; //Button on
            m_levelButton.interactable = true;
        }
    }
}
