using UnityEngine;

public class SlotTeleport : MonoBehaviour
{
    public Slot target;
    public Slot slot;

    public Vector2Int target_postion = Utils.Vector2IntNull;

    float lastTime = -10;
    float delay = 0.15f;

    void Start()
    {
        slot = GetComponent<Slot>();
        slot.slotTeleport = this;
    }
    public void Initialize()
    {
        if (!enabled) return;
        Vector2Int position = target_postion;

        target = Slot.GetSlot(position);
        if (target)
        {
            target.teleportTarget = true;


            //create portal out object on target
            GameObject portOut = ContentManager.Instance.GetItem("portalOut", target.transform.position);
            portOut.transform.parent = target.transform;
            portOut.transform.localPosition = Vector3.zero;
            portOut.transform.Rotate(0, 0, Utils.SideToAngle(target.slotGravity.gravityDirection) + 90);

            // remove generator at target if any
            SlotGenerator generator = target.GetComponent<SlotGenerator>();
            if (generator)
            {
                Destroy(generator);
            }
        }
        else
        {
            Destroy(this);
        }
    }

    void Update()
    {
        if (!target) return; // Teleport is possible only if target is exist

        if (!slot.chip) return; // Teleport is possible only if slot contains chip

        if (slot.chip.IsBusy) return; // If chip can't be moved, then it can't be teleported

        if (target.chip) return; // Teleport is impossible if target slot already contains chip

        if (slot.block) return; // Teleport is impossible, if the slot is blocked
        if (target.block) return; // Teleport is impossible, if the target slot is blocked

        if (slot.chip.transform.position != slot.transform.position) return;

        if (lastTime + delay > Time.time) return; // limit of frequency generation
        lastTime = Time.time;

        AnimationHelper.Instance.TeleportChip(slot.chip, target);
    }
}