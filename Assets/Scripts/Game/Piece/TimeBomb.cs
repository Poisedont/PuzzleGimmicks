using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeBomb : MonoBehaviour, IChipLogic
{
    protected Chip m_chip;
    int m_eventBirth;
    [SerializeField] Text m_timeText;
    [SerializeField] int m_defaultTime;

    int m_totalTime;

    public Chip chip
    {
        get { return m_chip; }
    }
    ////////////////////////////////////////////////////////////////////////////////
    public IEnumerator Destroying()
    {
        if (Session.Instance.eventCount == m_eventBirth)
        {
            chip.destroying = false;
            yield break;
        }

        chip.SetScore(1);

        

        GameObject o = ContentManager.Instance.GetItem("SmokeInSlot");
        o.transform.position = transform.position;
        ParticleSystem effect = o.GetComponent<ParticleSystem>();
        effect.Play();
        Session.Instance.PostSwapAction -= PostSwapAction;
        gameObject.SetActive(false);
        yield return new WaitForSeconds(effect.duration);
        chip.ParentRemove();
        effect.Stop();
        Destroy(o.gameObject);
    }

    public string GetChipType()
    {
        return EPieces.TimeBomb.ToString();
    }

    public List<Chip> GetDangeredChips(List<Chip> stack)
    {
        stack.Add(chip);
        return stack;
    }

    public int GetPotencial()
    {
        return 1 + (Session.Instance.swapEvent - m_eventBirth) * 2;
    }

    public bool IsMatchable()
    {
        return true;
    }

    ////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        m_chip = GetComponent<Chip>();
        m_totalTime = m_defaultTime;
        m_eventBirth = Session.Instance.swapEvent;

        Session.Instance.PostSwapAction += PostSwapAction;
    }

    private void Start()
    {
        if (m_timeText)
        {
            m_timeText.text = m_totalTime.ToString();
        }
    }

    void PostSwapAction()
    {
        int count = Session.Instance.swapEvent - m_eventBirth;
        if (count >= m_totalTime)
        {
            //chip.DestroyChip();

            StartCoroutine(ExplodeTimeBomb());
        }
        else
        {
            if (m_timeText)
            {
                m_timeText.text = (m_totalTime - count).ToString();
            }
        }
    }

    IEnumerator ExplodeTimeBomb()
    {
        GameObject o = ContentManager.Instance.GetItem("ExplodeTImeBomb");
        o.transform.position = transform.position;
        ParticleSystem effect = o.GetComponent<ParticleSystem>();
        effect.Play();
        //gameObject.SetActive(false);
        yield return new WaitForSeconds(effect.duration);
        effect.Stop();
        Destroy(o);

        // game over by timeBomb
        Debug.Log("TIMEBOMB: Game over");
        Session.Instance.gameForceOver = true;
    }

    private void OnDestroy()
    {
        Session.Instance.PostSwapAction -= PostSwapAction;
    }
}