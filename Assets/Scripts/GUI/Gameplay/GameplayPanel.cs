using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameplayPanel : MonoBehaviour
{
    //[SerializeField] GameObject m_IGM;
    [SerializeField] Text MoveCountDownTxt;
    [SerializeField] Image m_ScoreBar;
    [SerializeField] Text m_levelText;
    [SerializeField] TargetUI m_targetUI;

    [SerializeField] List<TargetUI> m_listTarget;
    [SerializeField] List<GameObject> m_listStars;
    [SerializeField] ParticleSystem m_finishEffects;
    [SerializeField] Animator m_finishAnim;
    [SerializeField] GameObject m_shuffPopup;

    public static UnityEvent m_eventSetTargetUI;

    public static UnityEvent m_eventPlayAnim;
    public static UnityEvent m_eventShuffAnim;
    // Start is called before the first frame update
    void Start()
    {
        SoundManager.PlayMusic(SoundDefine.k_Play_Music);
        m_listTarget = new List<TargetUI>();
        //SetTargetPanel();
        m_eventSetTargetUI = new UnityEvent();
        m_eventSetTargetUI.AddListener(SetTargetPanel);
        m_eventPlayAnim = new UnityEvent();
        m_eventPlayAnim.AddListener(PlayAnimFinish);
        m_eventShuffAnim = new UnityEvent();
        m_eventShuffAnim.AddListener(ShowShuffPopup);
    }

    void SetTargetPanel()
    {
        if (m_listTarget.Count > 0)
        {
            for (int i = 0; i < m_listTarget.Count; i++)
            {
                Destroy(m_listTarget[i].gameObject);
            }
            m_listTarget.Clear();
        }
        if (LevelProfile.main.allTargets.Count > 0)
        {
            for (int j = 0; j < LevelProfile.main.allTargets.Count; j++)
            {
                LevelTarget lvlTarget = LevelProfile.main.allTargets[j];
                //switch (lvlTarget.GetTarget())
                //{
                //    case FieldTarget.None:
                //        //Session.Instance.GetCurrentCountOfTarget(FieldTarget.None);
                //        break;
                //    case FieldTarget.Color:
                        //Session.Instance.GetCurrentCountOfTarget(FieldTarget.Color);
                        for (int i = 0; i < lvlTarget.GetNumberTargets(); i++)
                        {
                            int targetCount = lvlTarget.GetTargetCount(i);
                            if (targetCount > 0)
                            {
                                TargetUI targetUI = Instantiate(m_targetUI, m_targetUI.transform.position, Quaternion.identity, m_targetUI.transform.parent);
                                targetUI.gameObject.SetActive(true);
                                targetUI.SetLevelTarget(lvlTarget, i);
                                m_listTarget.Add(targetUI);
                            }
                        }
                        
                    //case FieldTarget.Stone:
                    //case FieldTarget.KeyDrop:
                    //    for (int i = 0; i < lvlTarget.GetNumberTargets(); i++)
                    //    {
                    //        int targetCount = lvlTarget.GetTargetCount(i);
                    //        if (targetCount > 0)
                    //        {

                    //            TargetUI targetUI = Instantiate(m_targetUI, m_targetUI.transform.position, Quaternion.identity, m_targetUI.transform.parent);
                    //            targetUI.gameObject.SetActive(true);
                    //            targetUI.SetLevelTarget(lvlTarget, i);
                    //            m_listTarget.Add(targetUI);
                    //        }
                    //    }
                    //    break;
                    //case FieldTarget.Block:
                    //    break;
                    //case FieldTarget.Smoke:
                    //    break;
                    
                //}
            }
        }
        m_levelText.text = "Level " + LevelProfile.main.level;

        float posStart = m_listStars[3].transform.position.x;
        float posEnd = m_listStars[2].transform.position.x;

        Vector3 starPos = m_listStars[0].transform.position;
        starPos.x = posStart + ((posEnd - posStart) * LevelProfile.main.firstStarScore / LevelProfile.main.thirdStarScore);
        m_listStars[0].transform.position = starPos;
        starPos = m_listStars[1].transform.position;
        starPos.x = posStart + ((posEnd - posStart) * LevelProfile.main.secondStarScore / LevelProfile.main.thirdStarScore);
        m_listStars[1].transform.position = starPos;

    }

    // Update is called once per frame
    void Update()
    {
        UpdateCountDownMove(Session.Instance.movesCount);
        float scoreBar = (Session.Instance.score * 1f) / LevelProfile.main.thirdStarScore;

        m_ScoreBar.fillAmount = scoreBar < 1f ? scoreBar : 1;

        if (Input.GetMouseButtonDown(0))
        {
            if(Session.Instance.reachedTheTarget && PlayerManager.Instance.m_canSkipCollapseAllPowerups && Session.Instance.score >= LevelProfile.main.thirdStarScore)
            {
                PlayerManager.Instance.m_skipCollapseAllPowerups = true;
            }
        }

        if(Session.Instance.score > LevelProfile.main.firstStarScore)
        {
            m_listStars[0].transform.FindChild("Image").gameObject.SetActive(true);
        }
        else
        {
            m_listStars[0].transform.FindChild("Image").gameObject.SetActive(false);
        }

        if (Session.Instance.score > LevelProfile.main.secondStarScore)
        {
            m_listStars[1].transform.FindChild("Image").gameObject.SetActive(true);
        }
        else
        {
            m_listStars[1].transform.FindChild("Image").gameObject.SetActive(false);
        }

        if (Session.Instance.score > LevelProfile.main.thirdStarScore)
        {
            m_listStars[2].transform.FindChild("Image").gameObject.SetActive(true);
        }
        else
        {
            m_listStars[2].transform.FindChild("Image").gameObject.SetActive(false);
        }
    }

    public void ShowIGM()
    {
        // m_IGM.gameObject.SetActive(true);
        if (!PopupControl.Instance.IsPopupShowing() && !Session.Instance.reachedTheTarget && !Session.Instance.gameForceOver)
        {
            PopupControl.Instance.OpenPopup("IGM");
        }
    }
    
    public void UpdateCountDownMove(int countMove)
    {
        MoveCountDownTxt.text = countMove.ToString();
    }

    public void PlayParticle()
    {
        m_finishEffects.Play();
    }

    public void PlayAnimFinish()
    {
        m_finishAnim.Play("FinishEffects", -1, 0f);
    }

    public void CheatWinLevel()
    {
        Session.Instance.reachedTheTarget = true;
        Session.Instance.gameForceOver = true;
    }

    public void CheatInfinityTools(Toggle enable)
    {
        PlayerManager.Instance.m_infinityTools = enable.isOn;
    }

    public void ShowShuffPopup()
    {
        m_shuffPopup.SetActive(true);
        m_eventShuffAnim.RemoveAllListeners();
        m_eventShuffAnim.AddListener(HideShuffPopup);
    }

    public void HideShuffPopup()
    {
        m_shuffPopup.SetActive(false);
        m_eventShuffAnim.RemoveAllListeners();
        m_eventShuffAnim.AddListener(ShowShuffPopup);
    }
}
