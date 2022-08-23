using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossBomb : IBomb, IChipLogic
{
    public enum EBombType
    {
        VLineBomb,
        HLineBomb,
        CrossBomb,
    }
    [SerializeField] EBombType m_type;
    [Tooltip("Effect Side of this bombs depend on bomb type")]
    [SerializeField] List<Side> sides;
    ////////////////////////////////////////////////////////////////////////////////
    private Chip m_chip;
    public Chip chip { get { return m_chip; } }
    private string type;
    private int birth;
    private EBombType m_forceBombType;
    public EBombType ForceBombType { set { m_forceBombType = value; } }

    public bool IsIce { get; set; } //mark bomb component is not able to work
    IceLineBomb m_ice;
    ////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        m_chip = GetComponent<Chip>();
        type = m_type.ToString();
        m_chip.chipType = type;
        birth = Session.Instance.eventCount;
        //chip.IsBusy = true;
        m_forceBombType = m_type;

        m_ice = GetComponent<IceLineBomb>();
        if (m_ice)
        {
            IsIce = true;
        }
        StartCoroutine(CreateBombAnim());
    }

    IEnumerator CreateBombAnim()
    {
        chip.CompleteAnim("CreateBomb");
        chip.PlayAnim("CreateBomb");

        yield return new WaitForSeconds(chip.TimePlaying("CreateBomb"));

        chip.PlayAnim("Idle");
        //chip.IsBusy = false;
    }
    List<Side> GetSides()
    {
        if (m_forceBombType == m_type)
        {
            return sides;
        }
        else
        {
            List<Side> forceSides = new List<Side>();
            switch (m_forceBombType)
            {
                case EBombType.HLineBomb:
                    forceSides.Add(Side.Left);
                    forceSides.Add(Side.Right);
                    break;
                case EBombType.VLineBomb:
                    forceSides.Add(Side.Top);
                    forceSides.Add(Side.Bottom);
                    break;
            }
            return forceSides;
        }
    }
    public IEnumerator Destroying()
    {
        if (birth == Session.Instance.eventCount)
        {
            chip.destroying = false;
            yield break;
        }

        if (IsIce)
        {
            if (m_ice)
            {
                yield return m_ice.Destroying();
            }
        }
        else
        {
            chip.destroying = true;
            AnimationHelper.Instance.Explode(transform.position, 2, -3);
            while (transform.localPosition != Vector3.zero)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, Time.deltaTime * 10);
                yield return 0;
            }

            List<Side> sides = GetSides();


            // run anim destroying, audio play 
            if (sides[0] != Side.Top || sides.Count < 4)
                chip.PlayAnim("DestroyingBomb");

            SoundManager.PlaySFX(SoundDefine.k_line);
            Vector2Int coord = chip.slot.coord;
            

            int count = 1;
            // mark chip busy on each side
            for (int path = 1; count > 0; path++)
            {
                count = 0;
                foreach (Side side in sides)
                {
                    if (Freez(coord + Utils.GetSideOffset(side) * path))
                    {
                        count++;
                    }
                }
            }

            count = 4;
            for (int path = 1; count > 0; path++)
            {
                count = 0;
                yield return new WaitForSeconds(0.04f);
                foreach (Side side in sides)
                {
                    if (Crush(coord + Utils.GetSideOffset(side) * path, m_forceBombType))
                    {
                        count++;
                    }
                }
            }

            yield return new WaitForEndOfFrame();
            chip.ParentRemove();
        }
    }

    public string GetChipType()
    {
        if (IsIce) return "IceBomb";
        return type;
    }

    public List<Chip> GetDangeredChips(List<Chip> stack)
    {
        if (stack.Contains(chip) || !chip.slot)
        {
            return stack;
        }

        stack.Add(chip);

        Slot s;
        for (int path = 1; path < LevelProfile.maxSize; path++)
        {
            foreach (Side side in sides)
            {
                s = Slot.GetSlot(chip.slot.coord + Utils.GetSideOffset(side) * path);
                if (s && s.chip)
                {
                    stack = s.chip.GetDangeredChips(stack);
                }
            }
        }

        return stack;
    }

    public int GetPotencial()
    {
        return LevelProfile.main.height + LevelProfile.main.width - 1;
    }

    public bool IsMatchable()
    {
        return true;
    }

    /// <summary>
    /// Make the chip in coord to be busy if it destroyable
    /// </summary>
    /// <returns>
    /// True if coord still in field. False if coord is touch edge of field
    /// </returns>
    public static bool Freez(Vector2Int coord)
    {
        Slot s = Slot.GetSlot(coord);
        if (s && s.chip && !s.chip.IsBusy && s.chip.destroyable && !(s.block && s.block is Caged))
        {
            s.chip.IsBusy = true;
        }

        return Utils.IsVec2Hit(coord, 0, 0, LevelProfile.main.width - 1, LevelProfile.main.height - 1);
    }

    public static bool Crush(Vector2Int coord, EBombType bombType)
    {
        Slot s = Slot.GetSlot(coord);
        FieldManager.Instance.StoneCrush(coord);
        FieldManager.Instance.BlockCrush(coord, false, true);
        if (s && s.chip && !(s.block && s.block is Caged))
        {
            if (s.chip.logic.GetChipType() == EBombType.HLineBomb.ToString() && bombType == EBombType.HLineBomb)
            {
                var bomb = s.chip.logic as CrossBomb;
                bomb.ForceBombType = EBombType.VLineBomb;
            }
            else if (s.chip.logic.GetChipType() == EBombType.VLineBomb.ToString() && bombType == EBombType.VLineBomb)
            {
                var bomb = s.chip.logic as CrossBomb;
                bomb.ForceBombType = EBombType.HLineBomb;
            }
            Chip c = s.chip;
            c.SetScore(GameConst.k_base_scoreChip);
            c.DestroyChip();
            AnimationHelper.Instance.Explode(s.transform.position, 2, -3);
        }
        return Utils.IsVec2Hit(coord, 0, 0, LevelProfile.main.width - 1, LevelProfile.main.height - 1);
    }

    #region Mix with other
    public void CrossSimpleMix(Chip secondary)
    {
        chip.PlayAnim("DestroyingCross3");
        StartCoroutine(CrossSimpleMixRoutine(secondary));
    }

    // Line bomb + Simple bomb
    IEnumerator CrossSimpleMixRoutine(Chip secondary)
    {
        chip.IsBusy = true;
        chip.destroyable = false;
        Session.Instance.EventCounter();
        sides = new List<Side>(Utils.straightSides); // 4 sides

        // make mix effect
        //GameObject effectObj = ContentManager.Instance.GetItem("SimpleCrossMixEffect");
        //if (effectObj)
        //{
        //    Transform effect = effectObj.transform;
        //    effect.SetParent(Slot.folder);
        //    effect.position = transform.position;
        //    effect.GetComponent<Animation>().Play();

        //}
        // TODO: Audio play("CrossBombCrush");
        //chip.Minimize();

        yield return new WaitForSeconds(0.1f);

        FieldManager.Instance.BlockCrush(chip.slot.coord, false);

        //System.Action<Vector2Int> Wave = (Vector2Int coord) =>
        //{
        //    Slot s = Slot.GetSlot(coord);
        //    if (s)
        //    {

        //        // TODO: anim explose at coord
        //    }
        //};

        // mix Line bomb with simple bomb will make cross 3 time 
        for (int path = 0; path < LevelProfile.maxSize; path++)
        {
            foreach (Side side in sides)
            {
                Freez(chip.slot.coord + Utils.GetSideOffset(side) * path);
                Freez(chip.slot.coord + Utils.GetSideOffset(side) * path + Utils.GetSideOffset(Utils.RotateSide(side, 2)));
                Freez(chip.slot.coord + Utils.GetSideOffset(side) * path + Utils.GetSideOffset(Utils.RotateSide(side, -2)));
            }
        }

        foreach (Side side in Utils.allSides)
        {
            Crush(chip.slot.coord + Utils.GetSideOffset(side), m_forceBombType);
        }
        //Wave(chip.slot.coord);

        yield return new WaitForSeconds(0.05f);

        

        for (int path = 2; path < LevelProfile.maxSize; path++)
        {
            foreach (Side side in sides)
            {
                Crush(chip.slot.coord + Utils.GetSideOffset(side) * path, m_forceBombType);
                Crush(chip.slot.coord + Utils.GetSideOffset(side) * path + Utils.GetSideOffset(Utils.RotateSide(side, 2)), m_forceBombType);
                Crush(chip.slot.coord + Utils.GetSideOffset(side) * path + Utils.GetSideOffset(Utils.RotateSide(side, -2)), m_forceBombType);
                //Wave(chip.slot.coord + Utils.GetSideOffset(side) * path);
            }
            yield return new WaitForSeconds(0.05f);
        }

        chip.IsBusy = false;
        //chip.HideChip(false);
        Destroy(gameObject);
    }

    // Line + Line
    public void LineMix(Chip secondary)
    {
        // set 4 sides to this bomb and make it boom
        Session.Instance.EventCounter();
        sides = new List<Side>(Utils.straightSides);
        chip.PlayAnim("DestroyingCross");
        chip.DestroyChip();
    }

    public void CrossLineMix(Chip secondary)
    {
        // set 8 sides to this bomb and make it boom
        Session.Instance.EventCounter();
        sides = new List<Side>(Utils.allSides);
        chip.PlayAnim("DestryingAllSides");
        chip.DestroyChip();
    }

    #endregion
}