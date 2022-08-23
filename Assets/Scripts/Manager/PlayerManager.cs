using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public enum ARTEFACT
{
    NONE,
    CLOCK,
    RING,
    CLOAK,
    LAMP,
    SWORD,
    CELL,
    BOMS,
    LUCKY,
    WAND,
}

public class PlayerManager : Singleton<PlayerManager>
{
    public bool m_SFXEnable { get; set; }
    public bool m_BGMEnable { get; set; }
    public int m_PlayerCoin { get; set; }
    public int m_PlayerLive { get; set; }
    public DateTime m_nextLifeTime { get; set; }

    public int m_currentLevel { get; set; }
    public int m_levelSelected { get; set; }
    public int[] m_levelResultScoreArray { get; set; }
    public int[] m_levelResultStarsArray { get; set; }

    public int m_boosterHammerCount { get; set; }
    public int m_boosterCrossCount { get; set; }
    public int m_boosterRainbowCount { get; set; }

    public bool m_unlockAllLevel = false;
    public bool m_infinityTools = false;
    public bool m_canSkipCollapseAllPowerups = false;
    public bool m_skipCollapseAllPowerups = false;
    // Start is called before the first frame update
    void Start()
    {
        m_currentLevel = 0;
        m_levelSelected = 0;
        m_levelResultScoreArray = new int[ProfileConfig.k_maxLevel];
        m_levelResultStarsArray = new int[ProfileConfig.k_maxLevel];
        m_SFXEnable = true;
        m_BGMEnable = true;
        m_PlayerCoin = 0;
        m_PlayerLive = ProfileConfig.k_lifePlayerLimit;
        m_nextLifeTime = DateTime.Now;
        m_boosterHammerCount = 1;
        m_boosterCrossCount = 1;
        m_boosterRainbowCount = 1;
        LoadGame();
        StartCoroutine(LifeSystemRoutine());

    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void ResetAll()
    {
        m_PlayerCoin = 0;
        m_SFXEnable = true;
        m_BGMEnable = true;
        m_currentLevel = 0;
        m_levelSelected = 0;
        m_PlayerLive = ProfileConfig.k_lifePlayerLimit;
        m_nextLifeTime = DateTime.Now;
        for (int i = 0; i< ProfileConfig.k_maxLevel; i++)
        {
            m_levelResultScoreArray[i] = 0;
            m_levelResultStarsArray[i] = 0;
        }
        m_boosterHammerCount = 1;
        m_boosterCrossCount = 1;
        m_boosterRainbowCount = 1;
    }


    private Save CreateSaveGameObject()
    {
        Save save = new Save();

        save.SFXEnable = m_SFXEnable;
        save.BGMEnable = m_BGMEnable;
        save.playerCoin = m_PlayerCoin;
        save.currentLevel = m_currentLevel;
        save.PlayerLive = m_PlayerLive;
        save.NextLifeTime = m_nextLifeTime.Ticks;
        for (int i = 0; i< ProfileConfig.k_maxLevel; i++)
        {
            save.levelResultScoreArray[i] = m_levelResultScoreArray[i];
            save.levelResultStarsArray[i] = m_levelResultStarsArray[i];
        }
        save.BoosterHammerCount = m_boosterHammerCount;
        save.BoosterCrossCount = m_boosterCrossCount;
        save.BoosterRainbowCount = m_boosterRainbowCount;

        return save;
    }

    public void SaveGame()
    {
        // 1
        Save save = CreateSaveGameObject();

        // 2
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/gs.save");
        bf.Serialize(file, save);
        file.Close();

        Debug.Log("Game Saved");
    }

    public void LoadGame()
    {
        // 1
        if (File.Exists(Application.persistentDataPath + "/gs.save"))
        {
            print(Application.persistentDataPath);
            // 2
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gs.save", FileMode.Open);
            Save save = (Save)bf.Deserialize(file);
            file.Close();

            // 3
            m_SFXEnable = save.SFXEnable;
            m_BGMEnable = save.BGMEnable;
            m_PlayerCoin = save.playerCoin;
            m_currentLevel = save.currentLevel;
            m_PlayerLive = save.PlayerLive;
            m_nextLifeTime = new DateTime(save.NextLifeTime);
            for (int i = 0; i < ProfileConfig.k_maxLevel; i++)
            {
                m_levelResultScoreArray[i] = save.levelResultScoreArray[i];
                m_levelResultStarsArray[i] = save.levelResultStarsArray[i];
            }
            m_boosterHammerCount = save.BoosterHammerCount;
            m_boosterCrossCount = save.BoosterCrossCount;
            m_boosterRainbowCount = save.BoosterRainbowCount;

            Debug.Log("Game Loaded");

        }
        else
        {
            Debug.Log("No game saved!");
        }
    }

    public void UpdateLevelInfo(int level, int score)
    {
        m_levelResultScoreArray[level] = score;

        SaveGame();
    }

    IEnumerator LifeSystemRoutine()
    {
        //while (local_profile == null)
        //    yield return 0;

        TimeSpan refilling_time = new TimeSpan(0, ProfileConfig.k_nextTimeLifeUp, 0); ///30 min

        while (true)
        {
            while (m_PlayerLive < ProfileConfig.k_lifePlayerLimit && m_nextLifeTime <= DateTime.Now)
            {
                m_PlayerLive++;
                m_nextLifeTime += refilling_time;
                PlayerManager.Instance.SaveGame();
                // ItemCounter.RefreshAll();
            }
            if (m_PlayerLive >= ProfileConfig.k_lifePlayerLimit)
            {
                m_PlayerLive = ProfileConfig.k_lifePlayerLimit;
                m_nextLifeTime = DateTime.Now + refilling_time;
            }
            yield return new WaitForSeconds(1);
        }

        
    }
}
