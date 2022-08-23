
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelProfile
{
    public static LevelProfile main; // current level
    public const int maxSize = 9; // maximal playing field size

    public int levelID = 0; // Level ID
    public int level = 0; // Level number
    // field size
    public int width = 9;
    public int height = 9;
    public int colorCount = 6; // count of chip colors
    public int[] easyColorRatio, normalColorRatio;
    public int firstStarScore = 100; // number of score points needed to get a first stars
    public int secondStarScore = 200; // number of score points needed to get a second stars
    public int thirdStarScore = 300; // number of score points needed to get a third stars

    public Limitation limitation = Limitation.Moves;
    // Session duration in time limitation mode = duration value (sec);
    // Count of moves in moves limimtation mode = moveCount value (sec);
    public int limit = 30; // Count of moves

    public List<SlotSettings> slots = new List<SlotSettings>();
    public List<LevelTarget> allTargets = new List<LevelTarget>();
    public int maxClimber;
    public int climberGenerateInterval;

    public RandomChangerConfig randomChangerConfig = new RandomChangerConfig();

    ////////////////////////////////////////////////////////////////////////////////
    public LevelProfile()
    {
        easyColorRatio = new int[6];
        normalColorRatio = new int[6];
    }
    public void SetTargetCount(int index, int target, FieldTarget targetType)
    {
        LevelTarget lvTarget = allTargets.Find(a => a.Type == targetType);
        if (lvTarget != null)
        {
            lvTarget.SetTargetCount(index, target);
        }
        else
        {
            Debug.LogWarning("Create target first: " + targetType);
        }
    }
    public int GetTargetCount(int index, FieldTarget targetType)
    {
        LevelTarget lvTarget = allTargets.Find(a => a.Type == targetType);
        if (lvTarget != null)
        {
            return lvTarget.GetTargetCount(index);
        }
        return 0;
    }

    public void AddTarget(FieldTarget targetType)
    {
        if (allTargets.Exists(a => a.Type == targetType))
        {
            Debug.LogWarning(".. Profile already has target: " + targetType);
        }
        else
        {
            LevelTarget target = new LevelTarget(targetType);
            allTargets.Add(target);
        }
    }

    public bool HasTarget(FieldTarget targetType)
    {
        return allTargets.Exists(a => a.Type == targetType);
    }

    // index > 0: for Target.Color
    public void IncreaseTargetProgress(FieldTarget targetType, int index = 0, int inc = 1)
    {
        LevelTarget target = allTargets.Find(a => a.Type == targetType);
        if (target != null)
        {
            target.IncreaseCurrentCount(index, inc);
        }
    }

    public LevelProfile GetClone()
    {
        LevelProfile clone = new LevelProfile();
        clone = (LevelProfile)MemberwiseClone();
        clone.levelID = -1;
        return clone;
    }

    //load level from text
    public static LevelProfile LoadLevel(int level)
    {
        //load level config from json
        string file = "Levels/" + level;

        TextAsset text = Resources.Load(file) as TextAsset;
        if (text)
        {
            LevelConfig config = JsonUtility.FromJson<LevelConfig>(text.text);

            LevelProfile profile = LevelConfig.ParseToProfile(config);

            profile.level = level;
            return profile;
        }

        Debug.LogError("!!! not found asset for level " + level);
        return null;
    }

    public int GetColorRandom()
    {
        int totalProb = 0;
        for (int i = 0; i < easyColorRatio.Length; i++)
        {
            totalProb += easyColorRatio[i];
        }

        int rand = UnityEngine.Random.Range(0, totalProb);
        int currentRate = 0;
        for (int i = 0; i < easyColorRatio.Length; i++)
        {
            if (easyColorRatio[i] > 0)
            {
                currentRate += easyColorRatio[i];

                if (rand <= currentRate)
                {
                    return i;
                }
            }
        }
        Debug.LogWarning("!!! Pieces random work wrong");
        return 0; //default return but should not come here
    }
}