using System;

public class LevelTarget
{
    FieldTarget m_target;
    int[] m_targetCounts;
    int[] m_currentProgress;

    public FieldTarget Type { get { return m_target; } }
    ////////////////////////////////////////////////////////////////////////////////

    public LevelTarget(FieldTarget target)
    {
        Init(target);
    }

    void Init(FieldTarget target)
    {
        m_target = target;
        if (target == FieldTarget.Color)
        {
            m_targetCounts = new int[6]; // 6 color
            m_currentProgress = new int[6];
        }
        else if (target == FieldTarget.Blocker
            )
        {
            int count = Enum.GetValues(typeof(BlockerTargetType)).Length;
            m_targetCounts = new int[count]; //count
            m_currentProgress = new int[count];
        }
        else
        {
            m_targetCounts = new int[1]; //count default
            m_currentProgress = new int[1];
        }
    }
    ////////////////////////////////////////////////////////////////////////////////
    public override string ToString()
    {
        string detail = " with:";
        for (int i = 0; i < m_targetCounts.Length; i++)
        {
            detail += " " + m_currentProgress[i] + "/" + m_targetCounts[i] + ",";
        }
        return m_target + detail;
    }

    public void SetTargetCount(int index, int count)
    {
        if (index >= 0 && index < m_targetCounts.Length)
        {
            m_targetCounts[index] = count;
        }
    }

    public int GetTargetCount(int index)
    {
        if (index >= 0 && index < m_targetCounts.Length)
        {
            return m_targetCounts[index];
        }
        return -1; //error
    }

    public void SetCurrentCount(int index, int count)
    {
        if (index >= 0 && index < m_currentProgress.Length)
        {
            m_currentProgress[index] = count;
        }
    }

    public void IncreaseCurrentCount(int index, int inc = 1)
    {
        SetCurrentCount(index, GetCurrentCount(index) + inc);
    }

    public int GetCurrentCount(int index)
    {
        if (index >= 0 && index < m_currentProgress.Length)
        {
            return m_currentProgress[index];
        }
        return -1; //error
    }

    public bool IsReachTarget()
    {
        for (int i = 0; i < m_targetCounts.Length; i++)
        {
            if (m_currentProgress[i] < m_targetCounts[i])
            {
                return false;
            }
        }
        return true;
    }

    // only use in Editor
    public void ChangeType(FieldTarget newType)
    {
        if (newType != m_target)
        {
            Init(newType);
        }
    }

    public int GetNumberTargets()
    {
        return m_targetCounts.Length;
    }

    public FieldTarget GetTarget()
    {
        return m_target;
    }
}