using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// Class button activation booster
[RequireComponent(typeof(Button))]
public class BoosterButton : MonoBehaviour
{

    // booster type
    public static string boosterSelectedId = "";
    public string logic_content;

    public string boosterItemId;

    public Text boosterNumber;
    // Mask of displaying booster depending on the limitation mode
    public Limitation[] limitationMask;

    Button button;

    void Awake()
    {
        SendMessage("BoosterInitialize", SendMessageOptions.DontRequireReceiver);
        button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            OnClick();
        });


    }

    private void Update()
    {
        if (PlayerManager.Instance)
        {
            if (boosterItemId.Contains("HammerHit"))
            {
                boosterNumber.text = PlayerManager.Instance.m_boosterHammerCount.ToString();
            }
            else if (boosterItemId.Contains("Cross"))
            {
                boosterNumber.text = PlayerManager.Instance.m_boosterCrossCount.ToString();
            }
            else if (boosterItemId.Contains("Rainbow"))
            {
                boosterNumber.text = PlayerManager.Instance.m_boosterRainbowCount.ToString();
            }
        }
    }

    void OnClick()
    {
        if (!Session.Instance.CanIWait())
            return;
        if (PlayerManager.Instance)
        {
            if ((((boosterItemId.Contains("HammerHit") && PlayerManager.Instance.m_boosterHammerCount == 0)
                || (boosterItemId.Contains("Cross") && PlayerManager.Instance.m_boosterCrossCount == 0)
                || (boosterItemId.Contains("Rainbow") && PlayerManager.Instance.m_boosterRainbowCount == 0))
            && !PlayerManager.Instance.m_infinityTools)
            || Session.Instance.reachedTheTarget
            )
            {
                return;
            }
        }
        boosterSelectedId = boosterItemId;
        ContentManager.Instance.GetItem(logic_content);
    }
}
