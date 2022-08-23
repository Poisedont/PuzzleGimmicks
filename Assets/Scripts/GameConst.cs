public static class GameConst
{
    public static int k_session_scoreChip = 100; // Score multiplier
    public static float k_base_scoreChip = 0.3f;

    // NOTE: if game change gravity, we need to save the gravity array and change the format of generator in LevelConfig (defaultSpawnColumn[])
    public static bool k_game_use_gravity = false;

    public static string k_sinkerGoal_tag = "KeyOut";
    public static string k_blockGenerator_tag = "genBlocker";
    public static string k_climberGenerator_tag = "genClimber"; //butterfly

    public static int k_blocker_group_slot_count = 4;
    public static int k_magic_tap_max = 8;
    public const int k_candy_tree_max = 4;

}

// For Pieces array
public enum EPieces
{
    SimpleChip = 0,
    HLineBomb = 1,
    VLineBomb = 2,
    SimpleBomb = 3,
    RainbowBomb = 4,
    CrossBomb = 5,
    KeyDrop = 8,
    TimeBomb = 14,
    Butterfly = 19,
    Portion = 20, //magic
    Compass = 26,
    Compass2 = 27,
    SmokePot = 28,
    Crystal = 29,
    /* Basic jewel half ice: 41
        Basic jewel full ice: 42
        Line Bomb half ice: 43
        Line Bomb full ice: 44 
    */
    HalfIceChip = 41,
    FullIceChip = 42,
    HalfIceLineBomb = 43,
    FullIceLineBomb = 44,
}

//For panel array
public enum ETile
{
    Visible = 0,
    Invisible = 1,
    ColumnGeneratorMark = 3, //this will make the below tile to be a generator 
    MagicBook = 6, //block book
    Smoke = 7,
    Cage = 9,
    MagicTap = 22,
    //23, 24, 25: Magic tap child
    ColorBook = 26,
    ColorChanger = 27,
    CandyTree = 30,
    Switcher = 31,
    BookShelf = 33, // lv 296
    Jewelcase = 35,
    JewelMold = 55,

    Scroll = 65,
    Curtain = 66,
    Lamp = 67,
    Metal = 68,
}