
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlotSettings
{
    public bool visible = true; //this slot will show in field
    public Vector2Int position = new Vector2Int();
    public Side gravity = Side.Bottom;
    public bool generator = false;
    public Vector2Int teleport = Utils.Vector2IntNull;
    public string chip = "";
    public int color_id = 0;
    public int stone_level = -1;
    public string jam = "";
    public string block_type = "";
    public int block_level = 0;

    public MagicScrollInfo scrollInfo;
    public BookShelfInfo bookShelfInfo;
    public ConveyorInfo conveyorInfo;
    public SwitcherInfo switcherInfo;

    public List<string> tags = new List<string>();

    public SlotSettings(int _x, int _y)
    {
        position = new Vector2Int(_x, _y);
    }

    public SlotSettings(Vector2Int _position)
    {
        position = _position;
    }

    public SlotSettings GetClone()
    {
        return MemberwiseClone() as SlotSettings;
    }

    public SlotSettings GetDeepCopy()
    {
        SlotSettings setting = MemberwiseClone() as SlotSettings;
        setting.tags = new List<string>(this.tags);
        if (this.scrollInfo != null)
        {
            setting.scrollInfo = new MagicScrollInfo(this.scrollInfo.Type, this.scrollInfo.Dir);
        }

        if (this.bookShelfInfo != null)
        {
            setting.bookShelfInfo = new BookShelfInfo()
            {
                Index = this.bookShelfInfo.Index
            };
        }
        if (this.conveyorInfo != null)
        {
            setting.conveyorInfo = this.conveyorInfo.GetClone();
        }

        if (this.switcherInfo != null)
        {
            setting.switcherInfo = this.switcherInfo.GetClone();
        }

        return setting;
    }
}