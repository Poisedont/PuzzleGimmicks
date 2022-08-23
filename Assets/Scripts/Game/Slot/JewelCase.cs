using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JewelCase : IBlock
{
    [SerializeField] Chip.EChipType m_Color;
    [SerializeField] Animator animator;

    ////////////////////////////////////////////////////////////////////////////////
    int m_eventCountBorn;
    int m_lastEventCheck;
    int m_orbCount;
    const int k_max_count = 3;
    ////////////////////////////////////////////////////////////////////////////////

    public override void BlockCrush(bool force)
    {
        if (m_eventCountBorn == Session.Instance.eventCount && !force) return;
        m_orbCount++;

        m_eventCountBorn = Session.Instance.eventCount;

        if (animator)
        {
            animator.SetInteger("orb", m_orbCount);
        }
        if (m_orbCount >= k_max_count)
        {
            StartCoroutine(CrushAllColor());
        }
    }

    IEnumerator CrushAllColor()
    {
        yield return null;

        List<Chip> colorChips = new List<Chip>();
        foreach (var slot in Slot.all.Values)
        {
            if (slot.chip)
            {
                if (slot.chip.IsColored() && slot.chip.id == (int)m_Color && !slot.chip.destroying)
                {
                    colorChips.Add(slot.chip);
                }
            }
        }

        var fireStart = ContentManager.Instance.GetItem("JewelcaseFire");
        if (fireStart)
        {
            var moveStart = fireStart.gameObject.GetComponent<MoveToTarget>();
            moveStart.transform.position = this.slot.transform.position;
            if (moveStart)
            {
                moveStart.m_targetPosition = Camera.main.transform.position;
                moveStart.UpdateRenderColor(Chip.colors[(int)m_Color]);
                moveStart.BeginMove();
            }
        }

        yield return new WaitForSeconds(1f);
        Destroy(fireStart.gameObject);

        foreach (var chip in colorChips)
        {
            var fire = ContentManager.Instance.GetItem("JewelcaseFire");
            if (fire)
            {
                var move = fire.gameObject.GetComponent<MoveToTarget>();
                move.transform.position = Camera.main.transform.position;// this.slot.transform.position;
                if (move)
                {
                    move.m_targetPosition = chip.transform.position;
                    move.UpdateRenderColor(Chip.colors[(int)m_Color]);
                    move.BeginMove();
                }
            }
            chip.IsBusy = true;
        }

        yield return new WaitForSeconds(1f);
        // kill all chips
        foreach (var chip in colorChips)
        {
            chip.DestroyChip();
        }

        this.slot.block = null;
        SlotGravity.Reshading();
        Destroy(gameObject);
    }

    public override bool CanBeCrushedByNearSlot(Chip near = null)
    {
        return false;
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
        m_eventCountBorn = Session.Instance.eventCount;
        m_lastEventCheck = Session.Instance.eventCount;
        m_orbCount = 0;
    }
    ////////////////////////////////////////////////////////////////////////////////
    float m_timeTocheck = 0f;
    private void Update()
    {
        if (m_lastEventCheck < Session.Instance.eventCount)
        {
            // wait for next anim
            m_timeTocheck += Time.deltaTime;
            if (m_timeTocheck > 0.3f)
            {
                m_timeTocheck = 0;

                // NOTE: default checking side is TOP
                Slot neighborSlot = Slot.GetSlot(Utils.Vec2IntAdd(this.slot.coord, Side.Top));
                if (neighborSlot)
                {
                    if (neighborSlot.chip
                        && !neighborSlot.chip.destroying
                        && !neighborSlot.chip.IsBusy
                        && neighborSlot.chip.transform.localPosition == Vector3.zero)
                    {
                        if (neighborSlot.chip.id == (int)this.m_Color)
                        {
                            neighborSlot.chip.HideChip(false);

                            BlockCrush(true);
                        }

                        m_lastEventCheck = Session.Instance.eventCount;
                    }
                }
            }
        }
    }
}