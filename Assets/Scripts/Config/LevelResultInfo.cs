using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelResultInfo
{
    private string levelID;
    private int score;
    private int star;

    public LevelResultInfo()
    {
        levelID = "level0";
        score = 0;
        star = 0;
    }

    public void Reset()
    {
        levelID = "level0";
        score = 0;
        star = 0;
    }

    public void SetLevelID(string id)
    {
        levelID = id;
    }

    public void SetScore(int pScore)
    {
        score = pScore;
    }

    public void SetStar(int pStar)
    {
        star = pStar;
    }

    public string GetLevelID()
    {
        return levelID;
    }

    public int GetScore()
    {
        return score;
    }

    public int GetStar()
    {
        return star;
    }
}
