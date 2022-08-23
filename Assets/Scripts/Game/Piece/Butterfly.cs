using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Butterfly : MonoBehaviour, IChipLogic
{
    private Chip m_chip;
    public Chip chip
    {
        get { return m_chip; }
    }

    int m_eventBirth;
    int m_lastEventCheck;
    WaitForSeconds m_waitForClimpup = new WaitForSeconds(1f);

    public static List<Butterfly> s_allClimbers = new List<Butterfly>();
    Slot m_slotTarget;
    Chip m_swapChip;
    ////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        m_chip = GetComponent<Chip>();
        s_allClimbers.Add(this);
    }
    private void Start()
    {
        m_eventBirth = Session.Instance.eventCount;
        m_lastEventCheck = Session.Instance.swapEvent;

    }
    public IEnumerator Destroying()
    {
        if (m_eventBirth == Session.Instance.eventCount)
        {
            chip.destroying = false;
            yield break;
        }

        chip.IsBusy = true;

        int ptarget = LevelProfile.main.GetTargetCount(0, FieldTarget.Butterfly);

        if (ptarget > 0)
        {
            TargetUI[] listO = GameObject.FindObjectsOfType<TargetUI>();

            TargetUI go = null;

            for (int i = 0; i < listO.Length; i++)
            {
                if (listO[i].GetLvlTarget().GetTarget().Equals(FieldTarget.Butterfly))
                {
                    if (listO[i].GetLvlTarget().GetTargetCount(0) > 0
                       && listO[i].GetIndexTaret() == 0
                       && listO[i].GetLvlTarget().GetTargetCount(0) > listO[i].GetLvlTarget().GetCurrentCount(0)
                       )
                    {
                        go = listO[i];
                    }
                }
            }

            if (go)
            {
                Transform target = go.transform;

                //sprite.sortingLayerName = "UI";
                //sprite.sortingOrder = 10;

                float time = 0;
                float speed = Random.Range(1f, 1.8f);
                Vector3 startPosition = transform.position;
                Vector3 targetPosition = target.position;

                while (time < 1)
                {
                    transform.position = Vector3.Lerp(startPosition, targetPosition, EasingFunctions.easeInOutQuad(time));
                    time += Time.unscaledDeltaTime * speed;
                    yield return 0;
                }

                transform.position = target.position;
            }
        }

        Session.Instance.CollectButterfly();
        chip.PlayAnim("destroying");
        chip.SetScore(1);

        //Quyen add
        yield return new WaitForEndOfFrame();

        chip.ParentRemove();
        chip.IsBusy = false;

        Destroy(gameObject, chip.TimePlaying("destroying"));
    }
    private void OnDestroy()
    {
        if (s_allClimbers.Contains(this))
        {
            s_allClimbers.Remove(this);
        }
    }

    private void Update()
    {
        if (m_lastEventCheck < Session.Instance.swapEvent)
        {
            if (!Session.Instance.CanIWait()) return;

            StartCoroutine(ClimbUpRoutine());

            m_lastEventCheck = Session.Instance.swapEvent;

        }

        if (m_slotTarget)
        {
            // swap slot
            Slot currentSlot = this.chip.slot;

            Session.Instance.SwapChipToSlot(this.chip, m_slotTarget);
            m_slotTarget = null;

            if (m_swapChip)
            {
                Session.Instance.SwapChipToSlot(m_swapChip, currentSlot);
                m_swapChip = null;
            }
        }
    }

    IEnumerator ClimbUpRoutine()
    {
        //yield return m_waitForClimpup;
        yield return StartCoroutine(Utils.WaitFor(Session.Instance.CanIWait, 1f));

        if (chip.destroying) yield break;

        //while (!Session.Instance.CanIWait())
        //{
        //    yield return null;
        //}
        // NOTE: default checking side is TOP
        Slot neighborSlot = Slot.GetSlot(Utils.Vec2IntAdd(chip.slot.coord, Side.Top));
        if (neighborSlot)
        {
            if (neighborSlot.block) yield break;
            if (!neighborSlot.chip) yield break;
            //NOTE: find climber at top (IMPORTANT: order of update must from top to down/ follow gravity direction)
            Butterfly neighborButt = neighborSlot.chip.gameObject.GetComponent<Butterfly>();
            if (neighborButt)
            {
                if (neighborButt.m_slotTarget)
                {
                    this.m_slotTarget = neighborButt.chip.slot;
                    // take slot of other butterfly, so take care of its swapped chip
                    this.m_swapChip = neighborButt.m_swapChip;
                    neighborButt.m_swapChip = null;
                }
            }
            else if (!neighborSlot.chip.destroying
               && !neighborSlot.chip.IsBusy
               && neighborSlot.chip.transform.localPosition == Vector3.zero)
            {

                if (neighborSlot.chip.gameObject.GetComponent<Butterfly>())
                {
                    yield return null;
                }
                // find where to climb up
                m_slotTarget = neighborSlot;
                m_swapChip = neighborSlot.chip;
            }
        }
        else
        {
            // turn to new simple pieces
            FieldManager.Instance.GetNewSimpleChip(chip.slot.coord, transform.position);
            chip.HideChip(false);
        }

    }

    public string GetChipType()
    {
        return EPieces.Butterfly.ToString();
    }

    public List<Chip> GetDangeredChips(List<Chip> stack)
    {
        stack.Add(chip);
        return stack;
    }

    public int GetPotencial()
    {
        if (LevelProfile.main.HasTarget(FieldTarget.Butterfly))
        {
            return 5;
        }
        return 1;
    }

    public bool IsMatchable()
    {
        return true;
    }

    public static void Cleanup()
    {
        if (s_allClimbers != null)
        {
            s_allClimbers.Clear();
        }
    }
}