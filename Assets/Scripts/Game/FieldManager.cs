using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FieldManager : Singleton<FieldManager>
{
    [HideInInspector]
    public Field field; //the field of current level

    private void Start()
    {
        StartLevel();
    }
    public void StartLevel()
    {
        int level = PlayerPrefs.GetInt("TestLevel", 0);
        if (level == 0) // no testing
        {
            if (PlayerManager.Instance)
            {
                level = PlayerManager.Instance.m_levelSelected + 1;

            }
            else
            {
                level = 1;
            }
        }
        else
        {
            PlayerPrefs.DeleteKey("TestLevel");
        }
        LevelProfile.main = LevelProfile.LoadLevel(level);
        StartCoroutine(SetTargetUI());
        StartCoroutine(StartLevelRoutine());

        PopupControl.Instance.OpenPopup("ShowTargetStartLevel");
    }

    IEnumerator SetTargetUI()
    {
        yield return new WaitForEndOfFrame();
        GameplayPanel.m_eventSetTargetUI.Invoke();
    }

    IEnumerator StartLevelRoutine()
    {
        Session.Reset();

        if (PlayerManager.Instance)
        {
            PlayerManager.Instance.m_PlayerLive--;
            PlayerManager.Instance.SaveGame();
        }
        yield return StartCoroutine(CreateField());

        Session.Instance.StartSession(FieldTarget.Color /* LevelProfile.main.target */, LevelProfile.main.limitation);

    }

    public IEnumerator CreateField()
    {
        RemoveField(); // Removing old field

        field = new Field(LevelProfile.main.GetClone());

        Slot.folder = new GameObject().transform;
        Slot.folder.name = "Slots";
        Slot.folder.gameObject.layer = 8;
        Vector3 fieldDimensions = new Vector3(field.width - 1, field.height - 1, 0) * CommonConfig.main.slot_offset;

        Slot.all.Clear();

        float blockerRatioPerSlot = field.blockerSpawnCount == 0 ? 0 : 100f / field.blockerSpawnCount;

        foreach (SlotSettings settings in field.slots.Values)
        {
            yield return 0;
            Slot slot;

            #region Creating a new empty slot
            Vector3 position = new Vector3(settings.position.x, settings.position.y, 0) * CommonConfig.main.slot_offset - fieldDimensions / 2;
            GameObject obj = ContentManager.Instance.GetItem("SlotEmpty", position);
            obj.name = "Slot_" + settings.position.x + "x" + settings.position.y;
            obj.transform.SetParent(Slot.folder);
            slot = obj.GetComponent<Slot>();
            slot.coord = settings.position;
            slot.gameObject.layer = 8;
            Slot.all.Add(slot.coord, slot);
            #endregion

            #region Create generator
            if (settings.generator)
            {
                var generator = slot.gameObject.AddComponent<SlotGenerator>();
                if (settings.tags.Contains(GameConst.k_blockGenerator_tag))
                {
                    generator.generateBlocker = true;
                    generator.spawnBlockerRatio = blockerRatioPerSlot;
                }
            }
            #endregion

            #region Creating a teleport
            if (settings.teleport != Utils.Vector2IntNull)
            {
                slot.slotTeleport.target_postion = settings.teleport;

                GameObject portIn = ContentManager.Instance.GetItem("portalIn", position);
                portIn.transform.parent = slot.transform;
                portIn.transform.localPosition = Vector3.zero;
                portIn.transform.Rotate(0, 0, Utils.SideToAngle(settings.gravity) + 90);
            }
            else { Destroy(slot.slotTeleport); }
            #endregion

            #region Setting gravity direction
            slot.slotGravity.gravityDirection = settings.gravity;
            #endregion

            #region Setting Key target (use slot's tag)
            if (LevelProfile.main.HasTarget(FieldTarget.KeyDrop) && settings.tags.Contains(GameConst.k_sinkerGoal_tag))
            {
                slot.sugarDropSlot = true;
                GameObject keySlot = ContentManager.Instance.GetItem("SlotKeyGoal", position);
                keySlot.name = "SlotKeyGoal";
                keySlot.transform.parent = slot.transform;
                keySlot.transform.localPosition = Vector3.zero;
                keySlot.transform.Rotate(0, 0, Utils.SideToAngle(settings.gravity) + 90);
            }
            #endregion

            #region Creating a block
            // implement block
            if (settings.block_type != "")
            {
                Session.BlockInfo blockInfo = Session.Instance.blockInfos.Find(x => x.name == settings.block_type);
                string contentName = settings.block_type;
                if (blockInfo != null)
                {
                    if (blockInfo.color)
                    {
                        // NOTE: if block has color, use color_id as its color
                        // Attention it should not work with block can contain chip(piece)
                        contentName += Chip.chipTypes[Mathf.Clamp(settings.color_id, 0, Chip.colors.Length - 1)];
                    }

                    if (blockInfo.name == LevelConfig.GetBlockTypeFrom(ETile.Switcher))
                    {
                        int id = (settings.switcherInfo.GroupIndex - 1) % Chip.colors.Length;
                        contentName += Chip.chipTypes[id];
                    }
                }
                GameObject b_obj = ContentManager.Instance.GetItem(contentName);
                b_obj.transform.SetParent(slot.transform);
                b_obj.transform.localPosition = Vector3.zero;
                b_obj.name = settings.block_type + "_" + settings.position.x + "x" + settings.position.y;
                IBlock block = b_obj.GetComponent<IBlock>();

                // don't need to keep these kinds of block for this slot because it won't block chip movement
                if (settings.block_type != LevelConfig.GetBlockTypeFrom(ETile.ColorChanger)
                    && settings.block_type != LevelConfig.GetBlockTypeFrom(ETile.Switcher))
                {
                    slot.block = block;
                }
                block.slot = slot;
                block.level = settings.block_level;
                block.Initialize(settings);
            }
            #endregion

            #region Create a chip/pieces
            if (!string.IsNullOrEmpty(settings.chip) && (slot.block == null || slot.block.CanItContainChip()))
            {
                Session.ChipInfo chipInfo = Session.Instance.chipInfos.Find(x => x.name == settings.chip);
                if (chipInfo != null)
                {
                    string key = chipInfo.contentName + (chipInfo.color && settings.color_id >= 0 ? Chip.chipTypes[Mathf.Clamp(settings.color_id, 0, Chip.colors.Length - 1)] : "");
                    GameObject c_obj = ContentManager.Instance.GetItem(key);
                    c_obj.transform.SetParent(slot.transform);
                    c_obj.transform.localPosition = Vector3.zero;
                    c_obj.name = key;
                    slot.chip = c_obj.GetComponent<Chip>();
                }
            }
            #endregion

            #region Create stone tile (backlayers)
            if (settings.stone_level >= 0)
            {
                GameObject j_obj;
                j_obj = ContentManager.Instance.GetItem("stone");
                j_obj.transform.SetParent(slot.transform);
                j_obj.transform.localPosition = Vector3.zero;
                j_obj.name = "Stone_" + settings.position.x + "x" + settings.position.y;
                BackLayer stone = j_obj.GetComponent<BackLayer>();
                slot.stone = stone;
                stone.SetLevel(settings.stone_level);
            }
            #endregion

            #region Add ClimberGenerator
            if (settings.tags.Contains(GameConst.k_climberGenerator_tag))
            {
                var generator = slot.gameObject.AddComponent<ClimberGenerator>();
            }
            #endregion

            #region Conveyor
            if (settings.conveyorInfo != null)
            {
                GameObject c_obj = ContentManager.Instance.GetItem("Conveyor");
                c_obj.transform.SetParent(slot.transform);
                c_obj.transform.localPosition = Vector3.zero;
                c_obj.gameObject.name = "conveyor_" + settings.position.x + "x" + settings.position.y;
                ConveyorTile tile = c_obj.GetComponent<ConveyorTile>();
                tile.SetConveyorInfo(settings.conveyorInfo);
                tile.slot = slot;
            }
            #endregion
        }

        Slot.Initialize();

        SlotGravity.Reshading();

        ConveyorTile.Initialize();
    }

    public void RemoveField()
    {
        if (Slot.folder)
        {
            Destroy(Slot.folder.gameObject);
        }
    }

    public Chip GetNewBlockerChip(Vector2Int coord, Vector3 position, int level)
    {
        EPieces piece = level == 2 ? EPieces.Compass2 : EPieces.Compass;
        GameObject o = ContentManager.Instance.GetItem(LevelConfig.GetChipTypeFrom((int)piece));
        o.transform.position = position;
        o.name = "Compass";
        if (Slot.GetSlot(coord).chip)
        {
            o.transform.position = Slot.GetSlot(coord).chip.transform.position;
        }
        Chip chip = o.GetComponent<Chip>();
        Slot.GetSlot(coord).chip = chip;
        return chip;
    }
    public IBlock GetNewFixBlockerChip(Vector2Int coord, Vector3 position, int level)
    {
        GameObject o = ContentManager.Instance.GetItem(LevelConfig.GetBlockTypeFrom(ETile.MagicBook));
        o.transform.position = position;
        Slot slot = Slot.GetSlot(coord);
        o.transform.SetParent(slot.transform);
        var block = o.GetComponent<IBlock>();
        slot.chip.HideChip(false);
        slot.block = block;
        block.level = level;
        block.slot = slot;
        block.Initialize();
        return block;
    }

    public IBlock GetNewCageBlock(Vector2Int coord)
    {
        Slot slot = Slot.GetSlot(coord);

        GameObject o = ContentManager.Instance.GetItem(LevelConfig.GetBlockTypeFrom(ETile.Cage));
        o.transform.position = slot.transform.position;
        o.transform.SetParent(slot.transform);

        var block = o.GetComponent<IBlock>();
        slot.block = block;
        block.level = 1;
        block.slot = slot;
        block.Initialize();
        return block;
    }
    public IBlock GetNewCurtain(Vector2Int coord, int level)
    {
        Slot slot = Slot.GetSlot(coord);
        GameObject o = ContentManager.Instance.GetItem(LevelConfig.GetBlockTypeFrom(ETile.Curtain));
        o.transform.position = slot.transform.position;
        o.transform.SetParent(slot.transform);

        var block = o.GetComponent<IBlock>();
        slot.chip.HideChip(false);
        slot.block = block;
        block.level = level;
        block.slot = slot;
        block.Initialize();
        return block;
    }

    public IBlock GetNewSmoke(Vector2Int coord)
    {
        Slot slot = Slot.GetSlot(coord);

        GameObject o = ContentManager.Instance.GetItem(LevelConfig.GetBlockTypeFrom(ETile.Smoke));
        o.transform.position = slot.transform.position;
        o.transform.SetParent(slot.transform);

        var block = o.GetComponent<IBlock>();
        slot.block = block;
        block.level = 1;
        block.slot = slot;
        block.Initialize();
        return block;
    }

    public Chip GetNewSimpleChip(Vector2Int coord, Vector3 position)
    {
        int color = LevelProfile.main.GetColorRandom();
        return GetNewSimpleChip(coord, position, Session.Instance.colorMask[color]);
    }

    public Chip GetNewSimpleChip(Vector2Int coord, Vector3 position, int id)
    {
        GameObject o = ContentManager.Instance.GetItem("SimpleChip" + Chip.chipTypes[id]);
        o.transform.position = position;
        o.name = "Chip_" + Chip.chipTypes[id];
        if (Slot.GetSlot(coord).chip)
        {
            o.transform.position = Slot.GetSlot(coord).chip.transform.position;
        }
        Chip chip = o.GetComponent<Chip>();
        Slot.GetSlot(coord).chip = chip;
        return chip;
    }

    public Chip GetSinkerKey(Vector2Int coord, Vector3 position)
    {
        GameObject o = ContentManager.Instance.GetItem("SinkerKey");
        o.transform.position = position;
        o.name = "KEY";
        if (Slot.GetSlot(coord).chip)
        {
            o.transform.position = Slot.GetSlot(coord).chip.transform.position;
        }
        Chip chip = o.GetComponent<Chip>();
        Slot.GetSlot(coord).chip = chip;
        return chip;
    }

    public Chip GetNewButterfly(Vector2Int coord, Vector3 position)
    {
        int color = LevelProfile.main.GetColorRandom();
        GameObject o = ContentManager.Instance.GetItem("Butterfly" + Chip.chipTypes[color]);
        o.transform.position = position;
        Chip currentChip = Slot.GetSlot(coord).chip;
        if (currentChip)
        {
            o.transform.position = currentChip.transform.position;
            currentChip.HideChip(false); //hide old chip
        }
        Chip chip = o.GetComponent<Chip>();
        Slot.GetSlot(coord).chip = chip;
        return chip;
    }

    public Chip GetNewMagicPortion(Vector2Int coord, Vector3 position)
    {
        int color = LevelProfile.main.GetColorRandom();
        GameObject o = ContentManager.Instance.GetItem(LevelConfig.GetChipTypeFrom((int)EPieces.Portion) + Chip.chipTypes[color]);
        o.transform.position = position;
        Chip currentChip = Slot.GetSlot(coord).chip;
        if (currentChip)
        {
            o.transform.position = currentChip.transform.position;
            currentChip.HideChip(false);
        }
        Chip chip = o.GetComponent<Chip>();
        Slot.GetSlot(coord).chip = chip;
        return chip;
    }

    public void AddPowerup(string powerup)
    {
        // TODO: optimize this find Func
        List<SimpleChip> chips = new List<SimpleChip>(FindObjectsOfType<SimpleChip>());
        if (chips.Count == 0) return;

        SimpleChip chip = chips.Where(x => x != null && !x.chip.IsBusy).ToList().GetRandom();
        if (chip)
        {
            Slot slot = chip.chip.slot;
            if (slot)
            {
                AddPowerup(slot.coord, powerup);
            }
        }
    }

    public Chip AddPowerup(Vector2Int coord, string powerup)
    {
        Slot slot = Slot.GetSlot(coord).GetComponent<Slot>();
        Chip chip = slot.chip;
        int id;
        if (chip)
        {
            id = chip.id;
        }
        else
        {
            id = UnityEngine.Random.Range(0, Chip.colors.Length);
        }
        if (chip)
        {
            Destroy(chip.gameObject);
        }

        chip = GetNewBomb(slot.coord, powerup, slot.transform.position, id);
        return chip;
    }

    public Chip GetNewBomb(Vector2Int coord, string powerup, Vector3 position, int id)
    {
        Session.ChipInfo p = Session.Instance.chipInfos.Find(pu => pu.name == powerup);
        if (p == null)
        {
            return null;
        }
        id = Mathf.Clamp(id, 0, Chip.colors.Length);
        string bombName = p.contentName + (p.color ? Chip.chipTypes[id] : "");
        GameObject o = ContentManager.Instance.GetItem(bombName);
        o.transform.position = position;

        o.name = bombName;
        if (Slot.GetSlot(coord).chip)
        {
            o.transform.position = Slot.GetSlot(coord).chip.transform.position;
        }
        Chip chip = o.GetComponent<Chip>();
        Slot.GetSlot(coord).chip = chip;
        return chip;
    }

    public void BlockCrush(Vector2Int coord, bool radius, bool force = false)
    {
        IBlock block = null;
        Slot slot = null;
        Chip chip = null; // neighbor chip
        Chip currentChip = null;

        slot = Slot.GetSlot(coord);
        if (slot)
        {
            currentChip = slot.chip;
        }

        if (radius)
        {
            foreach (Side side in Utils.straightSides)
            {
                block = null;
                slot = null;
                chip = null;

                slot = Slot.GetSlot(Utils.Vec2IntAdd(coord, side));
                if (!slot)
                    continue;

                block = slot.block;
                if (block && block.CanBeCrushedByNearSlot(currentChip))
                    block.BlockCrush(force);

                if (slot) chip = slot.chip;
                if (chip && chip.logic is IChipAffectByNeighBor)
                {
                    var neighborChip = (IChipAffectByNeighBor)chip.logic;
                    if (neighborChip.IsCanEffectedByNeighbor())
                    {
                        neighborChip.NeighborChip = currentChip;
                        chip.DestroyChip();
                    }
                }
            }
        }
        else
        {

            if (slot)
            {
                block = slot.block;
            }
            if (block)
            {
                block.BlockCrush(force);
            }
        }
    }

    public void StoneCrush(Vector2Int coord)
    {
        Slot s = Slot.GetSlot(coord);
        if (s && s.stone)
        {
            if (s.block && s.block is MetalBrick)
                return;
            s.stone.StoneCrush();
        }
    }

    public void DestroyAllChips()
    {
        foreach (var slot in Slot.all.Values)
        {
            if (slot.chip)
            {
                StoneCrush(slot.coord);
                slot.chip.SetScore(GameConst.k_base_scoreChip);
                slot.chip.DestroyChip();
            }
        }
    }
}