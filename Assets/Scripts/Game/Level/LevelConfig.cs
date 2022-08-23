using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelConfig
{
    public string key;
    public string author;
    public int[] panels;
    public int[] strengths;
    public int[] backlayers;
    public int[] pieces;
    public int[] colors;
    // public int[] gravity;
    public string[] panelInfos;
    public bool[] startPieces;
    public bool isMaxMovesGame;
    public bool isClearUnderLayerGame; //stone
    public bool isClearIncreaserGame; // spread object
    public bool isClearNormalBlockGame;
    public bool isCollectSinkerGame; //drop item
    // public bool isCollectBoosterGame;
    public bool isClearClimberGame; //butterfly target
    public int allowedMoves;
    public int[] scoreToReach;
    public int[] numToGet;
    public bool sinkerEnable;
    public int sinkerMaxOnScreen;
    public int[] sinkerGoal; //type of sinkerGoal is position of key goal slot
    public int[] sinkers; //target Key number to collect, array only 1 element
    public int[] easyProbability;
    public int[] normalProbability;
    public int[] hardProbability;
    public int[] portalTypes;
    public int[] portalIndices;
    public int[] defaultSpawnColumn;
    public int[] blockerSpawnColumn; // use to spawn blocker (compass), vary length

    public int climberMaxOnScreen;
    public int climberTargetCount;
    public int[] climberSpawn;
    public int climberGenerateInterval;
    public string[] magicScrollInfos;
    public bool isClearFixedMultiBlocker;
    public int fixedMultiBlockerTargetCount; // book
    public bool isClearBlockerGame; // sthing like Compass
    public int[] blockers;

    public bool randomchangerEnable;
    public int randomchangerGenerateInterval;
    public int randomchangerMinNumber;
    public int randomchangerMaxNumber;
    public int[] randomchangerConvertProbability;
    public int randomChangerGenerateCount;
    public bool isClearRandomChangerGame;

    public bool isClearCageGame;
    public bool isClearColorMatchBlocker;
    public bool isClearMagicScrollGame;
    public bool isClearIceBrickGame;
    public bool isClearMetalBrickGame;
    public bool isClearSpiderWebGame; // curtain

    public string[] conveyorInfos;


    ////////////////////////////////////////////////////////////////////////////////
    public static LevelProfile ParseToProfile(LevelConfig config)
    {
        LevelProfile profile = new LevelProfile();

        // mapping from config to this profile
        if (config != null)
        {
            Debug.Log("Loaded level: " + config.key);
            profile.width = 9;
            profile.height = 9;

            int width = 1;
            int height = 1;

            int stoneCount = 0;
            int cageCount = 0;
            int curtainCount = 0;
            int metalCount = 0;
            int iceCount = 0, rndChangeCount = 0;

            // collect portal info: key: index, Vector2Int : x -> entry, y -> exit
            Dictionary<int, Vector2Int> portalDict = new Dictionary<int, Vector2Int>();

            // highest slot each column to find generator
            int[] highestSlotY = new int[LevelProfile.maxSize];

            int fieldsize = profile.width * profile.height; //
            //create all slot first
            for (int i = 0; i < fieldsize; ++i)
            {
                int y = i / profile.width; //row
                y = LevelProfile.maxSize - 1 - y;
                int x = i % profile.width; //column
                Vector2Int fieldPos = new Vector2Int(x, y);
                SlotSettings slotSetting = new SlotSettings(fieldPos);
                profile.slots.Add(slotSetting);
            }
            for (int i = 0; i < fieldsize; ++i)
            {
                int y = i / profile.width; //row
                y = LevelProfile.maxSize - 1 - y;
                int x = i % profile.width; //column
                Vector2Int fieldPos = new Vector2Int(x, y);
                SlotSettings slotSetting = profile.slots.Find(s => s.position == fieldPos); //Can't be NULL

                int panelVal = config.panels[i];

                slotSetting.visible = IsPanelSlotVisible(panelVal);
                slotSetting.chip = GetChipTypeFrom(config.pieces[i]); //default pieces

                if (config.pieces[i] >= (int)EPieces.HalfIceChip && config.pieces[i] <= (int)EPieces.FullIceLineBomb)
                {
                    iceCount += 1;
                }
                else if (config.pieces[i] == (int)EPieces.Portion)
                {
                    rndChangeCount += 1;
                }

                if (!config.startPieces[i])
                {
                    slotSetting.chip = ""; //empty slot
                }

                ETile blockTile = (ETile)panelVal;
                slotSetting.block_type = GetBlockTypeFrom(blockTile);
                slotSetting.block_level = config.strengths[i];
                if (slotSetting.block_type != "")
                {
                    if (blockTile == ETile.Cage) cageCount += 1;
                    if (blockTile == ETile.Curtain) curtainCount += 1;
                    if (blockTile == ETile.Metal) metalCount += 1;

                    if (!BlockCanContainChip(blockTile))
                    {
                        slotSetting.chip = ""; //remove chip type if block can't keep
                    }

                    if (blockTile == ETile.BookShelf || blockTile == ETile.CandyTree)
                    {
                        string json = config.panelInfos[i];
                        if (json.Length > 0)
                        {
                            slotSetting.bookShelfInfo = JsonUtility.FromJson<BookShelfInfo>(json);
                        }
                    }
                    if (slotSetting.block_type == ETile.MagicTap.ToString())
                    {
                        slotSetting.bookShelfInfo = new BookShelfInfo() { Index = blockTile - ETile.MagicTap };
                    }

                    if (blockTile == ETile.Switcher)
                    {
                        string json = config.panelInfos[i];
                        if (json.Length > 0)
                        {
                            slotSetting.switcherInfo = JsonUtility.FromJson<SwitcherInfo>(json);
                        }
                    }
                }
                // check find middle generator
                if (blockTile == ETile.ColumnGeneratorMark)
                {
                    int belowY = y - 1;  // vert direct of json file is inverse of editor
                    do
                    {
                        SlotSettings slotBelow = profile.slots.Find(s => s.position.x == x && s.position.y == belowY);
                        int slotIdx = i + LevelProfile.maxSize * (y - belowY);
                        bool slotVisible = IsPanelSlotVisible(config.panels[slotIdx]);
                        if (slotBelow != null && slotVisible)
                        {
                            slotBelow.generator = true;
                            belowY = 0; // found generator so don't need to continue loop
                        }
                        else
                        {
                            belowY -= 1; // vert direct of json file is inverse of editor
                        }
                    } while (belowY > 0);
                }
                // colors
                if (config.colors != null)
                {
                    slotSetting.color_id = config.colors[i];
                }

                /* if (config.gravity != null && GameConst.k_game_use_gravity)
                {
                    if (config.gravity.Length != 0)
                    {
                        if (slotSetting.visible)
                        {
                            slotSetting.gravity = (Side)config.gravity[i];
                        }
                    }
                    else
                    {
                        slotSetting.gravity = Side.Bottom; //NOTE: default direction is down
                    }
                } */

                if (config.backlayers != null)
                {
                    slotSetting.stone_level = config.backlayers[i];
                    if (config.isClearUnderLayerGame)
                    {
                        if (config.backlayers[i] >= 0)
                        {
                            stoneCount += 1;
                        }
                    }
                }

                if (slotSetting.visible)
                {
                    width = Mathf.Max(x, width); //

                    height = Mathf.Max(y, height);

                    int colIndex = Array.IndexOf(config.defaultSpawnColumn, x);
                    if (colIndex >= 0)
                    {
                        highestSlotY[colIndex] = Mathf.Max(y, highestSlotY[colIndex]);
                    }
                }

                // map portal
                if (config.portalTypes[i] == 1)
                {
                    int indice = config.portalIndices[i];
                    if (portalDict.ContainsKey(indice))
                    {
                        Vector2Int port = portalDict[indice];
                        port.x = i; //save port entry
                        portalDict[indice] = port;
                    }
                    else
                    {
                        portalDict.Add(indice, new Vector2Int(i, 0));
                    }
                    // Debug.Log("find port: " + indice + " entry at: " + i);
                }
                else if (config.portalTypes[i] == 2) //exit
                {
                    int indice = config.portalIndices[i];
                    if (portalDict.ContainsKey(indice))
                    {
                        Vector2Int port = portalDict[indice];
                        port.y = i; // save port exit
                        portalDict[indice] = port;
                    }
                    else
                    {
                        portalDict.Add(indice, new Vector2Int(0, i));
                    }
                    // Debug.Log("find port: " + indice + " exit at: " + i);
                }

                // magic scroll
                if (config.magicScrollInfos.Length > 0)
                {
                    string tmp = config.magicScrollInfos[i];
                    if (!string.IsNullOrEmpty(tmp))
                    {
                        MagicScrollInfo info = JsonUtility.FromJson<MagicScrollInfo>(tmp);

                        slotSetting.scrollInfo = info;
                    }
                }

                // conveyorInfos
                if (config.conveyorInfos.Length > 0)
                {
                    string tmp = config.conveyorInfos[i];
                    if (!string.IsNullOrEmpty(tmp))
                    {
                        ConveyorInfo info = JsonUtility.FromJson<ConveyorInfo>(tmp);
                        slotSetting.conveyorInfo = info;
                    }
                }
            }

            //remap slot to teleport
            foreach (var port in portalDict)
            {
                int entry = port.Value.x;
                int entryRow = entry / LevelProfile.maxSize;
                entryRow = LevelProfile.maxSize - 1 - entryRow;
                int entryCol = entry % LevelProfile.maxSize;

                int exit = port.Value.y;
                int exitRow = exit / LevelProfile.maxSize;
                exitRow = LevelProfile.maxSize - 1 - exitRow;
                int exitCol = exit % LevelProfile.maxSize;

                SlotSettings slot = profile.slots.Find(a => a.position.x == entryCol && a.position.y == entryRow);
                if (slot != null)
                {
                    // Debug.Log(">> found portal: " + port.Key + ", entry: row: " + entryRow + ", col: " + entryCol
                    //     + ", Exit: row: " + exitRow + ", col: " + exitCol);
                    slot.teleport = new Vector2Int(exitCol, exitRow);
                }
            }

            //assign generator to highest slot
            for (int i = 0; i < config.defaultSpawnColumn.Length; i++)
            {
                SlotSettings slot = profile.slots.Find(a => a.position.x == config.defaultSpawnColumn[i]
                    && a.position.y == highestSlotY[i]);
                if (slot != null)
                {
                    slot.generator = true;
                }
            }

            if (config.blockerSpawnColumn.Length > 0)
            {
                for (int i = 0; i < config.blockerSpawnColumn.Length; i++)
                {
                    SlotSettings slot = profile.slots.Find(a => a.generator && a.position.x == config.blockerSpawnColumn[i]);
                    if (slot != null)
                    {
                        slot.tags.Add(GameConst.k_blockGenerator_tag);
                    }
                }
            }


            profile.width = width + 1;
            profile.height = height + 1;

            profile.colorCount = 6; //get color count from config
            profile.easyColorRatio = config.easyProbability;
            profile.normalColorRatio = config.normalProbability;

            if (config.scoreToReach != null)
            {
                profile.firstStarScore = config.scoreToReach[0];
                profile.secondStarScore = config.scoreToReach[1];
                profile.thirdStarScore = config.scoreToReach[2];
            }

            if (config.isClearNormalBlockGame)
            {
                profile.AddTarget(FieldTarget.Color);
            }
            if (config.numToGet != null)
            {
                for (int i = 0; i < config.numToGet.Length; i++)
                {
                    profile.SetTargetCount(i, config.numToGet[i], FieldTarget.Color);
                }
            }

            if (config.isCollectSinkerGame)
            {
                profile.AddTarget(FieldTarget.KeyDrop);
                profile.SetTargetCount(0, config.sinkers[0], FieldTarget.KeyDrop);

                for (int i = 0; i < config.sinkerGoal.Length / 2; i++)
                {
                    int col = config.sinkerGoal[i * 2];
                    int row = config.sinkerGoal[i * 2 + 1];

                    SlotSettings slot = profile.slots.Find(a => a.position.x == col && a.position.y == row);
                    if (slot != null)
                    {
                        slot.tags.Add(GameConst.k_sinkerGoal_tag);
                    }
                }
            }

            profile.maxClimber = config.climberMaxOnScreen;
            profile.climberGenerateInterval = config.climberGenerateInterval;

            for (int i = 0; i < config.climberSpawn.Length / 2; i++)
            {
                int col = config.climberSpawn[i * 2];
                int row = config.climberSpawn[i * 2 + 1];

                SlotSettings slot = profile.slots.Find(a => a.position.x == col && a.position.y == row);
                if (slot != null)
                {
                    slot.tags.Add(GameConst.k_climberGenerator_tag);
                }
            }

            if (config.isClearClimberGame)
            {
                profile.AddTarget(FieldTarget.Butterfly);
                profile.SetTargetCount(0, config.climberTargetCount, FieldTarget.Butterfly);
            }
            if (config.isClearUnderLayerGame)
            {
                profile.AddTarget(FieldTarget.Stone);
                profile.SetTargetCount(0, stoneCount, FieldTarget.Stone);
            }
            if (config.isClearFixedMultiBlocker)
            {
                profile.AddTarget(FieldTarget.FixBlock);
                profile.SetTargetCount(0, config.fixedMultiBlockerTargetCount, FieldTarget.FixBlock);
            }
            if (config.isClearBlockerGame)
            {
                profile.AddTarget(FieldTarget.Blocker);
                int count = config.blockers.Length;
                for (int i = 0; i < count; i++)
                {
                    profile.SetTargetCount(i, config.blockers[i], FieldTarget.Blocker);
                }
            }
            if (config.isClearCageGame)
            {
                profile.AddTarget(FieldTarget.Cage);
                profile.SetTargetCount(0, cageCount, FieldTarget.Cage);
            }
            if (config.isClearColorMatchBlocker)
            {
                profile.AddTarget(FieldTarget.ColorBlocker);
                profile.SetTargetCount(0, config.fixedMultiBlockerTargetCount, FieldTarget.ColorBlocker);
                // level 76: use fixedMultiBlockerTargetCount for isClearColorMatchBlocker
            }
            if (config.isClearMagicScrollGame)
            {
                profile.AddTarget(FieldTarget.MagicScroll);
                var scrollBegins = profile.slots.FindAll(s => s.scrollInfo != null && s.scrollInfo.Type == "BEGIN");
                profile.SetTargetCount(0, scrollBegins.Count, FieldTarget.MagicScroll);
            }
            if (config.isClearSpiderWebGame)
            {
                profile.AddTarget(FieldTarget.Curtain);
                // auto count target
                profile.SetTargetCount(0, curtainCount, FieldTarget.Curtain);
            }
            if (config.isClearMetalBrickGame)
            {
                profile.AddTarget(FieldTarget.MetalBrick);
                profile.SetTargetCount(0, metalCount, FieldTarget.MetalBrick);
            }
            if (config.isClearIceBrickGame)
            {
                profile.AddTarget(FieldTarget.IceBrick);
                profile.SetTargetCount(0, iceCount, FieldTarget.IceBrick);
            }
            if (config.isClearRandomChangerGame)
            {
                profile.AddTarget(FieldTarget.RandomChanger);
                profile.SetTargetCount(0, rndChangeCount, FieldTarget.RandomChanger);
            }

            if (config.isMaxMovesGame) profile.limitation = Limitation.Moves;
            else profile.limitation = Limitation.Time;
            profile.limit = config.allowedMoves;

            if (config.randomchangerEnable)
            {
                profile.randomChangerConfig.Enable = config.randomchangerEnable;
                profile.randomChangerConfig.GenerateInterval = config.randomchangerGenerateInterval;
                profile.randomChangerConfig.MinNumber = config.randomchangerMinNumber;
                profile.randomChangerConfig.MaxNumber = config.randomchangerMaxNumber;
                profile.randomChangerConfig.GenerateCount = config.randomChangerGenerateCount;
                if (config.randomchangerConvertProbability != null)
                {
                    int leng = config.randomchangerConvertProbability.Length;
                    profile.randomChangerConfig.ConvertProbability = new int[leng];
                    config.randomchangerConvertProbability.CopyTo(profile.randomChangerConfig.ConvertProbability, 0);
                }
            }
        }

        return profile;
    }

    public static LevelConfig ParseFromProfile(LevelProfile profile)
    {
        LevelConfig config = new LevelConfig();

        config.key = "themes.0.levels." + (profile.level - 1);

        config.author = "unknown";

        //NOTE: level config always has width = height = 9
        /*  profile.width * profile.height */
        int fieldSize = LevelProfile.maxSize * LevelProfile.maxSize;

        config.isMaxMovesGame = profile.limitation == Limitation.Moves;
        config.allowedMoves = profile.limit;
        config.isClearNormalBlockGame = profile.HasTarget(FieldTarget.Color);
        config.isClearUnderLayerGame = profile.HasTarget(FieldTarget.Stone);

        config.isClearFixedMultiBlocker = profile.HasTarget(FieldTarget.FixBlock);
        config.fixedMultiBlockerTargetCount = profile.GetTargetCount(0, FieldTarget.FixBlock);

        config.isClearColorMatchBlocker = profile.HasTarget(FieldTarget.ColorBlocker);
        if (config.isClearColorMatchBlocker)
        {
            config.fixedMultiBlockerTargetCount = profile.GetTargetCount(0, FieldTarget.ColorBlocker);
        }
        config.isClearBlockerGame = profile.HasTarget(FieldTarget.Blocker);
        int blockerTargets = Enum.GetValues(typeof(BlockerTargetType)).Length;
        config.blockers = new int[blockerTargets];
        for (int i = 0; i < blockerTargets; i++)
        {
            config.blockers[i] = profile.GetTargetCount(i, FieldTarget.Blocker);
        }

        config.isClearMagicScrollGame = profile.HasTarget(FieldTarget.MagicScroll);
        config.isClearSpiderWebGame = profile.HasTarget(FieldTarget.Curtain);
        config.isClearMetalBrickGame = profile.HasTarget(FieldTarget.MetalBrick);
        config.isClearIceBrickGame = profile.HasTarget(FieldTarget.IceBrick);
        config.isClearRandomChangerGame = profile.HasTarget(FieldTarget.RandomChanger);

        #region Key drop
        config.isCollectSinkerGame = profile.HasTarget(FieldTarget.KeyDrop);
        config.sinkers = new int[1];
        config.sinkers[0] = profile.GetTargetCount(0, FieldTarget.KeyDrop);
        List<SlotSettings> keyOutSlots = profile.slots.FindAll(a => a.tags.Contains(GameConst.k_sinkerGoal_tag));
        config.sinkerGoal = new int[keyOutSlots.Count * 2];
        for (int i = 0; i < keyOutSlots.Count; i++)
        {
            SlotSettings slot = keyOutSlots[i];

            config.sinkerGoal[i * 2] = slot.position.x;
            config.sinkerGoal[i * 2 + 1] = slot.position.y;
        }
        #endregion

        #region Butterfly
        config.isClearClimberGame = profile.HasTarget(FieldTarget.Butterfly);
        config.climberTargetCount = profile.GetTargetCount(0, FieldTarget.Butterfly);
        config.climberMaxOnScreen = profile.maxClimber;
        config.climberGenerateInterval = profile.climberGenerateInterval;

        List<SlotSettings> climberSpawn = profile.slots.FindAll(a => a.tags.Contains(GameConst.k_climberGenerator_tag));
        config.climberSpawn = new int[climberSpawn.Count * 2];
        for (int i = 0; i < climberSpawn.Count; i++)
        {
            SlotSettings slot = climberSpawn[i];
            config.climberSpawn[i * 2] = slot.position.x;
            config.climberSpawn[i * 2 + 1] = slot.position.y;
        }
        #endregion

        config.scoreToReach = new int[3];
        {
            config.scoreToReach[0] = profile.firstStarScore;
            config.scoreToReach[1] = profile.secondStarScore;
            config.scoreToReach[2] = profile.thirdStarScore;
        }

        config.numToGet = new int[6];
        {
            for (int i = 0; i < config.numToGet.Length; i++)
            {
                config.numToGet[i] = profile.GetTargetCount(i, FieldTarget.Color);
            }
        }

        config.easyProbability = profile.easyColorRatio;
        config.normalProbability = profile.normalColorRatio;

        int tmpPortalIndex = 0;

        config.panels = new int[fieldSize];
        config.strengths = new int[fieldSize];
        config.backlayers = new int[fieldSize];
        config.pieces = new int[fieldSize];
        config.colors = new int[fieldSize];
        config.startPieces = new bool[fieldSize];
        config.portalTypes = new int[fieldSize];
        config.portalIndices = new int[fieldSize];
        config.panelInfos = new string[fieldSize];
        config.magicScrollInfos = new string[fieldSize];
        config.conveyorInfos = new string[fieldSize];
        // if (GameConst.k_game_use_gravity) config.gravity = new int[fieldSize];
        {
            foreach (var slot in profile.slots)
            {
                int row = slot.position.y;
                row = LevelProfile.maxSize - 1 - row;
                int column = slot.position.x;
                int idx = row * LevelProfile.maxSize + column;

                if (!slot.visible)
                {
                    config.panels[idx] = (int)ETile.Invisible;
                }
                else
                {
                    int val = ConvertBlockType(slot.block_type);

                    config.panels[idx] = val;
                    config.strengths[idx] = slot.block_level;
                }

                config.colors[idx] = slot.color_id;

                // if (GameConst.k_game_use_gravity) config.gravity[idx] = (int)slot.gravity;

                config.pieces[idx] = GetChipValueFrom(slot.chip);
                config.backlayers[idx] = slot.stone_level;
                config.startPieces[idx] = slot.chip != "";

                if (slot.teleport != Utils.Vector2IntNull)
                {
                    tmpPortalIndex++;
                    config.portalIndices[idx] = tmpPortalIndex;
                    config.portalTypes[idx] = 1; // entry

                    int exitRow = LevelProfile.maxSize - 1 - slot.teleport.y;
                    int exitIdx = exitRow * LevelProfile.maxSize + slot.teleport.x;
                    config.portalIndices[exitIdx] = tmpPortalIndex;
                    config.portalTypes[exitIdx] = 2; // exit

                    // Debug.Log("<< Save portal: " + tmpPortalIndex + ", entry: row: " + exitRow + ", col: " + column
                    //     + ", Exit: row: " + slot.teleport.y + ", col: " + slot.teleport.x);
                }

                if (slot.scrollInfo != null && slot.block_type == GetBlockTypeFrom(ETile.Scroll))
                {
                    config.magicScrollInfos[idx] = JsonUtility.ToJson(slot.scrollInfo);
                }

                if (slot.bookShelfInfo != null
                    && (slot.block_type == GetBlockTypeFrom(ETile.BookShelf)
                            || slot.block_type == GetBlockTypeFrom(ETile.CandyTree))
                )
                {
                    config.panelInfos[idx] = JsonUtility.ToJson(slot.bookShelfInfo);
                }
                if (slot.bookShelfInfo != null && slot.block_type == GetBlockTypeFrom(ETile.MagicTap))
                {
                    config.panels[idx] = (int)ETile.MagicTap + slot.bookShelfInfo.Index;
                }

                if (slot.conveyorInfo != null)
                {
                    config.conveyorInfos[idx] = JsonUtility.ToJson(slot.conveyorInfo);
                }

                if (slot.switcherInfo != null && slot.block_type == GetBlockTypeFrom(ETile.Switcher))
                {
                    config.panelInfos[idx] = JsonUtility.ToJson(slot.switcherInfo);
                }
            }

            List<SlotSettings> generators = profile.slots.FindAll(a => a.generator);
            int spawnCount = Mathf.Min(generators.Count, LevelProfile.maxSize);
            config.defaultSpawnColumn = new int[spawnCount];
            int fillId = 0;
            for (int i = 0; i < generators.Count; i++)
            {
                if (Array.IndexOf(config.defaultSpawnColumn, generators[i].position.x) < 0)
                {
                    config.defaultSpawnColumn[fillId++] = generators[i].position.x;
                }

                // check if this slot is middle generator then mark panel value of above slot is 3 
                if (generators[i].position.y >= 0 && generators[i].position.y < LevelProfile.maxSize - 1)
                {
                    int aboveY = generators[i].position.y + 1;
                    SlotSettings aboveSlot = profile.slots.Find(s => s.position.y == aboveY && s.position.x == generators[i].position.x);
                    if (aboveSlot != null && !aboveSlot.visible)
                    {
                        int panelIdx = aboveSlot.position.x + LevelProfile.maxSize * (LevelProfile.maxSize - 1 - aboveY);
                        config.panels[panelIdx] = (int)ETile.ColumnGeneratorMark;
                    }
                }
            }

            List<SlotSettings> blockerGens = generators.FindAll(a => a.tags.Contains(GameConst.k_blockGenerator_tag));
            config.blockerSpawnColumn = new int[blockerGens.Count];
            for (int i = 0; i < blockerGens.Count; i++)
            {
                config.blockerSpawnColumn[i] = blockerGens[i].position.x;
            }

            if (profile.randomChangerConfig != null)
            {
                config.randomchangerEnable = profile.randomChangerConfig.Enable;
                config.randomChangerGenerateCount = profile.randomChangerConfig.GenerateCount;
                config.randomchangerGenerateInterval = profile.randomChangerConfig.GenerateInterval;
                config.randomchangerMinNumber = profile.randomChangerConfig.MinNumber;
                config.randomchangerMaxNumber = profile.randomChangerConfig.MaxNumber;
                if (profile.randomChangerConfig.ConvertProbability != null)
                {
                    config.randomchangerConvertProbability = new int[profile.randomChangerConfig.ConvertProbability.Length];
                    profile.randomChangerConfig.ConvertProbability.CopyTo(config.randomchangerConvertProbability, 0);
                }
                else
                {
                    config.randomchangerConvertProbability = new int[13];
                }
            }
        }

        return config;
    }
    ////////////////////////////////////////////////////////////////////////////////
    public static string GetBlockTypeFrom(ETile tile)
    {
        switch (tile)
        {
            case ETile.MagicBook: return "magicBook";
            case ETile.Cage: return "cage";
            case ETile.ColorBook: return "colorBook";
            case ETile.Smoke: return "Smoke";
            case ETile.Scroll: return "scroll";
            case ETile.JewelMold: return "JewelMold";
            case ETile.Curtain: return "Curtain";
            case ETile.BookShelf: return "BookShelf";
            case ETile.Lamp: return "Lamp";
            case ETile.MagicTap: return "MagicTap";
            case ETile.MagicTap + 1: return "MagicTap";
            case ETile.MagicTap + 2: return "MagicTap";
            case ETile.MagicTap + 3: return "MagicTap";
            case ETile.Metal: return "Metal";
            case ETile.Jewelcase: return "Jewelcase";
            case ETile.ColorChanger: return "ColorChanger";
            case ETile.CandyTree: return "CandyTree";
            case ETile.Switcher: return "Switcher";
            default:
                return "";
        }
    }

    static int ConvertBlockType(string typeStr)
    {
        ETile tile = ETile.Visible;
        switch (typeStr)
        {
            case "magicBook": tile = ETile.MagicBook; break;
            case "cage": tile = ETile.Cage; break;
            case "colorBook": tile = ETile.ColorBook; break;
            case "Smoke": tile = ETile.Smoke; break;
            case "scroll": tile = ETile.Scroll; break;
            case "JewelMold": tile = ETile.JewelMold; break;
            case "Curtain": tile = ETile.Curtain; break;
            case "BookShelf": tile = ETile.BookShelf; break;
            case "Lamp": tile = ETile.Lamp; break;
            case "MagicTap": tile = ETile.MagicTap; break;
            case "Metal": tile = ETile.Metal; break;
            case "Jewelcase": tile = ETile.Jewelcase; break;
            case "ColorChanger": tile = ETile.ColorChanger; break;
            case "CandyTree": tile = ETile.CandyTree; break;
            case "Switcher": tile = ETile.Switcher; break;
        }
        return (int)tile;
    }

    static Dictionary<int, string> chipDict = new Dictionary<int, string>()
    {
        {(int)EPieces.SimpleChip, EPieces.SimpleChip.ToString()},
        {(int)EPieces.SimpleBomb, EPieces.SimpleBomb.ToString()},
        {(int)EPieces.VLineBomb, EPieces.VLineBomb.ToString()},
        {(int)EPieces.HLineBomb, EPieces.HLineBomb.ToString()},
        {(int)EPieces.CrossBomb, EPieces.CrossBomb.ToString()},
        {(int)EPieces.RainbowBomb, EPieces.RainbowBomb.ToString()},
        {(int)EPieces.Compass, EPieces.Compass.ToString()},
        {(int)EPieces.Compass2, EPieces.Compass2.ToString()},
        {(int)EPieces.KeyDrop, EPieces.KeyDrop.ToString()},
        {(int)EPieces.TimeBomb, EPieces.TimeBomb.ToString()},
        {(int)EPieces.Butterfly, EPieces.Butterfly.ToString()},
        {(int)EPieces.Portion, EPieces.Portion.ToString()},
        {(int)EPieces.Crystal, EPieces.Crystal.ToString()},
        {(int)EPieces.SmokePot, EPieces.SmokePot.ToString()},
        {(int)EPieces.HalfIceChip, EPieces.HalfIceChip.ToString()},
        {(int)EPieces.FullIceChip, EPieces.FullIceChip.ToString()},
        {(int)EPieces.HalfIceLineBomb, EPieces.HalfIceLineBomb.ToString()},
        {(int)EPieces.FullIceLineBomb, EPieces.FullIceLineBomb.ToString()},
    };
    public static string GetChipTypeFrom(int pieceValue)
    {
        if (chipDict.ContainsKey(pieceValue))
        {
            return chipDict[pieceValue];
        }
        // return empty value
        return "";
    }

    static int GetChipValueFrom(string piece)
    {
        if (chipDict.ContainsValue(piece))
        {
            EPieces pieceType = EPieces.SimpleChip;
            bool ok = Enum.TryParse(piece, true, out pieceType);
            if (ok)
            {
                return (int)pieceType;
            }
        }
        return 0;
    }
    static bool BlockCanContainChip(ETile blockTile)
    {
        switch (blockTile)
        {
            case ETile.Cage:
            case ETile.ColorChanger:
            case ETile.Switcher:
                return true;
            default:
                return false;
        }
    }

    static bool IsPanelSlotVisible(int panelValue)
    {
        return panelValue != (int)ETile.Invisible
            && panelValue != (int)ETile.ColumnGeneratorMark;
    }
}
