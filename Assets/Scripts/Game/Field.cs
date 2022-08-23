using System.Collections.Generic;
using UnityEngine;

public class Field
{
    public int width;
    public int height;
    public int colorCount;

    public int blockerSpawnCount;

    public Dictionary<Vector2Int, SlotSettings> slots = new Dictionary<Vector2Int, SlotSettings>();

    public Field(LevelProfile profile)
    {
        width = profile.width;
        height = profile.height;
        colorCount = profile.colorCount;
        foreach (SlotSettings slot in profile.slots)
        {
            if (!slots.ContainsKey(slot.position))
            {
                if (slot.visible)
                {
                    slots.Add(slot.position, slot.GetClone());
                }

                if (slot.tags.Contains(GameConst.k_blockGenerator_tag))
                {
                    blockerSpawnCount++;
                }
            }
        }
        FirstChipGeneration();
    }

    public SlotSettings GetSlot(Vector2Int pos)
    {
        if (slots.ContainsKey(pos))
            return slots[pos];
        return null;
    }

    public SlotSettings GetSlot(int x, int y)
    {
        return GetSlot(new Vector2Int(x, y));
    }

    int NewRandomChip(Vector2Int coord)
    {
        List<int> ids = new List<int>();
        for (int i = 0; i < colorCount; i++)
        {
            if (LevelProfile.main.easyColorRatio[i] > 0)
            {
                ids.Add(i + 1);
            }
        }

        foreach (Side side in Utils.straightSides)
        {
            Vector2Int key = Utils.Vec2IntAdd(coord, side);
            if (slots.ContainsKey(key) && ids.Contains(slots[key].color_id))
                ids.Remove(slots[key].color_id);
        }

        if (ids.Count > 0)
            return ids.GetRandom();
        else
            return LevelProfile.main.GetColorRandom();
    }

    public void FirstChipGeneration()
    {
        // replace random chips on nonrandom
        foreach (Vector2Int pos in slots.Keys)
        {
            if (slots[pos].color_id == 0)
            {
                slots[pos].color_id = NewRandomChip(pos);
            }
        }

        // reduce color_id by 1 to match with color array
        foreach (Vector2Int pos in slots.Keys)
        {
            if (slots[pos].color_id > 0)
            {
                slots[pos].color_id--;
            }
        }

        int[] a = new int[Chip.colors.Length];
        // a => 0, 1, 2, 3, 4...
        for (int i = 0; i < a.Length; i++)
        {
            a[i] = i;
        }

        /* // make random color mask by swap 'a' array elements
        for (int i = Chip.colors.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i);
            a[j] = a[j] + a[i];
            a[i] = a[j] - a[i];
            a[j] = a[j] - a[i];
        } */

        Session.Instance.colorMask = a;

        // apply the results to the matrix shuffling chips	
        foreach (Vector2Int pos in slots.Keys)
        {
            if (slots[pos].color_id >= 0 && slots[pos].color_id < a.Length)
            {
                slots[pos].color_id = a[slots[pos].color_id];
            }
        }
    }
}

