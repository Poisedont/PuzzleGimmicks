using UnityEngine;

public class ScrollTile : IBlock
{
    public Sprite[] sprites; // Images of blocks of different type: begin, middle, end
    [HideInInspector] public ScrollMagic magicScroll;
    SpriteRenderer spriteRender;

    public MagicScrollInfo scrollInfo;

    public override void BlockCrush(bool force)
    {
        if (magicScroll)
        {
            magicScroll.Crush();
        }
    }

    public override bool CanBeCrushedByNearSlot(Chip near = null)
    {
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

    public override bool IsCastShadow() { return false; }

    public override void Initialize(SlotSettings settings = null)
    {
        spriteRender = GetComponentInChildren<SpriteRenderer>();

        if (settings != null && settings.scrollInfo != null)
        {
            scrollInfo = settings.scrollInfo;
            if (scrollInfo.Type == EScrollType.BEGIN.ToString())
            {
                SetHead();
                magicScroll = gameObject.AddComponent<ScrollMagic>(); //add ScrollMagic that hold all to the head 
            }
            else if (scrollInfo.Type == EScrollType.END.ToString())
            {
                SetTail();
            }
            else
            {// default sprite is middle
            }

            //rotate with default dir if LEFT
            if (scrollInfo.Dir == EScrollDir.UP.ToString())
            {
                transform.Rotate(0, 0, -90);
            }
            else if (scrollInfo.Dir == EScrollDir.RIGHT.ToString())
            {
                transform.Rotate(0, 0, 180);
            }
            else if (scrollInfo.Dir == EScrollDir.DOWN.ToString())
            {
                transform.Rotate(0, 0, 90);
            }

        }
    }

    public void SetTail()
    {
        spriteRender.sprite = sprites[sprites.Length - 1];
    }

    public void SetHead()
    {
        spriteRender.sprite = sprites[0];
    }
}