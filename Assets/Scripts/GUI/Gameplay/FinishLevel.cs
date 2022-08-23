using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinishLevel : MonoBehaviour
{

    [SerializeField] Text m_level;
    [SerializeField] List<Image> m_listStars;
    [SerializeField] Text m_score;
    [SerializeField] Text m_chestNumber;
    [SerializeField] Animation m_starsAnim;

    public static UnityEvent m_updateUIEvent;
    // Start is called before the first frame update
    void Start()
    {
        m_updateUIEvent = new UnityEvent();
        m_updateUIEvent.AddListener(UpdateUI);
        m_starsAnim = GetComponent<Animation>();
    }

    void UpdateUI()
    {
        if (PlayerManager.Instance)
        {
            PlayerManager.Instance.m_canSkipCollapseAllPowerups = false;
            PlayerManager.Instance.m_skipCollapseAllPowerups = false;
        }
        m_level.text = "Level " + (LevelProfile.main.level);
        m_score.text = Session.Instance.score.ToString();

        int chestNumCount = 0;// PlayerManager.Instance.m_currentLevel % 20;
        m_chestNumber.text = chestNumCount + "/20";

        m_listStars[0].gameObject.SetActive(false);
        m_listStars[1].gameObject.SetActive(false);
        m_listStars[2].gameObject.SetActive(false);

        StartCoroutine(ShowStars());
    }

    IEnumerator ShowStars()
    {

        if (Session.Instance.score > LevelProfile.main.firstStarScore)
        {
            m_listStars[0].gameObject.SetActive(true);
            m_starsAnim.Play("Star1FinishLevel");
        }

        while (m_starsAnim.isPlaying)
        {
            yield return 0;
        }

        if (Session.Instance.score > LevelProfile.main.secondStarScore)
        {
            m_listStars[1].gameObject.SetActive(true);
            m_starsAnim.Play("Star2FinishLevel");
            
        }

        while (m_starsAnim.isPlaying)
        {
            yield return 0;
        }

        if (Session.Instance.score > LevelProfile.main.thirdStarScore)
        {
            m_listStars[2].gameObject.SetActive(true);
            m_starsAnim.Play("Star3FinishLevel");
           
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ExitBtnClick()
    {
        SoundManager.StopMusic();
        SceneManager.LoadScene("Menu");
    }

    public void NextBtnClick()
    {
        gameObject.SetActive(false);
        PlayerManager.Instance.m_levelSelected++;
        FieldManager.Instance.StartLevel();
    }
}
