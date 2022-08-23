using System.Collections.Generic;
using UnityEngine;

public class Switcher : IBlock
{
    private int m_lastEventWorking;
    private float m_timeWaiting;

    private SwitcherInfo m_info;
    private bool m_isSwitching;

    private float m_phaseTime;
    private int m_phase; // 0: scale down, 1: scale up

    static List<Switcher> s_allSwitcher;

    const float k_switch_duration = 0.25f;
    readonly Quaternion m_startAngle = Quaternion.AngleAxis(0, Vector3.forward);
    readonly Quaternion m_endAngle = Quaternion.AngleAxis(180, Vector3.forward);

    public override void BlockCrush(bool force)
    {
        // this can't be destroy
    }

    public override bool CanBeCrushedByNearSlot(Chip near = null)
    {
        return false;
    }

    public override bool CanItContainChip()
    {
        return true;
    }

    public override int GetLevels()
    {
        return 1;
    }

    public override void Initialize(SlotSettings settings = null)
    {
        m_lastEventWorking = Session.Instance.swapEvent;
        m_info = settings.switcherInfo;
        if (m_info == null)
        {
            Debug.LogError("Something wrong with Switcher " + settings.position);
        }

        if (s_allSwitcher == null)
        {
            s_allSwitcher = new List<Switcher>();
        }
        s_allSwitcher.Add(this);

    }

    Chip m_nextChip = null;
    Switcher m_nextSwitcher = null;
    private void FindNextSwitcher()
    {
        if (!m_nextSwitcher)
        {
            List<Switcher> group = s_allSwitcher.FindAll(sw => sw.m_info.GroupIndex == this.m_info.GroupIndex);

            if (group.Count >= 2)
            {
                if (group.Count == 2)
                {
                    Switcher next = group.Find(sw => sw.GetInstanceID() != this.GetInstanceID());
                    m_nextSwitcher = next;
                }
                else
                {
                    Switcher next = group.Find(sw => sw.m_info.Index == (this.m_info.Index % group.Count) + 1);
                    if (next)
                    {
                        m_nextSwitcher = next;
                    }
                }
            }
        }
    }
    private void Update()
    {
        if (m_lastEventWorking < Session.Instance.swapEvent)
        {
            if (!Session.Instance.CanIWait()) return;

            if (slot.chip && slot.chip.destroying) return;

            m_timeWaiting += Time.deltaTime;
            if (m_timeWaiting <= 0.5f) return;

            m_lastEventWorking = Session.Instance.swapEvent;
            m_timeWaiting = 0;

            List<Switcher> group = s_allSwitcher.FindAll(sw => sw.m_info.GroupIndex == this.m_info.GroupIndex);
            if (!m_nextSwitcher)
            {
                FindNextSwitcher();
            }
            if (m_nextSwitcher)
            {
                m_nextSwitcher.m_nextChip = this.slot.chip;
            }
        }
        else if (m_nextChip != null && !m_isSwitching)
        {
            m_isSwitching = true;
            m_phase = 0;
        }

        if (m_isSwitching)
        {
            SwitchChipUpdate();
        }
    }

    void TransferChip()
    {
        if (m_nextSwitcher)
        {
            if (m_nextChip)
            {
                m_nextChip.transform.position = slot.transform.position;
                slot.chip = m_nextChip;
                m_nextChip = null;

                m_nextSwitcher.TransferChip(); //2 switcher must transfer chip in same frame
            }
        }
    }
    void SwitchChipUpdate()
    {
        float time = 0;
        float progress = m_phaseTime;
        m_isSwitching = true;

        if (m_phase == 0)
        {
            if (progress < k_switch_duration)
            {
                time = EasingFunctions.easeInOutQuad(progress / k_switch_duration);
                if (slot.chip)
                {
                    slot.chip.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time);
                    slot.chip.transform.rotation = Quaternion.Lerp(m_startAngle, m_endAngle, time);
                }
                progress += Time.deltaTime;

                m_phaseTime = progress;
            }
            else
            {
                TransferChip();
                m_phase = 1;
            }
        }
        else
        {
            if (progress > 0)
            {
                time = EasingFunctions.easeInOutQuad(progress / k_switch_duration);
                if (slot.chip)
                {
                    slot.chip.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time);
                    slot.chip.transform.rotation = Quaternion.Lerp(m_startAngle, m_endAngle, time);
                }
                progress -= Time.deltaTime;
                m_phaseTime = progress;
            }
            else
            {
                m_isSwitching = false;
                if (slot.chip)
                {
                    slot.chip.IsBusy = false;
                    this.slot.chip.transform.localScale = Vector3.one;
                    slot.chip.transform.rotation = Quaternion.identity;
                }
            }
        }
    }
}

public class SwitcherInfo
{
    public int GroupIndex;
    public int Index;

    public SwitcherInfo GetClone()
    {
        return MemberwiseClone() as SwitcherInfo;
    }

    // override object.Equals
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        SwitcherInfo otherInfo = obj as SwitcherInfo;
        if (this.GroupIndex != otherInfo.GroupIndex) return false;
        if (this.Index != otherInfo.Index) return false;

        return base.Equals(obj);
    }

    // override object.GetHashCode
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}