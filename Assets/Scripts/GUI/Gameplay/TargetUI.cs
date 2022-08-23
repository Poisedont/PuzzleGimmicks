using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetUI : MonoBehaviour
{
    [SerializeField] List<Sprite> m_listChipUI;
    [SerializeField] Sprite m_listColorBooksUI;
    [SerializeField] Sprite m_StoneUI;
    [SerializeField] Sprite m_KeyDropUI;
    [SerializeField] Sprite m_Compass;
    [SerializeField] Sprite m_crystal;
    [SerializeField] Sprite m_smokePot;
    [SerializeField] Sprite m_Book;
    [SerializeField] Sprite m_smoke;
    [SerializeField] Sprite m_Cage;
    [SerializeField] Sprite m_scroll;
    [SerializeField] Sprite m_bufterfly;
    [SerializeField] Sprite m_curtain;
    [SerializeField] Sprite m_metal;
    [SerializeField] Sprite m_ice;
    [SerializeField] Sprite m_randomChanger;
    [SerializeField] Image m_icon;
    [SerializeField] Text m_number;
    [SerializeField] Image m_passedTarget;
    // Start is called before the first frame update
    private LevelTarget m_lvlTarget;
    private int m_index;

    private int lastNumberTarget;

    void Start()
    {

    }

    public LevelTarget GetLvlTarget()
    {
        return m_lvlTarget;
    }

    public int GetIndexTaret()
    {
        return m_index;
    }

    // Update is called once per frame
    void Update()
    {
        if (Session.Instance)
        {
            int numCountDown = m_lvlTarget.GetTargetCount(m_index) - m_lvlTarget.GetCurrentCount(m_index);

            if (numCountDown > 0)
            {
                m_number.gameObject.SetActive(true);
                m_passedTarget.gameObject.SetActive(false);
                m_number.text = numCountDown.ToString();
            }
            else
            {
                m_number.gameObject.SetActive(false);
                m_passedTarget.gameObject.SetActive(true);
            }

            if (lastNumberTarget != m_lvlTarget.GetCurrentCount(m_index) && lastNumberTarget < m_lvlTarget.GetTargetCount(m_index))
            {
                Animation anim = transform.GetComponent<Animation>();
                if (anim)
                    anim.Play("CollectTarget");
                lastNumberTarget = m_lvlTarget.GetCurrentCount(m_index);
            }
        }
    }



    //public void UpdateIcon(Sprite icon)
    //{
    //    m_icon.sprite = icon;
    //}

    //public void UpdateCount(int num)
    //{
    //    m_number.text = num.ToString() ;
    //}

    public void SetLevelTarget(LevelTarget target, int indexColor)
    {
        m_lvlTarget = target;
        m_index = indexColor;
        switch (target.GetTarget())
        {
            case FieldTarget.None:
                //Session.Instance.GetCurrentCountOfTarget(FieldTarget.None);
                break;
            case FieldTarget.Color:
                m_icon.sprite = m_listChipUI[indexColor];
                break;
            case FieldTarget.Stone:
                m_icon.sprite = m_StoneUI;
                break;
            case FieldTarget.FixBlock:
                m_icon.sprite = m_Book;
                break;
            case FieldTarget.Smoke:
                m_icon.sprite = m_smoke;
                break;
            case FieldTarget.KeyDrop:
                m_icon.sprite = m_KeyDropUI;
                break;
            case FieldTarget.Blocker:
                if (target.GetTargetCount((int)BlockerTargetType.Compass) > 0 && m_index == (int)BlockerTargetType.Compass)
                    m_icon.sprite = m_Compass;
                else if (target.GetTargetCount((int)BlockerTargetType.Crystal) > 0 && m_index == (int)BlockerTargetType.Crystal)
                    m_icon.sprite = m_crystal;
                else if (target.GetTargetCount((int)BlockerTargetType.SmokePot) > 0 && m_index == (int)BlockerTargetType.SmokePot)
                    m_icon.sprite = m_smokePot;
                break;
            case FieldTarget.Cage:
                m_icon.sprite = m_Cage;
                break;
            case FieldTarget.ColorBlocker:
                m_icon.sprite = m_listColorBooksUI;
                break;
            case FieldTarget.MagicScroll:
                m_icon.sprite = m_scroll;
                break;
            case FieldTarget.Butterfly:
                m_icon.sprite = m_bufterfly;
                break;
            case FieldTarget.Curtain:
                m_icon.sprite = m_curtain;
                break;
            case FieldTarget.MetalBrick:
                m_icon.sprite = m_metal;
                break;
            case FieldTarget.IceBrick:
                m_icon.sprite = m_ice;
                break;
            case FieldTarget.RandomChanger:
                m_icon.sprite = m_randomChanger;
                break;
        }

        lastNumberTarget = m_lvlTarget.GetCurrentCount(m_index);

    }

    public void UpdateLevelTarget(LevelTarget target)
    {
        m_lvlTarget = target;
    }
}
