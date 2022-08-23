using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Group4Level : MonoBehaviour
{

    [SerializeField] LevelUI m_levelUI_1;
    [SerializeField] LevelUI m_levelUI_2;
    [SerializeField] LevelUI m_levelUI_3;
    [SerializeField] LevelUI m_levelUI_4;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupLevel(int line)
    {
        if (line * 4 <= ProfileConfig.k_maxLevel)
        {
            m_levelUI_1.gameObject.SetActive(true);
            m_levelUI_1.SetLevel(line * 4);
        }
        else
        {
            m_levelUI_1.gameObject.SetActive(false);
        }

        if (line * 4 + 1 <= ProfileConfig.k_maxLevel)
        {
            m_levelUI_2.gameObject.SetActive(true);
            m_levelUI_2.SetLevel(line * 4 + 1);
        }
        else
        {
            m_levelUI_2.gameObject.SetActive(false);
        }

        if (line * 4 + 2 <= ProfileConfig.k_maxLevel)
        {
            m_levelUI_3.gameObject.SetActive(true);
            m_levelUI_3.SetLevel(line * 4 + 2);
        }
        else
        {
            m_levelUI_3.gameObject.SetActive(false);
        }

        if (line * 4 + 3 <= ProfileConfig.k_maxLevel)
        {
            m_levelUI_4.gameObject.SetActive(true);
            m_levelUI_4.SetLevel(line * 4 + 3);
        }
        else
        {
            m_levelUI_4.gameObject.SetActive(false);
        }

    }
}
