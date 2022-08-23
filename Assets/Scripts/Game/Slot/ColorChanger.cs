using System.Collections;
using UnityEngine;

public class ColorChanger : IBlock
{
    private int m_lastEventWorking;
    [SerializeField] private float m_animDuration = 0.25f;
    ////////////////////////////////////////////////////////////////////////////////
    public override void BlockCrush(bool force)
    {
        // this block can't be destroyed
        return;
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
    }

    private void Update()
    {
        if (m_lastEventWorking < Session.Instance.swapEvent)
        {
            StartCoroutine(ChangeColor(slot));
        }
    }

    IEnumerator ChangeColor(Slot slot)
    {
        //due to player can make swap while chip moving, so need to work every swap 
        m_lastEventWorking++;

        yield return StartCoroutine(Utils.WaitFor(Session.Instance.CanIWait, 0.5f));

        if (slot.chip && slot.chip.IsColored())
        {
            slot.chip.IsBusy = true;
            transform.GetComponent<Animation>().Play("ChangeEffects");

            float time = 0;
            float progress = 0;
            Vector3 startPosition = Vector3.one;
            Vector3 targetPosition = Vector3.zero;

            while (progress < m_animDuration)
            {
                time = EasingFunctions.easeInOutQuad(progress / m_animDuration);
                slot.chip.transform.localScale = Vector3.Lerp(startPosition, targetPosition, time);
                progress += Time.deltaTime;
                yield return 0;
            }

            slot.chip.ChangeColor();

            while (progress > 0)
            {
                time = EasingFunctions.easeInOutQuad(progress / m_animDuration);
                slot.chip.transform.localScale = Vector3.Lerp(startPosition, targetPosition, time);
                progress -= Time.deltaTime;
                yield return 0;
            }

            slot.chip.transform.localScale = Vector3.one;

            slot.chip.IsBusy = false;
        }
    }
}