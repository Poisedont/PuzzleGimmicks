using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShowTargetStartLevel : MonoBehaviour
{
    [SerializeField] TargetUI m_targetUI;
    [SerializeField] Animator m_anim;

    List<TargetUI> m_listTarget { get; set; }
    public static UnityEvent m_callSetTarget;
    // Start is called before the first frame update
    void Start()
    {
        m_callSetTarget = new UnityEvent();
        m_callSetTarget.AddListener(SetTargetPanel);
        m_listTarget = new List<TargetUI>();
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

        m_anim.Play("ShowTargetStartLevelAnim", -1, 0f);
    }

    public void HidePopupShowTarget()
    {
        gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
