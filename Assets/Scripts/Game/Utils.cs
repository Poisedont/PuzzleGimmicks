using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static Vector2Int Vector2IntNull = new Vector2Int(int.MinValue, int.MinValue);
    #region Side utilities
    // all side follow clockwise
    public static readonly Side[] allSides = {
                                        Side.Top,
                                        Side.TopRight,
                                        Side.Right,
                                        Side.BottomRight,
                                        Side.Bottom,
                                        Side.BottomLeft,
                                        Side.Left,
                                        Side.TopLeft
                                    };
    public static readonly Side[] straightSides = { Side.Top, Side.Bottom, Side.Right, Side.Left };
    public static readonly Side[] slantedSides = { Side.TopLeft, Side.TopRight, Side.BottomRight, Side.BottomLeft };

    public static Vector2Int Vec2IntAdd(Vector2Int vector2, Side side)
    {
        return vector2 + GetSideOffset(side);
    }

    public static Vector2Int GetSideOffset(Side side)
    {
        switch (side)
        {
            case Side.Right: return Vector2Int.right;
            case Side.TopRight: return Vector2Int.one;
            case Side.Top: return Vector2Int.up;
            case Side.TopLeft: return new Vector2Int(-1, 1);
            case Side.Left: return Vector2Int.left;
            case Side.BottomLeft: return new Vector2Int(-1, -1);
            case Side.Bottom: return Vector2Int.down;
            case Side.BottomRight: return new Vector2Int(1, -1);
            default: return Vector2Int.zero;
        }
    }

    public static Side RotateSide(Side side, int steps)
    {
        int index = Array.IndexOf(allSides, side);
        index += steps;
        index = Mathf.CeilToInt(Mathf.Repeat(index, allSides.Length));
        return allSides[index];
    }

    public static Side MirrorSide(Side s)
    {
        switch (s)
        {
            case Side.Bottom: return Side.Top;
            case Side.Top: return Side.Bottom;
            case Side.Left: return Side.Right;
            case Side.Right: return Side.Left;
            case Side.BottomLeft: return Side.TopRight;
            case Side.BottomRight: return Side.TopLeft;
            case Side.TopLeft: return Side.BottomRight;
            case Side.TopRight: return Side.BottomLeft;
        }
        return Side.Null;
    }

    public static int SideOffsetX(Side s)
    {
        switch (s)
        {
            case Side.Top:
            case Side.Bottom:
                return 0;
            case Side.TopLeft:
            case Side.BottomLeft:
            case Side.Left:
                return -1;
            case Side.BottomRight:
            case Side.TopRight:
            case Side.Right:
                return 1;
        }
        return 0;
    }

    public static int SideOffsetY(Side s)
    {
        switch (s)
        {
            case Side.Left:
            case Side.Right:
                return 0;
            case Side.Bottom:
            case Side.BottomRight:
            case Side.BottomLeft:
                return -1;
            case Side.TopLeft:
            case Side.TopRight:
            case Side.Top:
                return 1;
        }
        return 0;
    }

    public static Side SideHorizontal(Side s)
    {
        switch (s)
        {
            case Side.Left:
            case Side.TopLeft:
            case Side.BottomLeft:
                return Side.Left;
            case Side.Right:
            case Side.TopRight:
            case Side.BottomRight:
                return Side.Right;
            default:
                return Side.Null;
        }
    }

    public static Side SideVertical(Side s)
    {
        switch (s)
        {
            case Side.Top:
            case Side.TopLeft:
            case Side.TopRight:
                return Side.Top;
            case Side.Bottom:
            case Side.BottomLeft:
            case Side.BottomRight:
                return Side.Bottom;
            default:
                return Side.Null;
        }
    }

    public static float SideToAngle(Side s)
    {
        switch (s)
        {
            case Side.Right: return 0;
            case Side.TopRight: return 45;
            case Side.Top: return 90;
            case Side.TopLeft: return 135;
            case Side.Left: return 180;
            case Side.BottomLeft: return 225;
            case Side.Bottom: return 270;
            case Side.BottomRight: return 315;
            default: return 0;
        }
    }
    #endregion

    /// <summary>
    /// Return random value from a collection
    /// </summary>
    public static T GetRandom<T>(this ICollection<T> collection)
    {
        if (collection == null)
            return default(T);
        int t = UnityEngine.Random.Range(0, collection.Count);
        foreach (T element in collection)
        {
            if (t == 0)
                return element;
            t--;
        }
        return default(T);
    }

    // Coroutine wait until the function "Action" will be true for a "delay" seconds
    public static IEnumerator WaitFor(Func<bool> Action, float delay)
    {
        float time = 0;
        while (time <= delay)
        {
            if (Action())
                time += Time.deltaTime;
            else
                time = 0;
            yield return 0;
        }
        yield break;
    }

    public static Vector3 ScaleVector(Vector3 original, float x, float y, float z)
    {
        return new Vector3(original.x * x, original.y * y, original.z * z);
    }

    public static bool IsVec2Hit(Vector2Int vector, int min_x, int min_y, int max_x, int max_y)
    {
        int x = vector.x;
        int y = vector.y;
        return x >= min_x && x <= max_x && y >= min_y && y <= max_y;
    }

    public static string ToTimerFormat(float second)
    {
        string f = "";
        float t = Mathf.Ceil(second);
        float min = Mathf.FloorToInt(t / 60);
        float sec = Mathf.FloorToInt(t - 60f * min);
        f += min.ToString();
        if (f.Length < 2)
        {
            f = "0" + f;
        }
        f += ":";
        if (sec.ToString().Length < 2)
        {
            f += "0";
        }
        f += sec.ToString();
        return f;
    }

    /// <summary>
    /// Find first empty slot follow gravity direction. Return null if not found. 
    /// Use for block that don't cast shadow
    /// </summary>
    public static Slot FindSlotEmptyFollowGravity(Slot beginSlot)
    {
        Slot slot = beginSlot[beginSlot.slotGravity.gravityDirection];

        if (!slot)
        {
            return null;
        }

        if (slot.block)
        {
            if (slot.block.IsCastShadow())
            {
                return null;
            }
            else
            {
                return FindSlotEmptyFollowGravity(slot);
            }
        }

        if (!slot.chip)
        {
            return slot;
        }
        else
        {
            return null;
        }

    }

    /// <summary>
    /// Get random index from ratio array config
    /// </summary>
    public static int GetRandomIndex(int[] ratioArray)
    {
        int totalProb = 0;
        for (int i = 0; i < ratioArray.Length; i++)
        {
            totalProb += ratioArray[i];
        }

        int rand = UnityEngine.Random.Range(0, totalProb);
        int currentRate = 0;
        for (int i = 0; i < ratioArray.Length; i++)
        {
            if (ratioArray[i] > 0)
            {
                currentRate += ratioArray[i];

                if (rand <= currentRate)
                {
                    return i;
                }
            }
        }
        Debug.LogWarning("!!! Array random work wrong");
        return 0; //default return but should not come here
    }
}
////////////////////////////////////////////////////////////////////////////////

public enum Side
{
    Null,
    Top, Bottom, Right, Left,
    TopRight, TopLeft,
    BottomRight, BottomLeft
}

public enum FieldTarget
{
    None = 0,
    Stone = 1, //isClearUnderLayerGame
    FixBlock = 2, //isClearFixedMultiBlocker
    Color = 3, //isClearNormalBlockGame
    KeyDrop = 4, //isCollectSinkerGame
    Smoke = 5, //isClearIncreaserGame
    Blocker = 6, //isClearBlockerGame (compass ...)
    Cage = 7, //isClearCageGame, auto value
    ColorBlocker = 8, //isClearColorMatchBlocker
    MagicScroll = 9, //isClearMagicScrollGame
    Butterfly = 10, //isClearClimberGame
    Curtain = 11, //isClearSpiderWebGame
    MetalBrick = 12, //isClearMetalBrickGame
    IceBrick = 13, //isClearIceBrickGame
    RandomChanger = 14, //isClearRandomChangerGame (Magic Portion)
}

public enum BlockerTargetType
{
    Compass = 0,
    Unknown = 1,
    SmokePot = 2,
    Crystal = 3,
}

public enum Limitation
{
    Moves,
    Time
}

[System.Serializable]
public class Pair
{
    public string a;
    public string b;

    public Pair(string pa, string pb)
    {
        a = pa;
        b = pb;
    }

    public static bool operator ==(Pair a, Pair b)
    {
        return Equals(a, b);
    }
    public static bool operator !=(Pair a, Pair b)
    {
        return !Equals(a, b);
    }


    public override bool Equals(object obj)
    {
        Pair sec = (Pair)obj;
        return (a.Equals(sec.a) && b.Equals(sec.b)) ||
            (a.Equals(sec.b) && b.Equals(sec.a));
    }

    public override int GetHashCode()
    {
        return a.GetHashCode() + b.GetHashCode();
    }
}

class EasingFunctions
{
    // no easing, no acceleration
    public static float linear(float t)
    {
        return t;
    }
    // accelerating from zero velocity
    public static float easeInQuad(float t)
    {
        return t * t;
    }
    // decelerating to zero velocity
    public static float easeOutQuad(float t)
    {
        return t * (2 - t);
    }
    // acceleration until halfway, then deceleration
    public static float easeInOutQuad(float t)
    {
        return t < .5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
    }
    // accelerating from zero velocity 
    public static float easeInCubic(float t)
    {
        return t * t * t;
    }
    // decelerating to zero velocity 
    public static float easeOutCubic(float t)
    {
        return (--t) * t * t + 1;
    }
    // acceleration until halfway, then deceleration 
    public static float easeInOutCubic(float t)
    {
        return t < .5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
    }
    // accelerating from zero velocity 
    public static float easeInQuart(float t)
    {
        return t * t * t * t;
    }
    // decelerating to zero velocity 
    public static float easeOutQuart(float t)
    {
        return 1 - (--t) * t * t * t;
    }
    // acceleration until halfway, then deceleration
    public static float easeInOutQuart(float t)
    {
        return t < .5f ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;
    }
    // accelerating from zero velocity
    public static float easeInQuint(float t)
    {
        return t * t * t * t * t;
    }
    // decelerating to zero velocity
    public static float easeOutQuint(float t)
    {
        return 1 + (--t) * t * t * t * t;
    }
    // acceleration until halfway, then deceleration 
    public static float easeInOutQuint(float t)
    {
        return t < .5f ? 16 * t * t * t * t * t : 1 + 16 * (--t) * t * t * t * t;
    }

    public static float easeInElastic(float t)
    {
        if (t == 0 || t == 1) return t;
        float p = 0.5f;
        return -(Mathf.Pow(2, -10 * t) * Mathf.Sin(-(t + p / 4) * (2 * Mathf.PI) / p));
    }

    public static float easeOutElastic(float t)
    {
        if (t == 0 || t == 1) return t;
        float p = 0.5f;
        return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - p / 4) * (2 * Mathf.PI) / p) + 1;
    }

    public static float easeInOutElastic(float t)
    {
        if (t <= 0 || t >= 1) return Mathf.Clamp01(t);
        t = Mathf.Lerp(-1, 1, t);

        float p = 0.9f;

        if (t < 0)
            return 0.5f * (Mathf.Pow(2, 10 * t) * Mathf.Sin((t + p / 4) * (2 * Mathf.PI) / p));
        else
            return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - p / 4) * (2 * Mathf.PI) / p) * 0.5f + 1;
    }
}