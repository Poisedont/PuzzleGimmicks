using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [SerializeField] Text m_title;
    [SerializeField] TargetUI m_targetUI;

    [SerializeField] List<TargetUI> m_listTarget { get; set; }

    public static UnityEvent m_callSetTarget;
    // Start is called before the first frame update
    void Start()
    {
        m_callSetTarget = new UnityEvent();
        m_callSetTarget.AddListener(SetTargetPanel);
        m_listTarget = new List<TargetUI>();
        //SetTargetPanel();
    }

    void SetTargetPanel()
    {
        m_title.text = "Level " + (LevelProfile.main.level);

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
            }
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

    public void RetryBtnClick()
    {
        gameObject.SetActive(false);
        FieldManager.Instance.StartLevel();
    }
}
