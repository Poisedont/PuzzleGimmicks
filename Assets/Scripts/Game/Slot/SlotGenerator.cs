using UnityEngine;

[RequireComponent(typeof(Slot))]
public class SlotGenerator : MonoBehaviour
{
    public Slot slot;
    float lastTime = -10;
    float delay = 0.15f; // delay between the generations

    public bool generateBlocker = false;
    public float spawnBlockerRatio = 0;
    void Awake()
    {
        slot = GetComponent<Slot>();
        slot.generator = true;
    }

    private void Update()
    {
        if (!Session.Instance || !Session.Instance.isPlaying) return;

        if (slot.chip) return; // Generation is impossible, if slot already contains chip

        if (slot.block && slot.block.IsCastShadow()) return; // Generation is impossible, if the slot is blocked

        if (slot.block && !slot.block.IsCastShadow())
        {
            Slot slotNeedChip = Utils.FindSlotEmptyFollowGravity(slot);
            if (!slotNeedChip) return;
        }

        if (Chip.gravityBlockers.Count > 0) return;

        if (lastTime + delay > Time.time) return; // limit of frequency generation
        lastTime = Time.time;

        //generate
        Vector3 spawnOffset = new Vector3(
            Utils.SideOffsetX(Utils.MirrorSide(slot.slotGravity.gravityDirection)),
            Utils.SideOffsetY(Utils.MirrorSide(slot.slotGravity.gravityDirection)),
            0) * 0.4f;

        if (LevelProfile.main.HasTarget(FieldTarget.KeyDrop) && Session.Instance.creatingDropsCount > 0)
        {
            int targetDropsCount = LevelProfile.main.GetTargetCount(0, FieldTarget.KeyDrop);
            if (KeyDrop.s_alive_key_count == 0
                || targetDropsCount > 0 && Session.Instance.GetResource() <= 0.4f + 0.6f * Session.Instance.creatingDropsCount / targetDropsCount)
            {
                Session.Instance.creatingDropsCount--;
                // creating new key sinker
                FieldManager.Instance.GetSinkerKey(slot.coord, transform.position + spawnOffset);
                return;
            }
        }
        if (LevelProfile.main.randomChangerConfig != null && LevelProfile.main.randomChangerConfig.Enable)
        {
            var changerConfig = LevelProfile.main.randomChangerConfig;
            if (Session.Instance.swapEvent >= MagicPortion.s_generatorEvent)
            {
                if (MagicPortion.s_portionCount < changerConfig.MaxNumber)
                {
                    FieldManager.Instance.GetNewMagicPortion(slot.coord, transform.position + spawnOffset);

                    MagicPortion.s_generatorEvent = Session.Instance.swapEvent + changerConfig.GenerateInterval;
                    return;
                }
            }
        }
        if (generateBlocker)
        {
            int random = Random.Range(0, 100);
            if (random < spawnBlockerRatio) // need to check ratio
            {
                FieldManager.Instance.GetNewBlockerChip(slot.coord, transform.position + spawnOffset, 1);
            }
            else
            {
                FieldManager.Instance.GetNewSimpleChip(slot.coord, transform.position + spawnOffset); // creating new chip
            }
        }
        else
        {
            FieldManager.Instance.GetNewSimpleChip(slot.coord, transform.position + spawnOffset); // creating new chip
        }

    }
}