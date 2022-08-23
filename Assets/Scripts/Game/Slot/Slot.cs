using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public static Dictionary<Vector2Int, Slot> all = new Dictionary<Vector2Int, Slot>();

    public bool generator = false;
    public bool teleportTarget = false;
    public BackLayer stone; // backlayer (stone) for this slot
    // public Jam jam;
    public IBlock block; // Block for this slot

    // Position of this slot
    public Vector2Int coord = new Vector2Int();
    public int x { get { return coord.x; } }
    public int y { get { return coord.y; } }

    public Slot this[Side index]
    { // access to neighby slots on the index
        get
        {
            return nearSlot[index];
        }
    }

    public Dictionary<Side, Slot> nearSlot = new Dictionary<Side, Slot>(); // Nearby slots dictionary
    public Dictionary<Side, bool> wallMask = new Dictionary<Side, bool>(); // Dictionary walls - blocks the movement of chips in certain directions

    public SlotGravity slotGravity;
    public SlotTeleport slotTeleport;

    public bool sugarDropSlot = false; //item that need collect which drop down
    /// <summary>
    /// GameObject transform that hold all slot of game Field
    /// </summary>
    public static Transform folder;

    [SerializeField] Chip _chip;
    public Chip chip
    {
        get { return _chip; }
        set
        {
            if (value == null)
            {
                if (_chip)
                {
                    _chip.slot = null;
                }
                _chip = null;
                return;
            }
            if (_chip)
            {
                _chip.slot = null;
            }
            _chip = value;
            _chip.transform.SetParent(transform);
            if (_chip.slot)
            {
                _chip.slot.chip = null;
            }
            _chip.slot = this;
        }
    }
    ////////////////////////////////////////////////////////////////////////////////
    #region Init
    private void Awake()
    {
        slotGravity = GetComponent<SlotGravity>();
        slotTeleport = GetComponent<SlotTeleport>();
    }

    public static void Initialize()
    {
        foreach (Slot slot in FindObjectsOfType<Slot>())
        {
            if (!all.ContainsKey(slot.coord))
            {
                all.Add(slot.coord, slot);
            }
        }


        foreach (Slot slot in all.Values)
        {
            foreach (Side side in Utils.allSides) // Filling of the nearby slots dictionary 
            {
                Vector2Int key = Utils.Vec2IntAdd(slot.coord, side);
                slot.nearSlot.Add(side, all.ContainsKey(key) ? all[key] : null);
            }
            slot.nearSlot.Add(Side.Null, null);
        }

        Side direction;
        SlotTeleport teleport;
        foreach (Slot slot in all.Values)
        {
            direction = slot.slotGravity.gravityDirection;
            if (slot[direction])
            {
                slot[direction].slotGravity.fallingDirection = Utils.MirrorSide(direction);
            }
            teleport = slot.GetComponent<SlotTeleport>();
            if (teleport)
            {
                teleport.Initialize();
            }
        }

        //Scroll magic init
        ScrollMagic.Initialize();

        BookShelfGroup.Initialize();

        MagicTapGroup.Initialize();
        CandyTreeGroup.Initialize();
    }

    #endregion

    #region Helper
    public static Slot GetSlot(Vector2Int position)
    {
        if (all.ContainsKey(position))
        {
            return all[position];
        }
        return null;
    }

    public static Slot GetSlot(int x, int y, int z)
    {
        return GetSlot(new Vector2Int(x, y));
    }

    public void SetWall(Side side)
    {
        wallMask[side] = true;

        foreach (Side s in Utils.straightSides)
        {
            if (wallMask[s])
            {
                if (nearSlot[s])
                {
                    nearSlot[s].nearSlot[Utils.MirrorSide(s)] = null;
                }
                nearSlot[s] = null;
            }
        }
        foreach (Side s in Utils.slantedSides)
        {
            if (wallMask[Utils.SideHorizontal(s)] && wallMask[Utils.SideVertical(s)])
            {
                if (nearSlot[s])
                {
                    nearSlot[s].nearSlot[Utils.MirrorSide(s)] = null;
                }
                nearSlot[s] = null;
            }
        }

    }

    public static bool HasDestroyingChip()
    {
        foreach (var item in all)
        {
            Chip chip = item.Value.chip;
            if (chip)
            {
                if (chip.destroying)
                {
                    return true;
                }
            }
        }
        return false;
    }


    public bool GetShadow()
    {
        if (slotGravity) return slotGravity.shadow;
        else return false;
    }

    // Shadow can also discard the other chips - it's a different kind of shadow.
    public bool GetChipShadow()
    {
        Side direction = slotGravity.fallingDirection;
        Slot s = nearSlot[direction];
        for (int i = 0; i < 40; i++)
        {
            if (!s) return false;
            if (s.block) return false;
            if (!s.chip || s.slotGravity.gravityDirection != direction)
            {
                direction = s.slotGravity.fallingDirection;
                s = s.nearSlot[direction];
            }
            else return true;
        }
        return false;
    }

    public void SetScore(float s)
    {
        Session.Instance.score += Mathf.RoundToInt(s * GameConst.k_session_scoreChip);

        // TODO: show score anim
    }
    #endregion
}