using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chip : MonoBehaviour
{
    public static List<Chip> busyList = new List<Chip>();
    public static List<Chip> gravityBlockers = new List<Chip>();
    /// <summary>
    /// Colors define array
    /// </summary>
    public static readonly Color[] colors = {
        new Color(0.75f, 0.3f, 0.3f),
        new Color(0.3f, 0.75f, 0.3f),
        new Color(0.3f, 0.5f, 0.75f),
        new Color(0.75f, 0.75f, 0.3f),
        new Color(0.75f, 0.3f, 0.75f),
        new Color(0.75f, 0.5f, 0.3f),
    };


    public static readonly string[] chipTypes = {
                                           "Red",
                                           "Green",
                                           "Blue",
                                           "Yellow",
                                           "Purple",
                                           "Orange"
    };

    public enum EChipType
    {
        None = -1,
        Red,
        Green,
        Blue,
        Yellow,
        Purple,
        Orange,
        Universal = 10,
    }

    public static readonly int universalColorId = 10;
    public static readonly int uncoloredId = -1;
    #region Variables
    public Slot slot; // Slot which include this chip
    public string chipType = "None"; // Chip type name
    public int id; // Chip color ID
    public int powerId; // Chip type ID
    public bool destroyable = true;
    public int movementID = 0;
    public Vector3 impulse = Vector3.zero;
    public Vector3 impulseHit = Vector3.zero;
    float velocity = 0;

    private bool m_busy;
    public bool m_checkHit = false;
    [SerializeField] EChipType m_color;
    [SerializeField] Animation m_animation;
    public bool IsBusy
    {
        get { return m_busy; }
        set
        {
            if (m_busy == value) return;
            m_busy = value;
            if (m_busy)
            {
                busyList.Add(this);
            }
            else
            {
                busyList.Remove(this);
            }
        }
    }

    private bool m_gravity;
    public bool Gravity
    {
        get { return m_gravity; }
        set
        {
            if (m_gravity == value) return;
            m_gravity = value;
            if (m_gravity)
            {
                gravityBlockers.Remove(this);
            }
            else
            {
                gravityBlockers.Add(this);
            }
        }
    }

    public bool destroying = false; // in the process of destruction
    Vector3 lastPosition;

    public IChipLogic logic;
    public IChipColorChangeable colorChangeable;
    #endregion

    ////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        logic = GetComponent<IChipLogic>();

        colorChangeable = GetComponent<IChipColorChangeable>();
        chipType = logic.GetChipType();

        id = (int)m_color;

        StartCoroutine(ChipPhysics());
    }
    #region Logic of Piece
    public bool IsMatchable()
    {
        if (!logic.IsMatchable()) return false;
        if (destroying) return false;
        if (IsUncolored()) return false;
        if (busyList.Count == 0) return true;
        if (transform.position != slot.transform.position) return false;
        return true;
    }

    public bool IsUniversalColor()
    {
        return id == universalColorId;
    }

    public bool IsUncolored()
    {
        return id == uncoloredId;
    }

    public bool IsColored()
    {
        return id == Mathf.Clamp(id, 0, colors.Length - 1);
    }

    public int GetPotencial()
    {
        int potential = logic.GetPotencial();
        List<Chip> dangerChips = GetDangeredChips(new List<Chip>());
        foreach (Chip c in dangerChips)
        {
            if (LevelProfile.main.HasTarget(FieldTarget.Color))
            {
                if (c.IsColored())
                {
                    int targetColor = LevelProfile.main.GetTargetCount(c.id, FieldTarget.Color);
                    int currentColor = Session.Instance.GetCurrentCountOfTarget(FieldTarget.Color, c.id);
                    if (targetColor - currentColor > 0) //color target remain > 0
                    {
                        potential += 10;
                    }
                }
            }
            if (LevelProfile.main.HasTarget(FieldTarget.Stone))
            {
                if (c.slot.stone)
                {
                    potential += 10;
                }
            }
            if (LevelProfile.main.HasTarget(FieldTarget.Butterfly))
            {
                if (c.logic.GetChipType() == EPieces.Butterfly.ToString())
                {
                    potential += 10;
                }
            }
            if (LevelProfile.main.HasTarget(FieldTarget.Cage))
            {
                if (c.slot.block && c.slot.block is Caged)
                {
                    potential += 10;
                }
            }
            if (LevelProfile.main.HasTarget(FieldTarget.RandomChanger))
            {
                if (c.logic.GetChipType() == EPieces.Portion.ToString())
                {
                    potential += 10;
                }
            }
            if (LevelProfile.main.HasTarget(FieldTarget.IceBrick))
            {
                if (c.logic.GetChipType() == "IceChip")
                {
                    potential += 10;
                }
            }

        }
        return potential;
    }

    public List<Chip> GetDangeredChips(List<Chip> stack)
    {
        if (stack.Contains(this))
            return stack;

        stack = logic.GetDangeredChips(stack);
        return stack;
    }

    public int GetPotencial(int i)
    {
        return logic.GetPotencial();
    }

    #endregion
    ////////////////////////////////////////////////////////////////////////////////

    public static void Swap(Chip chip, Side side)
    {
        if (chip.slot && chip.slot[side])
        {
            Session.Instance.SwapByPlayer(chip, chip.slot[side].chip, false);
        }
    }

    public void HideChip(bool collection)
    {
        if (destroying) return;
        destroying = true;

        ParentRemove();

        StartCoroutine(HidingRoutine());
    }

    IEnumerator HidingRoutine()
    {
        yield return StartCoroutine(MinimizingRoutine());
        Destroy(gameObject);
    }

    public void Minimize()
    {
        StartCoroutine(MinimizingRoutine());
    }

    IEnumerator MinimizingRoutine()
    {
        while (true)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, Time.deltaTime * 6f);
            if (transform.localScale.x == 0)
            {
                yield break;
            }
            yield return 0;
        }
    }

    public void ParentRemove()
    {
        if (!slot) return;
        slot.chip = null;
        slot = null;
    }

    void OnDestroy()
    {
        IsBusy = false;
        Gravity = true;
    }

    public void DestroyChip()
    {
        if (!destroyable) return;
        if (destroying) return;
        if (slot && slot.block)
        {
            slot.block.BlockCrush(false);
            return;
        }
        destroying = true;

        StartCoroutine(DestroyChipRoutine());
    }

    IEnumerator DestroyChipRoutine()
    {

        yield return StartCoroutine(logic.Destroying());

        if (!destroying)
        {
            yield break;
        }

        if (IsColored())
        {
            Session.Instance.IncreaseCountColor(id);
        }

        Destroy(gameObject);
    }

    public void Flashing(int eventCount)
    {
        //TODO: flash for hint

    }

    public bool CanSuffle()
    {
        string chipType = logic.GetChipType();
        if (chipType == "SimpleChip")
            return true;
        return false;
    }

    public void SetScore(float s)
    {
        Session.Instance.score += Mathf.RoundToInt(s * GameConst.k_session_scoreChip);

        // TODO: show score anim
    }

    IEnumerator ChipPhysics()
    {
        while (true)
        {
            yield return 0;

            if (velocity > 0)
                velocity -= velocity * Mathf.Min(1f, Time.deltaTime * 3);

            if (!Session.Instance.isPlaying || IsBusy || destroying)
                continue;


            if (!slot)
            {
                if (!destroying)
                {
                    DestroyChip();
                }
                yield break;
            }

            if (velocity == 0 && impulse == Vector3.zero)
            {
                transform.Find("icon").transform.position = transform.position;
            }

            #region Gravity
            while (transform.localPosition != Vector3.zero || impulse != Vector3.zero)
            {

                IsBusy = true;

                if (destroying)
                    break;

                if (impulse == Vector3.zero)
                {
                    velocity += CommonConfig.main.chip_acceleration * Time.deltaTime;
                    if (velocity > CommonConfig.main.chip_max_velocity)
                        velocity = CommonConfig.main.chip_max_velocity;

                    lastPosition = transform.position;

                    if (Mathf.Abs(transform.localPosition.x) < velocity * Time.deltaTime)
                    {
                        transform.localPosition = Utils.ScaleVector(transform.localPosition, 0, 1, 0);
                    }

                    if (Mathf.Abs(transform.localPosition.y) < velocity * Time.deltaTime)
                    {
                        transform.localPosition = Utils.ScaleVector(transform.localPosition, 1, 0, 0);
                    }

                    if (transform.localPosition.magnitude < Time.deltaTime * 2f)
                    {
                        if (slot)
                        {
                            slot.slotGravity.GravityReaction();
                        }
                        if (transform.localPosition != Vector3.zero)
                        {
                            transform.position = lastPosition;
                        }
                        else
                        {
                            IsBusy = false;
                            movementID = Session.Instance.GetMovementID();
                            velocity *= 0.5f;
                            OnHit();
                            break;
                        }

                    }

                    Vector3 moveVector = new Vector3();
                    if (transform.localPosition.x < 0)
                        moveVector.x = 1;
                    if (transform.localPosition.x > 0)
                        moveVector.x = -1;
                    if (transform.localPosition.y < 0)
                        moveVector.y = 1;
                    if (transform.localPosition.y > 0)
                        moveVector.y = -1;
                    moveVector = moveVector.normalized * velocity;
                    transform.localPosition += moveVector * Time.deltaTime;
                }
                else
                {
                    //if (transform.localPosition.magnitude < CommonConfig.main.slot_offset)
                    //{
                    //    if (slot)
                    //    {
                    //        slot.slotGravity.GravityReaction();
                    //    }
                    //}
                    if (impulse.sqrMagnitude > 4 * 4)
                    {
                        impulse = impulse.normalized * 4;
                    }

                    transform.Find("icon").position += impulse * Time.deltaTime;
                    transform.Find("icon").position -= transform.Find("icon").localPosition * Time.deltaTime;
                    impulse -= impulse * Time.deltaTime * 2f;
                    impulse -= transform.Find("icon").localPosition;
                    impulse *= Mathf.Max(0, 1f - Time.deltaTime * 4f);

                    if (impulse.magnitude < Time.deltaTime * 2f)
                    {
                        impulse = Vector3.zero;
                        transform.Find("icon").position = transform.position;
                        IsBusy = false;
                        break;
                    }
                }

                yield return 0;
                IsBusy = false;
            }
            #endregion
        }
    }

    public void OnHit()
    {
        // animate ("Hit");
        // Audio play("ChipHit");
        if (m_checkHit)
        {
            m_checkHit = false;
            return;
        }

        if (m_animation)
        {

            if (!m_animation.isPlaying || m_animation.IsPlaying("Idle"))
            {
                transform.Find("icon").transform.position = transform.position;
                //PlayAnim("Hit");
                StartCoroutine(PlayAnimHit());
            }
        }
        SoundManager.PlaySFX(SoundDefine.k_touch);

    }

    IEnumerator PlayAnimHit()
    {
        //PlayAnim("Hit");
        if (m_animation != null && m_animation.GetClip("Hit"))
        {
            m_animation.Play("Hit");
            yield return new WaitForSeconds(m_animation.GetClip("Hit").length);
            if (m_animation != null && m_animation.GetClip("Idle"))
            {
                m_animation.Play("Idle");
            }
        }
    }

    public void PlayAnim(string name)
    {
        if (m_animation != null && m_animation.GetClip(name))
            m_animation.Play(name);
    }

    public bool IsPlaying(string name)
    {
        if (m_animation != null)
            return m_animation.IsPlaying(name);
        else
            return false;
    }

    public float TimePlaying(string name)
    {
        if (m_animation != null)
            return m_animation.GetClip(name).length;
        else
            return 0f;
    }

    public void CompleteAnim(string name)
    {
        if (m_animation != null)
            m_animation[name].time = 0;
    }

    public void ChangeColor()
    {
        if (IsColored())
        {
            if (colorChangeable != null)
            {
                int newid = (colorChangeable).ChangeColor();
                if (newid != id)
                {
                    id = newid;
                    movementID = Session.Instance.GetMovementID();
                }
            }
        }
    }
}

public abstract class IBomb : MonoBehaviour
{

}

public interface IChipLogic
{
    IEnumerator Destroying();
    string GetChipType();
    List<Chip> GetDangeredChips(List<Chip> stack);
    bool IsMatchable();
    int GetPotencial();

    Chip chip { get; }
}

public interface IChipAffectByNeighBor
{
    bool IsCanEffectedByNeighbor();
    Chip NeighborChip { get; set; }
}

public interface IChipColorChangeable
{
    int ChangeColor();
}
