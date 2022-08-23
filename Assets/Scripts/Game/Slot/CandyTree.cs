using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyTree : IBlock
{
    [SerializeField] SpriteRenderer m_renderer;
    [SerializeField] GameObject[] m_candies;
    [SerializeField] internal float m_animDuration = 1f;
    [SerializeField] GameObject m_particle;


    ////////////////////////////////////////////////////////////////////////////////
    private int eventCountBorn;
    CandyTreeGroup m_myGroup;

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
        eventCountBorn = Session.Instance.eventCount;
        if (settings.bookShelfInfo != null)
        {
            int shelfIndex = settings.bookShelfInfo.Index;
            if (m_renderer)
            {
                m_renderer.gameObject.SetActive(shelfIndex == 0);
            }
            if (shelfIndex == 0)
            {
                m_myGroup = new CandyTreeGroup(this);
                HideAllCandies();
            }
        }
    }

    public void SetGroup(CandyTreeGroup group)
    {
        m_myGroup = group;
    }

    public void ActiveCandy(int idx)
    {
        if (m_candies != null && idx < m_candies.Length)
        {
            m_candies[idx].SetActive(true);
            if (m_particle)
            {
                m_particle.SetActive(true);
            }
        }
    }

    internal void HideAllCandies()
    {
        if (m_candies != null)
        {
            foreach (var item in m_candies)
            {
                item.SetActive(false);
            }
        }
    }
}

public class CandyTreeGroup
{
    static List<CandyTreeGroup> s_allGroups;
    private int eventCountBorn;
    private int m_collectCount;
    CandyTree[] m_treeSlots;

    static Side[] s_groupSides = new Side[] { Side.Right, Side.Top, Side.TopRight };
    public CandyTreeGroup(CandyTree tile)
    {
        if (s_allGroups == null)
        {
            s_allGroups = new List<CandyTreeGroup>();
        }
        s_allGroups.Add(this);
        if (m_treeSlots == null)
        {
            m_treeSlots = new CandyTree[GameConst.k_blocker_group_slot_count];
        }

        m_treeSlots[0] = tile;
        m_collectCount = 0;
    }

    public bool AddToGroup(CandyTree item, int index)
    {
        if (index > 0 && index < m_treeSlots.Length)
        {
            m_treeSlots[index] = item;
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

    void InitGroup()
    {
        /* 
        | 2 | 3 |
        | 0 | 1 |
        */
        if (m_treeSlots[0])
        {
            Vector2Int coord = m_treeSlots[0].slot.coord;
            Slot slot = null;
            for (int i = 0; i < s_groupSides.Length; i++)
            {
                slot = Slot.GetSlot(Utils.Vec2IntAdd(coord, s_groupSides[i]));

                if (slot)
                {
                    int index = i + 1;
                    var block = slot.block as CandyTree;
                    if (block)
                    {
                        AddToGroup(block, index);
                    }
                }
            }
        }
    }

    internal void OnChildCrush()
    {
        if (eventCountBorn == Session.Instance.eventCount) return;
        if (m_collectCount == GameConst.k_candy_tree_max) return;

        m_collectCount++;
        eventCountBorn = Session.Instance.eventCount;

        m_treeSlots[0].ActiveCandy(m_collectCount - 1);
        if (m_collectCount == GameConst.k_candy_tree_max)
        {
            m_collectCount = 0;
            m_treeSlots[0].HideAllCandies();
            // generate 4 pieces
            m_treeSlots[0].StartCoroutine(GeneratePieces(m_treeSlots[0].m_animDuration));
        }
    }
    IEnumerator GeneratePieces(float duration)
    {
        // get all slots that have no blocker
        List<Slot> possibleSlots = new List<Slot>();
        foreach (var slot in Slot.all.Values)
        {
            if (slot.block) continue;
            if (slot.chip)
            {
                if (!slot.chip.logic.IsMatchable()) continue;
                string chipType = slot.chip.logic.GetChipType();
                if (chipType == "Portion") continue;
            }

            possibleSlots.Add(slot);
        }

        // generate 4 chips
        Chip[] newChips = new Chip[GameConst.k_candy_tree_max];
        int colorID = LevelProfile.main.GetColorRandom();

        Slot[] targetSlots = new Slot[GameConst.k_candy_tree_max];
        Slot slot1 = null;
        for (int i = 0; i < GameConst.k_candy_tree_max; i++)
        {
            GameObject o = ContentManager.Instance.GetItem("SimpleChip" + Chip.chipTypes[colorID]);
            o.transform.position = m_treeSlots[0].transform.position;
            o.name = "Chip_" + Chip.chipTypes[colorID];
            newChips[i] = o.GetComponent<Chip>();
            newChips[i].transform.FindChild("icon").GetComponent<SpriteRenderer>().sortingOrder = 10;
            newChips[i].IsBusy = true;

            // find target slot
            slot1 = Utils.GetRandom(possibleSlots);
            targetSlots[i] = slot1;
            possibleSlots.Remove(slot1);

            if (targetSlots[i].chip)
            {
                targetSlots[i].chip.IsBusy = true; // make it busy to prevent drop
            }
        }

        float time = 0;
        float progress = 0;
        float animLeng = duration / 2; // for 2 phases
        Vector3 startScale = Vector3.one;
        Vector3 targetScale = Vector3.one * 2;
        Vector3 startPosition = m_treeSlots[0].transform.position;

        //fly to target and scale up
        while (progress < animLeng)
        {
            time = EasingFunctions.easeInOutQuad(progress / animLeng);
            for (int i = 0; i < newChips.Length; i++)
            {
                newChips[i].transform.localScale = Vector3.Lerp(startScale, targetScale, time);
                newChips[i].transform.position = Vector3.Lerp(startPosition, targetSlots[i].transform.position, time);
            }
            progress += Time.deltaTime;
            yield return 0;
        }

        yield return 0;

        // scale down
        while (progress > 0)
        {
            time = EasingFunctions.easeInOutQuad(progress / animLeng);
            for (int i = 0; i < newChips.Length; i++)
            {
                newChips[i].transform.localScale = Vector3.Lerp(startScale, targetScale, time);
            }
            progress -= Time.deltaTime;
            yield return 0;
        }

        // update slot with new chip
        for (int i = 0; i < GameConst.k_candy_tree_max; i++)
        {
            if (targetSlots[i].chip)
            {
                targetSlots[i].chip.IsBusy = false;
                targetSlots[i].chip.transform.FindChild("icon").gameObject.SetActive(false);
                targetSlots[i].chip.HideChip(false);
            }
            newChips[i].transform.position = targetSlots[i].transform.position;
            newChips[i].transform.localScale = Vector3.one;

            targetSlots[i].chip = newChips[i];
            newChips[i].IsBusy = false;
            newChips[i].transform.FindChild("icon").GetComponent<SpriteRenderer>().sortingOrder = 0;
            newChips[i].movementID = Session.Instance.GetMovementID();
        }
        yield return 0;
    }
}