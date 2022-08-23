[System.Serializable]
public class RandomChangerConfig
{
    public class ChangerInfo
    {
        public string Item { get; set; }
        public int Level { get; set; }

        public ChangerInfo(string item, int level)
        {
            Item = item;
            Level = level;
        }
    }
    public bool Enable;
    public int GenerateInterval;
    public int MinNumber;
    public int MaxNumber;
    public int GenerateCount;
    public int[] ConvertProbability;

    public RandomChangerConfig()
    {
        ConvertProbability = new int[0];
    }

    // 13 items
    public static ChangerInfo[] s_randomItems = new ChangerInfo[] {
        new ChangerInfo (EPieces.Compass.ToString(), 1),
        new ChangerInfo (EPieces.Compass.ToString(), 2),
        new ChangerInfo (LevelConfig.GetBlockTypeFrom( ETile.Cage), 1),
        new ChangerInfo (LevelConfig.GetBlockTypeFrom(ETile.MagicBook), 1),
        new ChangerInfo (LevelConfig.GetBlockTypeFrom(ETile.MagicBook), 2),
        new ChangerInfo (LevelConfig.GetBlockTypeFrom(ETile.MagicBook), 3),
        new ChangerInfo (LevelConfig.GetBlockTypeFrom(ETile.MagicBook), 4),
        new ChangerInfo (LevelConfig.GetBlockTypeFrom(ETile.MagicBook), 5),
        new ChangerInfo (LevelConfig.GetBlockTypeFrom(ETile.ColorBook), 1),
        new ChangerInfo (EPieces.Butterfly.ToString(), 1),
        new ChangerInfo (LevelConfig.GetBlockTypeFrom(ETile.Curtain), 1),
        new ChangerInfo (LevelConfig.GetBlockTypeFrom(ETile.Curtain), 2),
        new ChangerInfo (LevelConfig.GetBlockTypeFrom(ETile.Curtain), 3),

    };

    public ChangerInfo GetRandomObject()
    {
        int random = Utils.GetRandomIndex(ConvertProbability);
        return s_randomItems[random];
    }
}