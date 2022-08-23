using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollMagic : MonoBehaviour
{
    int eventCountBorn = 0;

    List<ScrollTile> m_magicScrolls = new List<ScrollTile>();

    ////////////////////////////////////////////////////////////////////////////////
    private void Start()
    {
        eventCountBorn = Session.Instance.eventCount;
    }
    void GatherScrollTiles(ScrollTile headTile)
    {
        MagicScrollInfo info = headTile.scrollInfo;
        Side dir = Side.Null;
        if (info.Dir == EScrollDir.UP.ToString())
        {
            dir = Side.Top;
        }
        else if (info.Dir == EScrollDir.DOWN.ToString())
        {
            dir = Side.Bottom;
        }
        else if (info.Dir == EScrollDir.LEFT.ToString())
        {
            dir = Side.Left;
        }
        else if (info.Dir == EScrollDir.RIGHT.ToString())
        {
            dir = Side.Right;
        }

        string type = info.Type;
        string endType = EScrollType.END.ToString();
        Slot slot = headTile.slot;

        m_magicScrolls.Add(headTile);
        while (type != endType)
        {
            slot = Slot.GetSlot(Utils.Vec2IntAdd(slot.coord, dir));
            if (slot)
            {
                ScrollTile scroll = slot.block as ScrollTile;
                if (scroll)
                {
                    m_magicScrolls.Add(scroll);
                    type = scroll.scrollInfo.Type;
                    scroll.magicScroll = this;
                }
            }
        }

    }

    internal void Crush()
    {
        if (eventCountBorn == Session.Instance.eventCount) return;

        eventCountBorn = Session.Instance.eventCount;
        if (m_magicScrolls.Count > 1)
        {
            ScrollTile tail = m_magicScrolls[m_magicScrolls.Count - 1];

            tail.slot.block = null;
            m_magicScrolls.Remove(tail);
            SlotGravity.Reshading();
            Destroy(tail.gameObject);

            ScrollTile newTail = m_magicScrolls[m_magicScrolls.Count - 1];
            newTail.SetTail();
        }
        else
        {
            m_magicScrolls[0].slot.block = null;
            SlotGravity.Reshading();
            StartCoroutine(DestroyingRoutine());
        }
    }

    IEnumerator DestroyingRoutine()
    {
        int ptarget = LevelProfile.main.GetTargetCount(0, FieldTarget.MagicScroll);

        if (ptarget > 0)
        {
            TargetUI[] listO = GameObject.FindObjectsOfType<TargetUI>();

            TargetUI go = null;

            for (int i = 0; i < listO.Length; i++)
            {
                if (listO[i].GetLvlTarget().GetTarget().Equals(FieldTarget.MagicScroll))
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
        Session.Instance.CollectScroll();
        Destroy(gameObject);
    }

    internal static void Initialize()
    {
        foreach (var slot in Slot.all.Values)
        {
            if (slot.block)
            {
                if (slot.block is ScrollTile)
                {
                    ScrollTile tile = (ScrollTile)(slot.block);
                    if (tile.scrollInfo.Type == EScrollType.BEGIN.ToString())
                    {
                        tile.magicScroll.GatherScrollTiles(tile);
                    }
                }
            }
        }
    }
}

[System.Serializable]
public class MagicScrollInfo
{
    public string Type;
    public string Dir;

    public MagicScrollInfo()
    {
        Type = EScrollType.BEGIN.ToString();
        Dir = EScrollDir.DOWN.ToString();
    }

    public MagicScrollInfo(string type, string dir)
    {
        Type = type;
        Dir = dir;
    }

    public override string ToString()
    {
        return "TYPE: " + Type + ", DIR: " + Dir;
    }
}

public enum EScrollType
{
    BEGIN,
    MIDDLE,
    END
}

public enum EScrollDir
{
    UP, DOWN, LEFT, RIGHT
}