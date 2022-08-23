using UnityEngine;

public class ClimberGenerator : MonoBehaviour
{
    public int m_lastGenerate;
    public int m_lastEventCheck; //swap event check

    public Slot slot; // slot that contain this generator 

    private void Start()
    {
        m_lastGenerate = 0;
        m_lastEventCheck = Session.Instance.swapEvent;
        slot = GetComponent<Slot>();
    }
    private void Update()
    {
        if (m_lastEventCheck >= Session.Instance.swapEvent) return;

        if (!Session.Instance.CanIWait()) return;

        if (slot.block) return;

        int count = Butterfly.s_allClimbers.Count;
        if (count >= LevelProfile.main.maxClimber) return;

        if (m_lastGenerate > 0)
        {
            m_lastGenerate--;
            return;
        }
        if (slot.chip)
        {
            if (slot.chip.IsBusy || slot.chip.destroying) return;
            if (slot.chip.gameObject.GetComponent<Butterfly>())
            {
                return;
            }
        }
        //gen climber
        FieldManager.Instance.GetNewButterfly(slot.coord, transform.position);
        m_lastEventCheck = Session.Instance.swapEvent;

        m_lastGenerate = LevelProfile.main.climberGenerateInterval;
    }
}