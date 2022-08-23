using System.Collections;
using UnityEngine;

public class AnimationHelper : Singleton<AnimationHelper>
{
    public void TeleportChip(Chip chip, Slot target)
    {
        StartCoroutine(TeleportChipRoutine(chip, target));
    }

    IEnumerator TeleportChipRoutine(Chip chip, Slot target)
    {
        if (!chip.slot) yield break;
        if (chip.destroying) yield break;
        if (target.chip || target.block) yield break;

        Vector3 scale_target = Vector3.zero;
        target.chip = chip;
        chip.IsBusy = true;

        scale_target.z = 1;
        while (chip.transform.localScale.x != scale_target.x)
        {
            chip.transform.localScale = Vector3.MoveTowards(chip.transform.localScale, scale_target, Time.deltaTime * 20);
            yield return 0;
            if (!chip) yield break;
        }

        chip.transform.localPosition = Vector3.zero;
        scale_target.x = 1;
        scale_target.y = 1;
        while (chip.transform.localScale.x != scale_target.x)
        {
            chip.transform.localScale = Vector3.MoveTowards(chip.transform.localScale, scale_target, Time.deltaTime * 20);
            yield return 0;
            if (!chip) yield break;
        }

        chip.IsBusy = false;
    }

    // Function of creating of explosion effect
    public void Explode(Vector3 center, float radius, float force)
    {
        Chip[] chips = GameObject.FindObjectsOfType<Chip>();

        Vector3 impuls;
        foreach (Chip chip in chips)
        {
            if (chip.slot && chip.slot.block && chip.slot.block.CanItContainChip()) continue;
            if ((chip.transform.Find("icon").position - center).magnitude > radius) continue;
            impuls = (chip.transform.Find("icon").position - center) * force;
            impuls *= Mathf.Pow((radius - (chip.transform.Find("icon").position - center).magnitude) / radius, 2);
            chip.impulse += impuls;
        }
    }
}