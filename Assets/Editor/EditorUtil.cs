using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EditorUtil
{
    public static Texture LoadAssetTexture(string path)
    {
        return (Texture)AssetDatabase.LoadAssetAtPath(path, typeof(Texture));
    }

    public static Texture LoadResourceTexture(string path)
    {
        return EditorGUIUtility.Load(path) as Texture;
    }
}

public abstract class MetaEditor : Editor
{
    public Object metaTarget
    {
        get
        {
            try
            {
                return target;
            }
            catch (System.Exception)
            {
                return FindTarget();
            }
        }
    }

    public abstract Object FindTarget();

    public System.Action onRepaint = delegate { };

    public void RepaintIt()
    {
        Repaint();
        onRepaint.Invoke();
    }
}

////////////////////////////////////////////////////////////////////////////////
public class SlotUndo
{
    Stack<SlotSettings> m_stack;

    static SlotUndo instance;
    private SlotUndo()
    {
        instance = this;
        m_stack = new Stack<SlotSettings>(50);
    }

    public static void Init()
    {
        if (instance == null)
        {
            new SlotUndo();
        }
        instance.m_stack.Clear();
    }

    public static SlotUndo GetInstance()
    {
        if (instance == null)
        {
            Init();
        }
        return instance;
    }

    public static void Record(SlotSettings slot)
    {
        if (slot != null)
        {
            GetInstance().m_stack.Push(slot.GetDeepCopy());
        }
    }

    public static SlotSettings GetTop()
    {
        if (GetInstance().m_stack.Count > 0)
        {
            SlotSettings slot = instance.m_stack.Pop();
            return slot;
        }
        return null;
    }
}