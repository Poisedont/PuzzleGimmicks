using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundDefine
{
    /// <summary>
    /// Music file name
    /// </summary>
    public const string k_Intro_Music = "intro_bgm";
    public const string k_Play_Music = "play_bgm";

    ////SFX fild name
    /// 
    public const string k_ButtonClickSFXName = "touch";
    public const string k_OutOfMoveSFX = "outofmove";
    public const string k_GoalGet = "goal_get";
    public const string k_shuffle = "shuffle";
    public const string k_blockchange = "blockchange";
    public const string k_cage = "cage";
    public const string k_line = "line";
    public const string k_key_drop = "key_drop";
    public const string k_rainbow = "rainbow";
    public const string stone_crash = "stone_crash";
    public const string k_touch = "touch";
    public const string k_3m = "3m";
}

public class ProfileConfig
{
    public const int k_lifePlayerLimit = 5;
    public const int k_nextTimeLifeUp = 30; /// 30 min
    public const int k_maxLevel = 1500;
}
