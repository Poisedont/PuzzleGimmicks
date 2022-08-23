using System.Collections;
using UnityEngine;

public class JewelMold : IBlock
{
    [SerializeField] Chip.EChipType m_Color;
    int m_eventCountBorn;
    int m_lastEventCheck;
    ////////////////////////////////////////////////////////////////////////////////
    
    public override void BlockCrush(bool force)
    {
        if (m_eventCountBorn == Session.Instance.eventCount && !force) return;

        if (Random.value - 0.5f > 0)
        {
            FieldManager.Instance.GetNewBomb(slot.coord, "VLineBomb", slot.transform.position, (int)m_Color);
        }
        else
        {
            FieldManager.Instance.GetNewBomb(slot.coord, "HLineBomb", slot.transform.position, (int)m_Color);
        }
        slot.block = null;
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
    }

    private void Update()
    {
        if (m_lastEventCheck < Session.Instance.eventCount)
        {
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