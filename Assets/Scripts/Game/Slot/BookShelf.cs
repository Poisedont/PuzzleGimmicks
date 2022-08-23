using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookShelf : IBlock
{
    [SerializeField] SpriteRenderer m_renderer;
    [SerializeField] SpriteRenderer m_book;
    // [SerializeField]
    BookShelfGroup m_bookGroup;
    private int eventCountBorn;

    public override void BlockCrush(bool force)
    {
        if (eventCountBorn == Session.Instance.eventCount && !force) return;
        if (m_bookGroup != null)
            m_bookGroup.GenerateBook();
        eventCountBorn = Session.Instance.eventCount;
    }

    public override bool CanBeCrushedByNearSlot(Chip near = null)
    {
        if (near == null)
        {
            return false;
        }
        return true;
    }

    public override bool CanItContainChip()
    {
        return false;
    }

    public override int GetLevels()
    {
        return 1;
    }

    public override void Initialize(SlotSettings settings = null)
    {
        if (settings.bookShelfInfo != null)
        {
            int shelfIndex = settings.bookShelfInfo.Index;
            if (m_renderer)
            {
                m_renderer.gameObject.SetActive(shelfIndex == 0);
            }
            if (shelfIndex == 0)
            {
                m_bookGroup = new BookShelfGroup(this);
            }
        }

    }

    public override bool IsCastShadow()
    {
        return base.IsCastShadow();
    }

    public BookShelfGroup GetBookShelfGroup()
    {
        return m_bookGroup;
    }
    public void SetBookShelfGroup(BookShelfGroup group)
    {
        m_bookGroup = group;
    }

    public void StartBookfly()
    {
        StartCoroutine(BookFly());
    }

    IEnumerator BookFly()
    {
        GameObject o = ContentManager.Instance.GetItem("SmokeInSlot");
        o.transform.position = transform.position;

        ParticleSystem effect = o.GetComponent<ParticleSystem>();

        effect.Play();

        int ptarget = LevelProfile.main.GetTargetCount(0, FieldTarget.FixBlock);

        if (ptarget > 0)
        {
            TargetUI[] listO = GameObject.FindObjectsOfType<TargetUI>();

            TargetUI go = null;

            for (int i = 0; i < listO.Length; i++)
            {
                if (listO[i].GetLvlTarget().GetTarget().Equals(FieldTarget.FixBlock))
                {
                    if (listO[i].GetLvlTarget().GetTargetCount(0) > 0
                    && listO[i].GetIndexTaret() == 0
                    && listO[i].GetLvlTarget().GetTargetCount(0) > listO[i].GetLvlTarget().GetCurrentCount(0)
                    )
                    {
                        go = listO[i];

                    }
                }
            }

            if (go)
            {
                Transform target = go.transform;
                //m_book.gameObject.SetActive(true);
                GameObject book = Instantiate(m_book.gameObject);
                book.SetActive(true);
                //sprite.sortingLayerName = "UI";
                //sprite.sortingOrder = 10;

                float time = 0;
                float speed = Random.Range(1f, 1.8f);
                Vector3 startPosition = transform.position;
                Vector3 targetPosition = target.position;

                while (time < 1)
                {
                    book.transform.position = Vector3.Lerp(startPosition, targetPosition, EasingFunctions.easeInOutQuad(time));
                    time += Time.unscaledDeltaTime * speed;
                    yield return 0;
                }

                book.transform.position = target.position;
                book.SetActive(false);
                Destroy(book);
            }
        }
        
        //m_book.gameObject.SetActive(false);
        //yield return new WaitForSeconds(effect.duration);
        Session.Instance.CollectBook();
        effect.Stop();
        Destroy(o);

        eventCountBorn = Session.Instance.eventCount;
    }
}

[System.Serializable]
public class BookShelfGroup
{
    static List<BookShelfGroup> s_allGroups;
    BookShelf[] m_bookShelfSlots;
    private int eventCountBorn;

    public BookShelfGroup(BookShelf shelf0)
    {
        if (s_allGroups == null)
        {
            s_allGroups = new List<BookShelfGroup>();
        }
        s_allGroups.Add(this);
        m_bookShelfSlots = new BookShelf[GameConst.k_blocker_group_slot_count];
        m_bookShelfSlots[0] = shelf0;
    }

    public bool AddToGroup(BookShelf shelf, int index)
    {
        if (index > 0 && index < m_bookShelfSlots.Length)
        {
            m_bookShelfSlots[index] = shelf;
            shelf.SetBookShelfGroup(this);
            return true;
        }

        return false;
    }

    public void GenerateBook()
    {
        if (eventCountBorn == Session.Instance.eventCount) return;

        // make anim for book
        m_bookShelfSlots[2].StartBookfly();
    }

    

    public static void Initialize()
    {

        if (s_allGroups != null)
        {
            foreach (var group in s_allGroups)
            {
                group.InitGroup();
            }
        }
    }

    public static void Cleanup()
    {
        if (s_allGroups != null)
        {
            s_allGroups.Clear();
        }
    }

    static Side[] s_bookSides = new Side[] { Side.Right, Side.Top, Side.TopRight };

    void InitGroup()
    {
        /* 
        | 2 | 3 |
        | 0 | 1 |
        */
        if (m_bookShelfSlots[0])
        {
            Vector2Int coord = m_bookShelfSlots[0].slot.coord;
            Slot slot = null;
            for (int i = 0; i < s_bookSides.Length; i++)
            {
                slot = Slot.GetSlot(Utils.Vec2IntAdd(coord, s_bookSides[i]));

                if (slot)
                {
                    int shelfIndex = i + 1;
                    var bookShelf = slot.block as BookShelf;
                    if (bookShelf)
                    {
                        AddToGroup(bookShelf, shelfIndex);
                    }
                }
            }
        }
    }
}

// TODO: refactor
[System.Serializable]
public class BookShelfInfo
{
    public int Index;
}