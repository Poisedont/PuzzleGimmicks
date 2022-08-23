using System.Collections.Generic;
using UnityEngine;

public class ConveyorTile : MonoBehaviour
{
    [SerializeField] SpriteRenderer m_spriteRender;
    [SerializeField] Sprite[] m_spinnerCWSprites; //clockwise
    [SerializeField] Sprite[] m_spinnerCCWSprites; //counter-clockwise
    [SerializeField] Sprite[] m_conveyorSprites;
    ConveyorInfo m_info;

    public Slot slot { get; set; }

    public ConveyorTile headTile = null; //headTile of linked list
    public ConveyorTile nextTile = null; //work like linked list

    bool blocking = false; //when a block in on conveyor

    public static List<ConveyorTile> s_all;
    private int m_lastEventWorking;

    public void SetConveyorInfo(ConveyorInfo info)
    {
        m_info = info;
        SetupConveyor();
    }

    void SetupConveyor()
    {
        if (m_info != null)
        {
            EConveyorImg imgType = m_info.GetConveyorImg();
            EScrollDir inDir = m_info.GetDirIn();
            EScrollDir outDir = m_info.GetDirOut();
            // get correct image from info
            int imgIdx = -1;
            float rotate = 0;
            if (imgType == EConveyorImg.PLATE_CLOCKWISE)
            {
                if (inDir == EScrollDir.LEFT && outDir == EScrollDir.DOWN)
                {
                    imgIdx = 0;
                }
                else if (inDir == EScrollDir.DOWN && outDir == EScrollDir.RIGHT)
                {
                    imgIdx = 1;
                }
                else if (inDir == EScrollDir.RIGHT && outDir == EScrollDir.UP)
                {
                    imgIdx = 2;
                }
                else if (inDir == EScrollDir.UP && outDir == EScrollDir.LEFT)
                {
                    imgIdx = 3;
                }
                if (m_spriteRender && imgIdx >= 0)
                {
                    m_spriteRender.sprite = m_spinnerCWSprites[imgIdx];
                }

            }
            else if (imgType == EConveyorImg.PLATE_COUNTERCLOCKWISE)
            {
                if (inDir == EScrollDir.DOWN && outDir == EScrollDir.LEFT)
                {
                    imgIdx = 0;
                }
                else if (inDir == EScrollDir.RIGHT && outDir == EScrollDir.DOWN)
                {
                    imgIdx = 1;
                }
                else if (inDir == EScrollDir.UP && outDir == EScrollDir.RIGHT)
                {
                    imgIdx = 2;
                }
                else if (inDir == EScrollDir.LEFT && outDir == EScrollDir.UP)
                {
                    imgIdx = 3;
                }

                if (m_spriteRender && imgIdx >= 0)
                {
                    m_spriteRender.sprite = m_spinnerCCWSprites[imgIdx];
                }
            }
            else if (imgType == EConveyorImg.DEFAULT)
            {
                if (inDir == EScrollDir.LEFT && outDir == EScrollDir.RIGHT
                    || inDir == EScrollDir.RIGHT && outDir == EScrollDir.LEFT
                    || inDir == EScrollDir.UP && outDir == EScrollDir.DOWN
                    || inDir == EScrollDir.DOWN && outDir == EScrollDir.UP
                    )
                {
                    imgIdx = 0;
                    // defautl is LEFT -> RIGHT
                    if (inDir == EScrollDir.UP)
                    {
                        rotate = -90;
                    }
                    else if (inDir == EScrollDir.DOWN)
                    {
                        rotate = 90;
                    }
                    else if (inDir == EScrollDir.RIGHT)
                    {
                        rotate = 180;
                    }
                }
                else
                {
                    //conveyor corner

                    imgIdx = -1; // deactive main render
                    if (m_spriteRender)
                    {
                        m_spriteRender.gameObject.SetActive(false);
                    }

                    // find child obj that correct with in-out
                    string objName = (inDir.ToString() + "_" + outDir.ToString()).ToLower();
                    var obj = gameObject.transform.Find(objName);
                    if (obj)
                    {
                        obj.gameObject.SetActive(true);
                    }
                }

                if (m_spriteRender && imgIdx >= 0)
                {
                    m_spriteRender.sprite = m_conveyorSprites[imgIdx];
                }
            }

            if (m_spriteRender)
            {
                if (imgIdx >= 0)
                {
                    m_spriteRender.gameObject.SetActive(true);
                    m_spriteRender.transform.rotation = Quaternion.AngleAxis(rotate, Vector3.forward);
                }
            }
        }
    }

    private void Awake()
    {
        if (s_all == null)
        {
            s_all = new List<ConveyorTile>();
        }
        s_all.Add(this);

        m_lastEventWorking = Session.Instance.swapEvent;
    }
    public Slot GetSlotAtOut()
    {
        EScrollDir outDir = m_info.GetDirOut();
        Side dir = GetSideFromDir(outDir);

        Slot slotOut = Slot.GetSlot(Utils.Vec2IntAdd(slot.coord, dir));

        return slotOut;

    }

    public Side GetSideFromDir(EScrollDir inDir)
    {
        Side dir = Side.Null;
        if (inDir == EScrollDir.LEFT)
        {
            dir = Side.Left;
        }
        else if (inDir == EScrollDir.RIGHT)
        {
            dir = Side.Right;
        }
        else if (inDir == EScrollDir.UP)
        {
            dir = Side.Top;
        }
        else if (inDir == EScrollDir.DOWN)
        {
            dir = Side.Bottom;
        }
        return dir;
    }

    public Slot GetSlotAtIn()
    {
        EScrollDir inDir = m_info.GetDirIn();
        Side dir = GetSideFromDir(inDir);

        Slot slotOut = Slot.GetSlot(Utils.Vec2IntAdd(slot.coord, dir));

        return slotOut;
    }

    ConveyorTile FindHead()
    {
        var slotIn = GetSlotAtIn();
        if (slotIn)
        {
            var tile = slotIn.GetComponentInChildren<ConveyorTile>();
            if (tile)
            {
                if (tile.headTile)
                {
                    tile.nextTile = this;
                    return tile.headTile;
                }
                if (tile.nextTile != null && tile.nextTile == this)
                {
                    return this;
                }

                tile.nextTile = this;

                var head = tile.FindHead();
                tile.headTile = head;
                return head;
            }
        }
        return this;
    }

    public static void Initialize()
    {
        if (s_all != null)
        {
            foreach (var tile in s_all)
            {
                if (tile.headTile == null) //it mean this tile has not in conveyor group
                {
                    tile.headTile = tile.FindHead();
                }
            }

            //remove all tiles except head tile for reference
            s_all.RemoveAll(t => t.headTile != t);

            //debug head tile
            foreach (var tile in s_all)
            {
                Debug.Log("headTile: " + tile.gameObject.name);
            }
        }
    }

    public static void Cleanup()
    {
        if (s_all != null)
        {
            s_all.Clear();
        }
    }

    float m_timeWaiting = 0;
    private void Update()
    {
        if (headTile != this) return; //child node don't need update

        if (m_lastEventWorking < Session.Instance.swapEvent)
        {

            if (!Session.Instance.CanIWait()) return;

            m_timeWaiting += Time.deltaTime;
            if (m_timeWaiting <= 0.5f) return;

            CheckBlocker();

            if (headTile.blocking)
            {
                m_lastEventWorking = Session.Instance.swapEvent;
                m_timeWaiting = 0;
                return;
            }

            //due to player can make swap while chip moving, so need to transfer every swap 
            m_lastEventWorking++;
            m_timeWaiting = 0;

            ConveyorTile tile = headTile;
            Chip nextChip = null;
            Chip currentChip = tile.slot.chip;
            if (currentChip) currentChip.m_checkHit = true;
            while (tile != null && tile.nextTile != headTile) // not the tail
            {
                if (tile.nextTile)
                {
                    nextChip = tile.nextTile.slot.chip; //store chip of next node
                    tile.nextTile.slot.chip = currentChip; // and assign currentchip to next node
                    currentChip = nextChip; // save current chip of next node to continue transfer
                }

                if (currentChip) currentChip.m_checkHit = true;
                tile = tile.nextTile;
            }

            //at tail node, its chip still not transfer to head, so transfer last chip
            headTile.slot.chip = currentChip;
            //set position of head's chip to in slot
            if (headTile.slot.chip)
            {
                EScrollDir inDir = m_info.GetDirIn();
                Side dir = GetSideFromDir(inDir);
                Vector2Int sideOffset = Utils.GetSideOffset(dir);
                Vector3 offset = new Vector3(sideOffset.x, sideOffset.y, 1) * CommonConfig.main.slot_offset;

                headTile.slot.chip.transform.position = headTile.slot.transform.position + offset;
            }
        }
    }

    void CheckBlocker()
    {
        ConveyorTile tile = headTile;
        while (tile != null && tile.nextTile != headTile)
        {
            if (tile.slot.block)
            {
                headTile.blocking = true;
                return;
            }

            tile = tile.nextTile;
        }
        //check last tile
        if (tile && tile.slot.block)
        {
            headTile.blocking = true;
            return;
        }
        else
        {
            headTile.blocking = false; // all tile has no block
        }

    }
}