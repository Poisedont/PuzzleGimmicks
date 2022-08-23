using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainbowBomb : IBomb, IChipLogic
{
    private Chip m_chip;
    public Chip chip { get { return m_chip; } }
    private string type;
    private int birth;

    ////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        m_chip = GetComponent<Chip>();

        birth = Session.Instance.eventCount;
        //chip.IsBusy = true;
        StartCoroutine(CreateAnim());
    }

    IEnumerator CreateAnim()
    {
        chip.CompleteAnim("CreateRainbowBomb");
        chip.PlayAnim("CreateRainbowBomb");
        //AnimationHelper.Instance.Explode(transform.position, 5, 10);
        yield return new WaitForSeconds(chip.TimePlaying("CreateRainbowBomb"));
        //chip.IsBusy = false;
        chip.PlayAnim("Idle");
    }

    public IEnumerator Destroying()
    {
        if (birth == Session.Instance.eventCount)
        {
            chip.destroying = false;
            yield break;
        }

        chip.destroying = true;
        chip.IsBusy = true;

        // find random pieces color id
        List<Chip> target = new List<Chip>();
        foreach (Slot slot in Slot.all.Values)
        {
            if (slot == chip.slot)
                continue;
            if (slot.chip == null) continue;

            if (slot.chip.chipType == "SimpleChip")
            {
                target.Add(slot.chip);
            }
        }

        if (target.Count == 0)
        {
            yield break;
        }

        chip.id = target[UnityEngine.Random.Range(0, target.Count)].id;
        chip.chipType = "SimpleChip";

        yield return StartCoroutine(RainbowColorMixRoutine(chip));

    }

    public string GetChipType()
    {
        return "RainbowBomb";
    }

    public List<Chip> GetDangeredChips(List<Chip> stack)
    {
        if (stack.Contains(chip))
            return stack;

        stack.Add(chip);

        // find same pieces color id
        int color_id = UnityEngine.Random.Range(0, LevelProfile.main.colorCount);
        foreach (Slot slot in Slot.all.Values)
        {
            if (slot.coord == chip.slot.coord) continue;
            if (slot.chip && slot.chip.id == color_id && !stack.Contains(slot.chip))
            {
                stack = slot.chip.GetDangeredChips(stack);
            }
        }
        return stack;
    }

    public int GetPotencial()
    {
        return Slot.all.Count / LevelProfile.main.colorCount; ; //average value per color
    }

    public bool IsMatchable()
    {
        return false;
    }

    #region Mix

    public void RainbowBombMix(Chip other)
    {
        StartCoroutine(RainbowColorMixRoutine(other));
    }

    WaitForSeconds waitForEachSlot = new WaitForSeconds(0.02f);
    IEnumerator RainbowColorMixRoutine(Chip secondary)
    {
        chip.IsBusy = true;
        chip.destroyable = false;
        SoundManager.PlaySFX(SoundDefine.k_rainbow);

        List<Chip> target = new List<Chip>();
        foreach (Slot slot in Slot.all.Values)
        {
            if (slot == chip.slot)
                continue;
            if (slot.chip == null || slot.chip == secondary)
                continue;
            if (secondary.chipType == GetChipType() || slot.chip.id == secondary.id)
            {
                yield return waitForEachSlot;
                if (slot.chip)
                {
                    if (secondary.chipType != "SimpleChip" && secondary.chipType != GetChipType())
                    {
                        FieldManager.Instance.AddPowerup(slot.coord, secondary.chipType);
                    }
                    Lightning.CreateLightning(3, transform, slot.chip.transform, slot.chip.IsColored() ? Chip.colors[slot.chip.id] : Color.white);
                    target.Add(slot.chip);
                }
            }

        }
        yield return new WaitForSeconds(0.5f);

        Session.Instance.EventCounter();

        foreach (Chip t in target)
        {
            if (t)
            {
                if (t.destroying)
                    continue;
                t.SetScore(GameConst.k_base_scoreChip);

                FieldManager.Instance.StoneCrush(t.slot.coord);
                FieldManager.Instance.BlockCrush(t.slot.coord, true);
                t.DestroyChip();
            }
            yield return waitForEachSlot;
        }

        yield return new WaitForSeconds(0.1f);

        FieldManager.Instance.StoneCrush(chip.slot.coord);

        // TODO: play anim destroy
        yield return 0;

        chip.IsBusy = false;
        chip.ParentRemove();

        Destroy(gameObject);
    }

    public void DoubleRainbowMix(Chip other)
    {
        StartCoroutine(DoubleRainbowRoutine(other));
    }

    private IEnumerator DoubleRainbowRoutine(Chip other)
    {
        chip.IsBusy = true;
        chip.destroyable = false;
        other.IsBusy = true;
        other.destroyable = false;

        yield return null;

        chip.destroyable = true;
        other.destroyable = true;
        Session.Instance.EventCounter();

        foreach (var slot in Slot.all.Values)
        {
            if (slot.chip)
            {
                // if (slot.chip == chip || slot.chip == other) continue;
                if (slot.chip.destroying) continue;

                slot.chip.SetScore(GameConst.k_base_scoreChip);
                FieldManager.Instance.StoneCrush(slot.coord);
                FieldManager.Instance.BlockCrush(slot.coord, true);
                slot.chip.DestroyChip();
            }
        }

    }
    #endregion
}