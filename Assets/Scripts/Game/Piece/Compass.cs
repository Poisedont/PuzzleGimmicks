using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour, IChipLogic, IChipAffectByNeighBor
{
    protected Chip m_chip;
    [SerializeField] int m_level = 1;
    [SerializeField] SpriteRenderer m_spriteRender;
    [SerializeField] Sprite[] m_sprites;

    int m_eventBirth;

    public Chip chip
    {
        get { return m_chip; }
    }

    public Chip NeighborChip { get; set; }

    ////////////////////////////////////////////////////////////////////////////////
    void Awake()
    {
        m_chip = GetComponent<Chip>();
        m_spriteRender = transform.Find("icon").GetComponent<SpriteRenderer>();

    }

    private void Start()
    {
        m_eventBirth = Session.Instance.eventCount;
        m_spriteRender.sprite = m_sprites[m_level - 1];
    }

    public string GetChipType()
    {
        return EPieces.Compass.ToString();
    }

    public List<Chip> GetDangeredChips(List<Chip> stack)
    {
        stack.Add(chip);
        return stack;
    }

    public int GetPotencial()
    {
        return 1;
    }

    public bool IsMatchable()
    {
        return false;
    }

    public IEnumerator Destroying()
    {
        if (Session.Instance.eventCount == m_eventBirth)
        {
            chip.destroying = false;
            yield break;
        }

        chip.IsBusy = true;
        // TODO: Audio play("Compass");

        m_level--;
        if (m_level > 0)
        {
            chip.destroying = false;
            m_spriteRender.sprite = m_sprites[m_level - 1];
            chip.IsBusy = false;

            m_eventBirth = Session.Instance.eventCount;
        }
        else
        {
            // chip.Play("Destroying");

            yield return new WaitForSeconds(0.1f);
            chip.IsBusy = false;

            chip.SetScore(1);

            chip.ParentRemove();

            // show crush effect
            GameObject o = ContentManager.Instance.GetItem("CompassCrush");
            if (o) o.transform.position = transform.position;

            int ptarget = LevelProfile.main.GetTargetCount((int)BlockerTargetType.Compass, FieldTarget.Blocker);

            if (ptarget > 0)
            {
                TargetUI[] listO = GameObject.FindObjectsOfType<TargetUI>();

                TargetUI go = null;

                for (int i = 0; i < listO.Length; i++)
                {
                    if (listO[i].GetLvlTarget().GetTarget().Equals(FieldTarget.Blocker))
                    {
                        if (listO[i].GetLvlTarget().GetTargetCount((int)BlockerTargetType.Compass) > 0
                        && listO[i].GetIndexTaret() == (int)BlockerTargetType.Compass
                        && listO[i].GetLvlTarget().GetTargetCount((int)BlockerTargetType.Compass) > listO[i].GetLvlTarget().GetCurrentCount((int)BlockerTargetType.Compass)
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

            Session.Instance.CollectCompass();
            Destroy(gameObject);
            // while (chip.IsPlaying("Destroying"))
            //     yield return 0;
        }
    }

    public bool IsCanEffectedByNeighbor()
    {
        return true;
    }

}