using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceLineBomb : MonoBehaviour
{
    Chip m_chip;
    public Chip chip { get { return m_chip; } }

    [Tooltip("ICE chip type: should only be 1 in 2 Ice bomb types")]
    [SerializeField] EPieces m_chipType;
    [SerializeField] SpriteRenderer m_spriteRender;
    [SerializeField] SpriteRenderer m_iceRender;
    [SerializeField] Sprite[] m_iceSprites;

    int m_level;
    int m_eventBirth;
    CrossBomb.EBombType m_bombType;
    CrossBomb m_bomb;
    ////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        m_bomb = GetComponent<CrossBomb>();

        m_chip = GetComponent<Chip>();
        int lv = m_chipType - EPieces.HalfIceLineBomb + 1;
        if (lv > 0 && lv <= 2)
        {
            m_level = lv;
        }
        else
        {
            Debug.LogWarning("set Ice chip type wrong!!!");
        }

        m_eventBirth = Session.Instance.eventCount;
        m_bombType = Random.value > 0.5f ? CrossBomb.EBombType.HLineBomb : CrossBomb.EBombType.VLineBomb;
        if (m_spriteRender)
        {
            if (m_bombType == CrossBomb.EBombType.HLineBomb)
                m_spriteRender.transform.rotation = Quaternion.AngleAxis(90, Vector3.forward);
            else
                m_spriteRender.transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
        }

        if (m_bomb)
        {
            m_bomb.ForceBombType = m_bombType;
        }
    }
    private void Start()
    {
        if (m_bomb)
        {
            m_bomb.chip.chipType = "IceBomb"; // prevent mix with other
        }
    }
    public IEnumerator Destroying()
    {
        if (m_eventBirth == Session.Instance.eventCount)
        {
            chip.destroying = false;
            yield break;
        }

        m_eventBirth = Session.Instance.eventCount;

        m_level--;

        if (m_level >= 0)
        {
            chip.destroying = false;
            chip.IsBusy = true;

            GameObject Obj = ContentManager.Instance.GetItem("IceDropEffects");

            ParticleSystem iceEffects = Obj.GetComponent<ParticleSystem>();
            Obj.transform.position = transform.position;
            iceEffects.Play();

            if (m_iceRender)
            {
                if (m_level == 0)
                {
                    m_iceRender.gameObject.SetActive(false);
                    Session.Instance.CollectIce();
                    if (m_bomb)
                    {
                        m_bomb.IsIce = false; //disable ice on main bomb
                        m_bomb.chip.chipType = m_bombType.ToString();
                        enabled = false;
                    }
                }
                else
                {
                    m_iceRender.sprite = m_iceSprites[m_level - 1];
                }
            }
            // GameObject Obj = ContentManager.Instance.GetItem("IceDropEffects");

            // ParticleSystem iceEffects = Obj.GetComponent<ParticleSystem>();
            // Obj.transform.position = transform.position;
            // iceEffects.Play();


            // yield return new WaitForSeconds(iceEffects.duration);
            // Destroy(Obj);
            //yield return m_waitForIceBreak; // wait for change color

            chip.IsBusy = false;
        }
        else
        {
            //destroy like a bomb
        }
    }

}