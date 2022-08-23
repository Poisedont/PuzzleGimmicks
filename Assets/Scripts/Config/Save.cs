using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Save
{
    public bool SFXEnable;
    public bool BGMEnable;
    public int playerCoin;
    public int PlayerLive;
    public long NextLifeTime;
    public int currentLevel;
    public int[] levelResultScoreArray = new int[ProfileConfig.k_maxLevel];
    public int[] levelResultStarsArray = new int[ProfileConfig.k_maxLevel];
    public int BoosterHammerCount;
    public int BoosterCrossCount;
    public int BoosterRainbowCount;
}
