using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBomb : IBomb, IChipLogic
{
    ////////////////////////////////////////////////////////////////////////////////

    private Chip m_chip;
    public Chip chip { get { return m_chip; } }
    int birth;
    int bombSize = 2; // default size of bomb, increase size will make bomb expose bigger

    ////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        m_chip = GetComponent<Chip>();
        chip.chipType = "SimpleBomb";
        birth = Session.Instance.eventCount;
        //chip.IsBusy = true;
        StartCoroutine(CreateAnim());
    }

    IEnumerator CreateAnim()
    {

        chip.CompleteAnim("CreateSimpleBomb");
        chip.PlayAnim("CreateSimpleBomb");


        yield return new WaitForSeconds(chip.TimePlaying("CreateSimpleBomb"));
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

        Vector2Int coord = chip.slot.coord;

        chip.destroying = true;
        chip.IsBusy = true;

        if (bombSize == 2)
            chip.PlayAnim("DestroyingBomb");
        else
            chip.PlayAnim("DestroyingBombX2");
        AnimationHelper.Instance.Explode(transform.position, 3, 4);
        // yield return new WaitForSeconds(chip.TimePlaying("DestroyingBomb"));
        yield return 0;

        // destroy diamond neighbors chip
        for (int row = 0; row <= bombSize; row++)
        {

            for (int col = 0; col <= bombSize - row; col++)
            {
                if (col == 0 && row == 0) continue;

                Vector2Int offset = (new Vector2Int(col, row));

                NeighborCrush(coord + offset);

                if (offset.x != 0)
                {
                    offset.x *= -1;
                    NeighborCrush(coord + offset);
                }
                if (offset.y != 0)
                {
                    offset.y *= -1;
                    NeighborCrush(coord + offset);
                }
                // must do again to get neighbor with y *= -1
                if (offset.x != 0 && offset.y != 0)
                {
                    offset.x *= -1;
                    NeighborCrush(coord + offset);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

        // TODO: play anim destroy, audio play


        SoundManager.PlaySFX(SoundDefine.k_line);
        yield return new WaitForSeconds(0.2f);


        // TODO: anim explose bomb
        //chip.PlayAnim("DestroyingBomb");
        //yield return new WaitForSeconds(chip.TimePlaying("DestroyingBomb"));

        chip.ParentRemove();
        chip.IsBusy = false;

        ///wait for animation finish
        //while (chip.IsPlaying("DestroyingBomb"))
        //     yield return 0;

        Destroy(gameObject);
    }

    void NeighborCrush(Vector2Int coord)
    {
        Slot slot = Slot.GetSlot(coord);
        if (slot)
        {
            if (slot.chip && !(slot.block && slot.block is Caged))
            {
                slot.chip.SetScore(GameConst.k_base_scoreChip);
                slot.chip.DestroyChip();
            }
            FieldManager.Instance.StoneCrush(coord);
            FieldManager.Instance.BlockCrush(coord, false, true);
        }

    }

    public string GetChipType()
    {
        return "SimpleBomb";
    }

    public List<Chip> GetDangeredChips(List<Chip> stack)
    {
        if (stack.Contains(chip))
        { return stack; }

        stack.Add(chip);

        Slot slot;

        foreach (Side side in Utils.allSides)
        {
            slot = chip.slot[side];
            if (slot && slot.chip)
            {
                stack = slot.chip.GetDangeredChips(stack);
            }
        }

        return stack;
    }

    public int GetPotencial()
    {
        return 7;
    }

    public bool IsMatchable()
    {
        return true;
    }

    #region Mix
    // Simple + Simple
    public void SimpleMix(Chip secondary)
    {
        Session.Instance.EventCounter();
        this.bombSize = 3;
        chip.DestroyChip();
    }

    #endregion
}