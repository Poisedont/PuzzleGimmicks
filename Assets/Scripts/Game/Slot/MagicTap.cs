using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicTap : IBlock
{
    [SerializeField] public GameObject m_container;
    [SerializeField] GameObject[] m_magicOrbs; // 8 orb for tap
    [SerializeField] Animation m_anim;
    [SerializeField] string m_fullOrbAnim = "magicTap_fullOrb";
    private int eventCountBorn;

    MagicTapGroup m_myGroup;

    public override void BlockCrush(bool force)
    {
        if (eventCountBorn == Session.Instance.eventCount && !force) return;
        if (m_myGroup != null)
        {
            m_myGroup.OnChildCrush();
            eventCountBorn = Session.Instance.eventCount;
        }
    }

    public override bool CanBeCrushedByNearSlot(Chip near = null)
    {
        return true;
    }

    public override bool CanItContainChip()
    {
        return false;
    }

    public override int GetLevels()
    {
        return 1;
    }

    public override void Initialize(SlotSettings settings = null)
    {

        if (settings.bookShelfInfo != null)
        {
            int shelfIndex = settings.bookShelfInfo.Index;
            if (m_container)
            {
                m_container.SetActive(shelfIndex == 0);
            }
            if (shelfIndex == 0)
            {
                m_myGroup = new MagicTapGroup(this);
            }
        }
    }

    public override bool IsCastShadow()
    {
        return base.IsCastShadow();
    }

    ////////////////////////////////////////////////////////////////////////////////
    public void SetGroup(MagicTapGroup group)
    {
        m_myGroup = group;
    }

    public void ActiveOrb(int orbId)
    {
        if (orbId >= 0 && orbId < m_magicOrbs.Length)
        {
            m_magicOrbs[orbId].SetActive(true);
        }
    }

    public float FullOrbCollected()
    {
        if (m_anim)
        {
            m_anim.Play(m_fullOrbAnim);
            return m_anim.GetClip(m_fullOrbAnim).length;
        }
        return 0;
    }
}

[System.Serializable]
public class MagicTapGroup
{
    static List<MagicTapGroup> s_allGroups;
    MagicTap[] m_magicTapSlots;
    private int eventCountBorn;
    private int m_collectCount;


    public MagicTapGroup(MagicTap slot0)
    {
        if (s_allGroups == null)
        {
            s_allGroups = new List<MagicTapGroup>();
        }
        s_allGroups.Add(this);
        m_magicTapSlots = new MagicTap[GameConst.k_blocker_group_slot_count];
        m_magicTapSlots[0] = slot0;
        m_collectCount = 0;

    }

    public bool AddToGroup(MagicTap item, int index)
    {
        if (index > 0 && index < m_magicTapSlots.Length)
        {
            m_magicTapSlots[index] = item;
            item.SetGroup(this);
            return true;
        }

        return false;
    }

    public static void Initialize()
    {
        if (s_allGroups != null)
        {
            foreach (var group in s_allGroups)
            {
                group.InitGroup();
            }
        }
    }

    public static void Cleanup()
    {
        if (s_allGroups != null)
        {
            s_allGroups.Clear();
        }
    }

    static Side[] s_bookSides = new Side[] { Side.Right, Side.Top, Side.TopRight };

    void InitGroup()
    {
        /* 
        | 2 | 3 |
        | 0 | 1 |
        */
        if (m_magicTapSlots[0])
        {
            Vector2Int coord = m_magicTapSlots[0].slot.coord;
            Slot slot = null;
            for (int i = 0; i < s_bookSides.Length; i++)
            {
                slot = Slot.GetSlot(Utils.Vec2IntAdd(coord, s_bookSides[i]));

                if (slot)
                {
                    int shelfIndex = i + 1;
                    var item = slot.block as MagicTap;
                    if (item)
                    {
                        AddToGroup(item, shelfIndex);
                    }
                }
            }
        }
    }

    public void OnChildCrush()
    {
        if (eventCountBorn == Session.Instance.eventCount) return;
        if (m_collectCount == GameConst.k_magic_tap_max) return;

        m_collectCount++;
        eventCountBorn = Session.Instance.eventCount;
        m_magicTapSlots[0].ActiveOrb(m_collectCount - 1);
        if (m_collectCount == GameConst.k_magic_tap_max)
        {
            // destroy all effect
            m_magicTapSlots[0].StartCoroutine(ExplodeRoutine());
        }
    }

    IEnumerator ExplodeRoutine()
    {
        yield return null;
        float time = m_magicTapSlots[0].FullOrbCollected();
        yield return new WaitForSeconds(time*2/3);

        AnimationHelper.Instance.Explode(m_magicTapSlots[0].m_container.transform.position, 5, 20);

        yield return new WaitForSeconds(time / 6);
        FieldManager.Instance.DestroyAllChips();
        yield return new WaitForSeconds(time / 6);
        //destroy all slots
        foreach (var tap in m_magicTapSlots)
        {
            tap.slot.block = null;
            GameObject.Destroy(tap.gameObject);
        }

        SlotGravity.Reshading();
    }
}