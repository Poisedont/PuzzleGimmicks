using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour, IChipLogic, IChipAffectByNeighBor
{
    protected Chip m_chip;
    public Chip chip
    {
        get { return m_chip; }
    }

    public Chip NeighborChip { get; set; }

    int m_eventBirth;
    [SerializeField] SpriteRenderer m_render;
    [SerializeField] Sprite[] m_coloredSprites;
    ////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        m_chip = GetComponent<Chip>();
        if (!m_render)
            m_render = GetComponentInChildren<SpriteRenderer>();

        m_eventBirth = Session.Instance.eventCount;
    }

    public IEnumerator Destroying()
    {
        if (m_eventBirth == Session.Instance.eventCount)
        {
            chip.destroying = false;
            yield break;
        }

        if (chip.IsUncolored())
        {
            if (NeighborChip && NeighborChip.chipType == GetChipType())
            {
                chip.destroying = false;
                yield break;
            }
            chip.destroying = false; //cancel destroy
            m_eventBirth = Session.Instance.eventCount;
            chip.IsBusy = true;
            // change to matching color
            if (NeighborChip)
            {
                ChangeColor(NeighborChip.id);
            }

            yield return new WaitForSeconds(0.2f); // wait for change color

            chip.IsBusy = false;
        }
        else
        {
            // destroy like a simple chip
            chip.IsBusy = true;
            yield return null;

            chip.ParentRemove();

            int ptarget = LevelProfile.main.GetTargetCount((int)BlockerTargetType.Crystal, FieldTarget.Blocker);

            if (ptarget > 0)
            {
                TargetUI[] listO = GameObject.FindObjectsOfType<TargetUI>();

                TargetUI go = null;

                for (int i = 0; i < listO.Length; i++)
                {
                    if (listO[i].GetLvlTarget().GetTarget().Equals(FieldTarget.Blocker))
                    {
                        if (listO[i].GetLvlTarget().GetTargetCount((int)BlockerTargetType.Crystal) > 0
                        && listO[i].GetIndexTaret() == (int)BlockerTargetType.Crystal
                        && listO[i].GetLvlTarget().GetTargetCount((int)BlockerTargetType.Crystal) > listO[i].GetLvlTarget().GetCurrentCount((int)BlockerTargetType.Crystal)
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

            Session.Instance.CollectCrystal();

            // animation chip destroy and collect
            chip.PlayAnim("destroying");

            yield return new WaitForSeconds(chip.TimePlaying("destroying"));

            // chip.CompleteAnim("destroying");

            SoundManager.PlaySFX(SoundDefine.k_3m);
        }
    }

    public string GetChipType()
    {
        return EPieces.Crystal.ToString();
    }

    public List<Chip> GetDangeredChips(List<Chip> stack)
    {
        return stack;
    }

    public int GetPotencial()
    {
        return 1;
    }

    public bool IsMatchable()
    {
        return chip.IsColored();
    }

    void ChangeColor(int chipColor)
    {
        int clIdx = Mathf.Clamp((int)chipColor, 0, m_coloredSprites.Length);
        m_render.sprite = m_coloredSprites[clIdx];

        chip.id = clIdx;
    }

    public bool IsCanEffectedByNeighbor()
    {
        return chip.IsUncolored();
    }
}