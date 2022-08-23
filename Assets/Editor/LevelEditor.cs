using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;

public class LevelEditor : MetaEditor
{
    int m_level;
    Rect rect;

    static int cellSize = 40;
    static int legendSize = 20;
    static int slotOffset = 4;

    static Color defaultColor;
    Color[] chipColor;

    static GUIStyle mLabelStyle;

    static bool k_use_gravity = GameConst.k_game_use_gravity;
    Vector2 settingScroll;

    public static GUIStyle labelStyle
    {
        get
        {
            if (mLabelStyle == null)
            {
                mLabelStyle = new GUIStyle(GUI.skin.button);
                mLabelStyle.wordWrap = true;
                ;
                mLabelStyle.normal.background = null;
                mLabelStyle.focused.background = null;
                mLabelStyle.active.background = null;

                mLabelStyle.normal.textColor = Color.black;
                mLabelStyle.focused.textColor = mLabelStyle.normal.textColor;
                mLabelStyle.active.textColor = mLabelStyle.normal.textColor;

                mLabelStyle.fontSize = 8;
                mLabelStyle.margin = new RectOffset();
                mLabelStyle.padding = new RectOffset();
            }
            return mLabelStyle;
        }
    }

    Dictionary<Vector2Int, SlotSettings> slots = new Dictionary<Vector2Int, SlotSettings>();
    Dictionary<string, Session.ChipInfo> chipInfos = new Dictionary<string, Session.ChipInfo>();
    Dictionary<string, Session.BlockInfo> blockInfos = new Dictionary<string, Session.BlockInfo>();
    List<Vector2Int> teleportTargets = new List<Vector2Int>();
    SlotSettings target_selection;
    bool wait_target = false;

    public static Dictionary<string, bool> layers = new Dictionary<string, bool>();
    List<Vector2Int> selected = new List<Vector2Int>();

    #region Icons
    public static Texture slotIcon;
    public static Texture chipIcon;
    public static Texture stoneIcon;
    public static Texture jamAIcon;
    public static Texture blockIcon;
    public static Texture generatorIcon;
    public static Texture teleportIcon, teleOutIcon;
    public static Texture keyOutIcon;
    public static Texture wallhIcon;
    public static Texture wallvIcon;
    public static Dictionary<Side, Texture> gravityIcon = new Dictionary<Side, Texture>();
    public static Dictionary<string, Texture> blockIcons = new Dictionary<string, Texture>();
    public static Dictionary<string, Texture> piecesIcons = new Dictionary<string, Texture>();
    public static Dictionary<string, Texture> conveyorIcons = new Dictionary<string, Texture>();

    static string[] alphabet = { "A", "B", "C", "D", "E", "F" };

    Texture LoadIcon(string resource)
    {
        return EditorUtil.LoadAssetTexture(resource);
    }
    #endregion

    #region Level variables
    LevelConfig m_lvConfig;
    LevelProfile profile;
    #endregion

    bool targetSetting, probSetting, climberSetting, portionSetting = false;
    Vector2Int copiedCoord = Utils.Vector2IntNull;

    ////////////////////////////////////////////////////////////////////////////////

    public override void OnInspectorGUI()
    {
        if (profile == null)
        {
            profile = new LevelProfile();

            ResetField();

        }
        teleportTargets.Clear();
        // foreach (Vector2Int coord in selected)
        foreach (var slot in slots)
        {
            Vector2Int coord = slot.Key;
            if (slots.ContainsKey(coord) && !teleportTargets.Contains(slots[coord].teleport))
            {
                teleportTargets.Add(slots[coord].teleport);
            }
        }

        #region Level Parameters
        EditorGUILayout.BeginVertical(EditorStyles.textArea);
        GUILayout.Label("Level Parameters", EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true));

        #region Navigation Panel
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("<<", EditorStyles.miniButtonLeft, GUILayout.Width(40)))
        {
            SelectLevel(1);
            return;
        }
        if (GUILayout.Button("<", EditorStyles.miniButtonRight, GUILayout.Width(40)))
        {
            if (m_level - 1 > 0)
            {
                SelectLevel(m_level - 1);
            }
            return;
        }

        GUILayout.Label("Level :", EditorStyles.label, GUILayout.Width(40));
        string changeLvl = GUILayout.TextField("" + m_level, new GUILayoutOption[] { GUILayout.Width(50) });
        try
        {
            if (int.Parse(changeLvl) != m_level)
            {
                m_level = int.Parse(changeLvl);
                SelectLevel(m_level);
            }
        }
        catch (Exception)
        {
        }

        if (GUILayout.Button(">", EditorStyles.miniButtonMid, GUILayout.Width(40)))
        {
            SelectLevel(m_level + 1);
            return;
        }
        if (GUILayout.Button(">>", EditorStyles.miniButtonRight, GUILayout.Width(40)))
        {
            SelectLevel(GetLastFileIndex());
            return;
        }

        EditorGUILayout.EndHorizontal();
        #endregion


        // /* profile.width =  */
        // Mathf.RoundToInt(EditorGUILayout.Slider("Width", 1f * LevelProfile.maxSize, 5f, LevelProfile.maxSize));
        // /* profile.height =  */
        // Mathf.RoundToInt(EditorGUILayout.Slider("Height", 1f * LevelProfile.maxSize, 5f, LevelProfile.maxSize));
        // profile.colorCount = Mathf.RoundToInt(EditorGUILayout.Slider("Count of Possible Colors", 1f * profile.colorCount, 3f, chipColor.Length));

        #region Stars
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Score Stars", GUILayout.ExpandWidth(true));
        profile.firstStarScore = Mathf.Max(EditorGUILayout.IntField(profile.firstStarScore, GUILayout.ExpandWidth(true)), 1);
        profile.secondStarScore = Mathf.Max(EditorGUILayout.IntField(profile.secondStarScore, GUILayout.ExpandWidth(true)), profile.firstStarScore + 1);
        profile.thirdStarScore = Mathf.Max(EditorGUILayout.IntField(profile.thirdStarScore, GUILayout.ExpandWidth(true)), profile.secondStarScore + 1);
        EditorGUILayout.EndHorizontal();
        #endregion Star

        #region Limitation
        Enum limitation = EditorGUILayout.EnumPopup("Limitation", profile.limitation);
        if (profile.limitation != (Limitation)limitation)
        {
            profile.limit = 0;
        }
        profile.limitation = (Limitation)limitation;
        switch (profile.limitation)
        {
            case Limitation.Moves:
                profile.limit = Mathf.RoundToInt(EditorGUILayout.Slider("Move Count", profile.limit, 5, 100));
                break;
            case Limitation.Time:
                profile.limit = Mathf.RoundToInt(EditorGUILayout.Slider("Game duration (" + Utils.ToTimerFormat(profile.limit) + ")", Mathf.Ceil(profile.limit / 5) * 5, 5, 300));
                break;
        }
        #endregion

        #region Target
        targetSetting = EditorGUILayout.Foldout(targetSetting, "Target settings", EditorStyles.foldout);
        if (targetSetting)
        {
            for (int i = 0; i < profile.allTargets.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                LevelTarget target = profile.allTargets[i];
                FieldTarget type = target.Type;

                type = (FieldTarget)EditorGUILayout.EnumPopup("Target " + (i + 1) + ":", type);
                if (type != target.Type)
                {
                    target.ChangeType(type);
                }

                // delete target btn
                defaultColor = GUI.color;
                GUI.color = Color.Lerp(defaultColor, Color.red, 0.3f);
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    profile.allTargets.RemoveAt(i);
                }
                GUI.color = defaultColor;
                EditorGUILayout.EndHorizontal();

                // target detail
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.BeginVertical();
                //don't need to show target count
                bool autoTarget = target.Type == FieldTarget.Stone
                    || target.Type == FieldTarget.Curtain;

                for (int j = 0; j < target.GetNumberTargets(); j++)
                {
                    if (!autoTarget)
                    {
                        target.SetTargetCount(j,
                            Mathf.Clamp(EditorGUILayout.IntField("Value " + j,
                                target.GetTargetCount(j)),
                            0, 999)
                        );
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            defaultColor = GUI.color;
            GUI.color = Color.Lerp(defaultColor, Color.blue, 0.3f);
            if (GUILayout.Button("Add Target", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                profile.AddTarget(FieldTarget.None);
            }
            EditorGUILayout.EndHorizontal();

            GUI.color = defaultColor;
        }
        #endregion

        #region Probability
        probSetting = EditorGUILayout.Foldout(probSetting, "Probability settings", EditorStyles.foldout);
        if (probSetting)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Easy:");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            for (int i = 0; i < profile.colorCount; i++)
            {
                profile.easyColorRatio[i] = Mathf.Max(0, EditorGUILayout.IntField(profile.easyColorRatio[i], GUILayout.ExpandWidth(true)));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Normal:");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            for (int i = 0; i < profile.colorCount; i++)
            {
                profile.normalColorRatio[i] = Mathf.Max(0, EditorGUILayout.IntField(profile.normalColorRatio[i], GUILayout.ExpandWidth(true)));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Climber (Butterfly)
        climberSetting = EditorGUILayout.Foldout(climberSetting, "Climber(BF) settings", EditorStyles.foldout);
        if (climberSetting)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max Climber on Screen", GUILayout.Width(200));
            EditorGUILayout.Space();
            profile.maxClimber = Mathf.Max(0, EditorGUILayout.IntField(profile.maxClimber, GUILayout.ExpandWidth(true)));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Climber generate interval", GUILayout.Width(200));
            EditorGUILayout.Space();
            profile.climberGenerateInterval = Mathf.Max(0, EditorGUILayout.IntField(profile.climberGenerateInterval, GUILayout.ExpandWidth(true)));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        #endregion
        #region Random changer (Magic portion)
        portionSetting = EditorGUILayout.Foldout(portionSetting, "Random changer settings", EditorStyles.foldout);
        if (portionSetting)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            profile.randomChangerConfig.Enable = EditorGUILayout.ToggleLeft("Enable Random changer",
                profile.randomChangerConfig.Enable);
            EditorGUILayout.EndHorizontal();
            if (profile.randomChangerConfig.Enable)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.BeginVertical();

                EditorGUILayout.BeginHorizontal();
                profile.randomChangerConfig.GenerateInterval = Mathf.Max(1, EditorGUILayout.IntField("Generate Interval:",
                    profile.randomChangerConfig.GenerateInterval,
                    GUILayout.ExpandWidth(true), GUILayout.MaxWidth(200)));

                GUILayout.Space(30);
                profile.randomChangerConfig.GenerateCount = Mathf.Max(1, EditorGUILayout.IntField("Generate Count:",
                    profile.randomChangerConfig.GenerateCount,
                    GUILayout.ExpandWidth(true), GUILayout.MaxWidth(200)));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                profile.randomChangerConfig.MinNumber = Mathf.Max(0, EditorGUILayout.IntField("Min Number:",
                    profile.randomChangerConfig.MinNumber,
                    GUILayout.ExpandWidth(true), GUILayout.MaxWidth(200)));

                GUILayout.Space(30);
                profile.randomChangerConfig.MaxNumber = Mathf.Max(profile.randomChangerConfig.MinNumber, EditorGUILayout.IntField("Max Number:",
                    profile.randomChangerConfig.MaxNumber,
                    GUILayout.ExpandWidth(true), GUILayout.MaxWidth(200)));

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Probability:");
                if (profile.randomChangerConfig.ConvertProbability == null)
                {
                    profile.randomChangerConfig.ConvertProbability = new int[RandomChangerConfig.s_randomItems.Length];
                }
                for (int i = 0; i < profile.randomChangerConfig.ConvertProbability.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    var item = RandomChangerConfig.s_randomItems[i];
                    profile.randomChangerConfig.ConvertProbability[i] = Mathf.Max(0, EditorGUILayout.IntField(
                        "Ratio " + i + " (" + item.Item + "_" + item.Level + ")",
                        profile.randomChangerConfig.ConvertProbability[i],
                        GUILayout.MinWidth(100), GUILayout.MaxWidth(200)));
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (profile.randomChangerConfig.ConvertProbability != null)
                {
                    profile.randomChangerConfig.ConvertProbability = null;
                }
            }
        }
        #endregion

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
        #endregion Level Parameters

        #region Slot parameters
        GUILayout.Label("Slot Parameters", EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true));
        EditorGUILayout.BeginHorizontal();

        DrawSlotSettings();
        DrawLayersSettings();

        EditorGUILayout.EndHorizontal();

        #endregion Slot parameters

        GUILayout.Label("Level Layout", EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true));
        DrawActionBar();

        defaultColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.gray;
        EditorGUILayout.BeginHorizontal(EditorStyles.textArea, GUILayout.MinWidth(10));
        GUI.backgroundColor = defaultColor;

        rect = GUILayoutUtility.GetRect(
            /* profile.width */LevelProfile.maxSize * (cellSize + slotOffset) + legendSize + EditorStyles.textArea.margin.left + EditorStyles.textArea.margin.right,
            /* profile.height */LevelProfile.maxSize * (cellSize + slotOffset) + legendSize + EditorStyles.textArea.margin.top + EditorStyles.textArea.margin.bottom);


        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        DrawFieldView();

        OnSceneGUI();
    }
    private void OnSceneGUI()
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            if (e.control)
            {
                if (e.keyCode == KeyCode.C)
                {
                    if (selected.Count == 1 && copiedCoord != selected[0])
                    {
                        copiedCoord = selected[0];
                        Debug.Log("copy " + copiedCoord);
                    }
                }
                else if (e.keyCode == KeyCode.V)
                {
                    if (copiedCoord != Utils.Vector2IntNull && selected.Count > 0)
                    {
                        SlotSettings slotCopy = slots[copiedCoord];
                        foreach (var slotDest in selected)
                        {
                            if (slotDest != copiedCoord)
                            {
                                Debug.Log("Paste " + copiedCoord + " to " + slotDest);
                                SlotUndo.Record(slots[slotDest]);
                                slots[slotDest] = slotCopy.GetDeepCopy();
                                slots[slotDest].position = slotDest;
                            }
                        }

                        Event.current.Use();

                    }
                }
                else if (e.keyCode == KeyCode.Z)
                {
                    SlotSettings lastSlot = SlotUndo.GetTop();
                    if (lastSlot != null)
                    {
                        Vector2Int coord = lastSlot.position;
                        slots[coord] = lastSlot;

                        Event.current.Use();
                    }
                }
            }
            else if (e.keyCode == KeyCode.Delete)
            {
                DeleteCells();
                Event.current.Use();
            }
        }
    }

    void OnEnable()
    {
        if (m_level < 1)
        {
            m_level = 1;
        }

        if (slotIcon == null) slotIcon = LoadIcon("Assets/Editor/LevelEditor/SlotIcon.png");
        if (chipIcon == null) chipIcon = LoadIcon("Assets/Editor/LevelEditor/ChipIcon.png");
        if (stoneIcon == null) stoneIcon = LoadIcon("Assets/Editor/LevelEditor/tile01.png");
        if (jamAIcon == null) jamAIcon = LoadIcon("Assets/Editor/LevelEditor/JamAIcon.png");
        if (blockIcon == null) blockIcon = LoadIcon("Assets/Editor/LevelEditor/BlockIcon.png");
        if (generatorIcon == null) generatorIcon = LoadIcon("Assets/Editor/LevelEditor/GeneratorIcon.png");
        if (teleportIcon == null) teleportIcon = LoadIcon("Assets/Editor/LevelEditor/TeleportIcon.png");
        if (teleOutIcon == null) teleOutIcon = LoadIcon("Assets/Editor/LevelEditor/portal_top01.png");
        if (keyOutIcon == null) keyOutIcon = LoadIcon("Assets/Editor/LevelEditor/key_outIcon.png");
        if (wallhIcon == null) wallhIcon = LoadIcon("Assets/Editor/LevelEditor/WallHIcon.png");
        if (wallvIcon == null) wallvIcon = LoadIcon("Assets/Editor/LevelEditor/WallVIcon.png");
        if (gravityIcon.Count == 0)
        {
            foreach (Side side in Utils.straightSides)
            {
                gravityIcon.Add(side, LoadIcon("Assets/Editor/LevelEditor/GravityIcon" + side.ToString() + ".png"));
            }
            gravityIcon.Add(Side.Null, LoadIcon("Assets/Editor/LevelEditor/GravityIcon" + Side.Null.ToString() + ".png"));
        }

        if (blockIcons.Count == 0)
        {
            blockIcons.Add("colorBook", LoadIcon("Assets/Editor/LevelEditor/colorBook.png"));
        }

        if (piecesIcons.Count == 0)
        {
            piecesIcons.Add("KE", LoadIcon("Assets/Editor/LevelEditor/key.png"));
        }

        if (conveyorIcons.Count == 0)
        {
            conveyorIcons.Add("spinner_ccw_00", LoadIcon("Assets/Editor/LevelEditor/spinner_ccw_00.png"));
            conveyorIcons.Add("spinner_ccw_01", LoadIcon("Assets/Editor/LevelEditor/spinner_ccw_01.png"));
            conveyorIcons.Add("spinner_ccw_02", LoadIcon("Assets/Editor/LevelEditor/spinner_ccw_02.png"));
            conveyorIcons.Add("spinner_ccw_03", LoadIcon("Assets/Editor/LevelEditor/spinner_ccw_03.png"));
            conveyorIcons.Add("spinner_cw_0", LoadIcon("Assets/Editor/LevelEditor/spinner_cw_0.png"));
            conveyorIcons.Add("spinner_cw_1", LoadIcon("Assets/Editor/LevelEditor/spinner_cw_1.png"));
            conveyorIcons.Add("spinner_cw_2", LoadIcon("Assets/Editor/LevelEditor/spinner_cw_2.png"));
            conveyorIcons.Add("spinner_cw_3", LoadIcon("Assets/Editor/LevelEditor/spinner_cw_3.png"));
            conveyorIcons.Add("conveyer_left_right", LoadIcon("Assets/Editor/LevelEditor/conveyer_left_right.png"));
            conveyorIcons.Add("conveyer_right_left", LoadIcon("Assets/Editor/LevelEditor/conveyer_right_left.png"));
            conveyorIcons.Add("conveyer_up_down", LoadIcon("Assets/Editor/LevelEditor/conveyer_up_down.png"));
            conveyorIcons.Add("conveyer_down_up", LoadIcon("Assets/Editor/LevelEditor/conveyer_down_up.png"));
            conveyorIcons.Add("conveyer_left_down", LoadIcon("Assets/Editor/LevelEditor/conveyer_left_down.png"));
            conveyorIcons.Add("conveyer_down_left", LoadIcon("Assets/Editor/LevelEditor/conveyer_down_left.png"));
            conveyorIcons.Add("conveyer_down_right", LoadIcon("Assets/Editor/LevelEditor/conveyer_down_right.png"));
            conveyorIcons.Add("conveyer_left_up", LoadIcon("Assets/Editor/LevelEditor/conveyer_left_up.png"));
            conveyorIcons.Add("conveyer_right_down", LoadIcon("Assets/Editor/LevelEditor/conveyer_right_down.png"));
            conveyorIcons.Add("conveyer_right_up", LoadIcon("Assets/Editor/LevelEditor/conveyer_right_up.png"));
            conveyorIcons.Add("conveyer_up_left", LoadIcon("Assets/Editor/LevelEditor/conveyer_up_left.png"));
            conveyorIcons.Add("conveyer_up_right", LoadIcon("Assets/Editor/LevelEditor/conveyer_up_right.png"));
        }

        chipColor = Chip.colors.Select(x => Color.Lerp(x, Color.white, 0.4f)).ToArray();

        if (layers.Count == 0)
        {
            layers.Add("Chips", true);
            layers.Add("BackLayer", true);
            layers.Add("Blocks", true);
            layers.Add("Generators", true);
            layers.Add("Teleports", true);
            if (k_use_gravity) layers.Add("Gravity", true);
            layers.Add("Conveyors", true);
        }

        if (chipInfos.Count == 0)
        {
            ChipInfosDefine chipInfosDefine = Resources.Load("Define/ChipInfosDefine") as ChipInfosDefine;
            if (chipInfosDefine)
            {
                chipInfos = chipInfosDefine.chipInfos.ToDictionary(x => x.name, x => x);
            }
        }
        if (blockInfos.Count == 0)
        {
            BlockInfosDefine blockInfosDefine = Resources.Load("Define/BlockInfosDefine") as BlockInfosDefine;
            if (blockInfosDefine)
            {
                blockInfos = blockInfosDefine.blockInfos.ToDictionary(x => x.name, x => x);
            }
        }

        SelectLevel(m_level);

        SlotUndo.Init();
    }

    public override UnityEngine.Object FindTarget()
    {
        return null;
    }

    bool SelectLevel(int v)
    {
        // if (v != m_level)
        {
            m_level = v;

            // load level from json
            TextAsset jsonText = Resources.Load("Levels/" + v) as TextAsset;
            if (jsonText == null)
            {
                Debug.Log("Asset for Level " + v + " not exist. Create new one.");
                profile = new LevelProfile();
                profile.level = m_level;
                ResetField();
                profile.slots = slots.Values.ToList();

                return false;
            }

            LoadLevelFromJson(jsonText);
        }

        return true;
    }

    void LoadLevelFromJson(TextAsset asset)
    {
        LevelConfig config = JsonUtility.FromJson<LevelConfig>(asset.text);
        m_lvConfig = config;

        profile = LevelConfig.ParseToProfile(config);
        profile.level = m_level;

        slots = profile.slots.ToDictionary(x => x.position, x => x);
        SlotUndo.Init();
    }

    void DrawSlotSettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.ExpandWidth(true), GUILayout.Height(170));

        if (selected.Count == 0)
        {
            GUILayout.Label("Nothing selected", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndVertical();
            return;
        }

        settingScroll = EditorGUILayout.BeginScrollView(settingScroll);

        #region Slots property
        DrawMixedProperty(
            mask: (Vector2Int coord) =>
            {
                return true;
            },
            getValue: (Vector2Int coord) => { return slots.ContainsKey(coord) && slots[coord].visible; },
            setValue: (Vector2Int coord, bool value) =>
            {
                if (value && !slots.ContainsKey(coord))
                {
                    NewSlotSettings(coord);
                }
                if (slots.ContainsKey(coord))
                {
                    if (slots[coord].visible != value)
                    {
                        SlotUndo.Record(slots[coord]);
                    }
                    slots[coord].visible = value;
                }
            },
            drawSingle: (bool value) => { return EditorGUILayout.Toggle("Visible", value); },
            drawMixed: (Action<bool> setDefault) =>
            {
                if (EditorGUILayout.Toggle("Visible", false))
                {
                    setDefault(true);
                    return true;
                }
                return false;
            });
        #endregion

        #region Generators property
        EditorGUILayout.BeginHorizontal();
        DrawMixedProperty(
            mask: (Vector2Int coord) =>
            {
                return slots.ContainsKey(coord) && slots[coord].visible;
            },
            getValue: (Vector2Int coord) => { return slots[coord].generator; },
            setValue: (Vector2Int coord, bool value) =>
            {
                if (slots[coord].generator != value)
                {
                    SlotUndo.Record(slots[coord]);
                }
                slots[coord].generator = value;
            },
            drawSingle: (bool value) =>
            {
                return EditorGUILayout.Toggle("Generator", value);
            },
            drawMixed: (Action<bool> setDefault) =>
            {
                if (EditorGUILayout.Toggle("Generator", false))
                {
                    setDefault(true);
                    return true;
                }
                return false;
            });
        DrawMixedProperty(
            mask: (Vector2Int coord) =>
            {
                return slots.ContainsKey(coord) && slots[coord].visible;
            },
            getValue: (Vector2Int coord) => { return slots[coord].generator && slots[coord].tags.Contains(GameConst.k_blockGenerator_tag); },
            setValue: (Vector2Int coord, bool value) =>
            {
                if (value && !slots[coord].tags.Contains(GameConst.k_blockGenerator_tag))
                {
                    SlotUndo.Record(slots[coord]);
                    slots[coord].tags.Add(GameConst.k_blockGenerator_tag);
                }
                else if (!value && slots[coord].tags.Contains(GameConst.k_blockGenerator_tag))
                {
                    SlotUndo.Record(slots[coord]);
                    slots[coord].tags.Remove(GameConst.k_blockGenerator_tag);
                }
            },
            drawSingle: (bool value) =>
            {
                return EditorGUILayout.Toggle("Generate blocker", value);
            },
            drawMixed: (Action<bool> setDefault) =>
            {
                if (EditorGUILayout.Toggle("Generate blocker", false))
                {
                    setDefault(true);
                    return true;
                }
                return false;
            });
        EditorGUILayout.EndHorizontal();
        #endregion

        #region Gravity property
        if (k_use_gravity)
        {
            Dictionary<int, string> gravity = Utils.straightSides.ToDictionary(x => (int)x, x => x.ToString());
            gravity.Add((int)Side.Null, Side.Null.ToString());
            DrawMixedProperty(
                mask: (Vector2Int coord) =>
                {
                    return slots.ContainsKey(coord) && slots[coord].visible;
                },
                getValue: (Vector2Int coord) => { return slots[coord].gravity; },
                setValue: (Vector2Int coord, Side value) =>
                {
                    if (slots[coord].gravity != value)
                    {
                        SlotUndo.Record(slots[coord]);
                    }
                    slots[coord].gravity = value;
                },
                drawSingle: (Side value) =>
                {
                    return (Side)EditorGUILayout.IntPopup("Gravity", (int)value, gravity.Values.ToArray(), gravity.Keys.ToArray());
                },
                drawMixed: (Action<Side> setDefault) =>
                {
                    int side = EditorGUILayout.IntPopup("Gravity", -1, gravity.Values.ToArray(), gravity.Keys.ToArray());
                    if (side != -1)
                    {
                        setDefault((Side)side);
                        return true;
                    }
                    return false;
                });
        }
        #endregion

        #region Chip property
        if (chipInfos.Count > 0)
        {
            List<string> chips = new List<string>();
            chips.Add("Empty");
            foreach (Session.ChipInfo chip in chipInfos.Values)
            {
                if (!chips.Contains(chip.name))
                {
                    chips.Add(chip.name);
                }
            }

            DrawMixedProperty(
                mask: (Vector2Int coord) =>
                {
                    return slots.ContainsKey(coord) && slots[coord].visible && (slots[coord].block_type == "" || blockInfos[slots[coord].block_type].chip);
                },
                getValue: (Vector2Int coord) => { return slots[coord].chip; },
                setValue: (Vector2Int coord, string value) =>
                {
                    if (slots[coord].chip != value)
                    {
                        SlotUndo.Record(slots[coord]);
                    }
                    slots[coord].chip = value;
                    if (!chipInfos.ContainsKey(value) || !chipInfos[value].color)
                        slots[coord].color_id = -1;
                },
                drawSingle: (string value) =>
                {
                    int id = chips.IndexOf(value);
                    if (id == -1) id = 0;
                    id = EditorGUILayout.Popup("Chip type", id, chips.ToArray());
                    return chips[id] == "Empty" ? "" : chips[id];

                },
                drawMixed: (Action<string> setDefault) =>
                {
                    int id = EditorGUILayout.Popup("Chip type", -1, chips.ToArray());
                    if (id != -1)
                    {
                        setDefault(chips[id] == "Empty" ? "" : chips[id]);
                        return true;
                    }
                    return false;
                });
        }
        #endregion

        #region Chip color property
        List<string> colors = new List<string>();
        colors.Add("Random");
        for (int i = 0; i < profile.colorCount; i++)
        {
            colors.Add(Chip.chipTypes[i]);
        }
        DrawMixedProperty(
            mask: (Vector2Int coord) =>
            {
                return slots.ContainsKey(coord) && slots[coord].visible && slots[coord].chip != "" && chipInfos[slots[coord].chip].color;
            },
            getValue: (Vector2Int coord) => { return slots[coord].color_id; },
            setValue: (Vector2Int coord, int value) =>
            {
                if (slots[coord].color_id != value)
                {
                    SlotUndo.Record(slots[coord]);
                }
                slots[coord].color_id = value;
            },
            drawSingle: (int value) =>
            {
                return EditorGUILayout.Popup("Color group", Mathf.Max(0, value), colors.ToArray());

            },
            drawMixed: (Action<int> setDefault) =>
            {
                int id = EditorGUILayout.Popup("Color group", -1, colors.ToArray());
                if (id != -1)
                {
                    setDefault(id);
                    return true;
                }
                return false;
            });
        #endregion

        #region Stone level property (BackLayer)
        DrawMixedProperty(
            mask: (Vector2Int coord) =>
            {
                return slots.ContainsKey(coord) && slots[coord].visible;
            },
            getValue: (Vector2Int coord) =>
            {
                return slots[coord].stone_level + 1;
            },
            setValue: (Vector2Int coord, int value) =>
            {
                if (slots[coord].stone_level != value - 1)
                {
                    SlotUndo.Record(slots[coord]);
                }
                slots[coord].stone_level = value - 1;
            },
            drawSingle: (int value) =>
            {
                return Mathf.RoundToInt(EditorGUILayout.Slider("Stone HP", value, 0, 3));

            },
            drawMixed: (Action<int> setDefault) =>
            {
                float level = EditorGUILayout.Slider("Stone HP", -1, 0, 3);
                if (level != -1)
                {
                    setDefault(Mathf.RoundToInt(level - 1));
                    return true;
                }
                return false;
            });
        #endregion

        #region Key drop out property (in levelJson: it's SinkerGoal)
        DrawMixedProperty(
            mask: (Vector2Int coord) =>
            {
                return slots.ContainsKey(coord) && slots[coord].visible;
            },
            getValue: (Vector2Int coord) =>
            {
                return slots[coord].tags.Contains(GameConst.k_sinkerGoal_tag);
            },
            setValue: (Vector2Int coord, bool value) =>
            {
                if (!value && slots[coord].tags.Contains(GameConst.k_sinkerGoal_tag))
                {
                    SlotUndo.Record(slots[coord]);
                    slots[coord].tags.Remove(GameConst.k_sinkerGoal_tag);
                }
                else if (value && !slots[coord].tags.Contains(GameConst.k_sinkerGoal_tag))
                {
                    SlotUndo.Record(slots[coord]);
                    slots[coord].tags.Add(GameConst.k_sinkerGoal_tag);
                }
            },
            drawSingle: (bool value) =>
            {
                return EditorGUILayout.Toggle("Key goal Slot", value);
            },
            drawMixed: (Action<bool> setDefault) =>
            {
                if (EditorGUILayout.Toggle("Key goal Slot", false))
                {
                    setDefault(true);
                    return true;
                }
                return false;
            });
        #endregion

        #region Block property
        {
            Dictionary<string, Session.BlockInfo> blocks = new Dictionary<string, Session.BlockInfo>();
            blocks.Add("Empty", null);
            foreach (Session.BlockInfo block in blockInfos.Values)
            {
                if (!blocks.ContainsKey(block.name))
                {
                    blocks.Add(block.name, block);
                }
            }
            List<string> block_keys = new List<string>(blocks.Keys);

            #region Block type
            DrawMixedProperty(
                mask: (Vector2Int coord) =>
                {
                    return slots.ContainsKey(coord) && slots[coord].visible;
                },
                getValue: (Vector2Int coord) =>
                {
                    return slots[coord].block_type;
                },
                setValue: (Vector2Int coord, string value) =>
                {
                    if (slots[coord].block_type != value)
                    {
                        SlotUndo.Record(slots[coord]);
                    }
                    slots[coord].block_type = value;
                    if (value != "" && !blockInfos[value].chip)
                        slots[coord].chip = "";
                    if (slots[coord].block_type == "scroll" && slots[coord].scrollInfo == null)
                    {
                        slots[coord].scrollInfo = new MagicScrollInfo();
                    }
                    else if (slots[coord].block_type == "")
                    {
                        slots[coord].scrollInfo = null;
                    }
                },
                drawSingle: (string value) =>
                {
                    int id = block_keys.IndexOf(value);
                    if (id == -1)
                        id = 0;
                    id = EditorGUILayout.Popup("Block type", id, block_keys.ToArray());
                    return block_keys[id] == "Empty" ? "" : block_keys[id];

                },
                drawMixed: (Action<string> setDefault) =>
                {
                    int id = EditorGUILayout.Popup("Block type", -1, block_keys.ToArray());
                    if (id != -1)
                    {
                        setDefault(block_keys[id] == "Empty" ? "" : block_keys[id]);
                        return true;
                    }
                    return false;
                });
            #endregion

            #region Block level
            int max = 1000;
            DrawMixedProperty(
                mask: (Vector2Int coord) =>
                {
                    if (!slots[coord].visible) return false;
                    if (!slots.ContainsKey(coord) || slots[coord].block_type == "")
                    { return false; }

                    // don't need show level of block has max level == 1
                    if (blocks[slots[coord].block_type].levelCount == 1) return false;
                    max = Mathf.Min(max, blocks[slots[coord].block_type].levelCount);
                    return true;
                },
                getValue: (Vector2Int coord) =>
                {
                    return slots[coord].block_level;
                },
                setValue: (Vector2Int coord, int value) =>
                {
                    if (slots[coord].block_level != value)
                    {
                        SlotUndo.Record(slots[coord]);
                    }
                    slots[coord].block_level = value;
                },
                drawSingle: (int value) =>
                {
                    return Mathf.RoundToInt(EditorGUILayout.Slider("Block level", value, 1, max));

                },
                drawMixed: (Action<int> setDefault) =>
                {
                    float level = EditorGUILayout.Slider("Block level", -1, -1, max);
                    if (level != -1)
                    {
                        setDefault(Mathf.RoundToInt(level));
                        return true;
                    }
                    return false;
                });
            #endregion
            #region Block color property
            DrawMixedProperty(
                mask: (Vector2Int coord) =>
                {
                    return slots.ContainsKey(coord) && slots[coord].visible && slots[coord].block_type != "" && blockInfos[slots[coord].block_type].color;
                },
                getValue: (Vector2Int coord) => { return slots[coord].color_id; },
                setValue: (Vector2Int coord, int value) =>
                {
                    if (slots[coord].color_id != value)
                    {
                        SlotUndo.Record(slots[coord]);
                    }
                    slots[coord].color_id = value;
                },
                drawSingle: (int value) =>
                {
                    return EditorGUILayout.Popup("Block color", Mathf.Max(0, value), colors.ToArray());

                },
                drawMixed: (Action<int> setDefault) =>
                {
                    int id = EditorGUILayout.Popup("Block color", -1, colors.ToArray());
                    if (id != -1)
                    {
                        setDefault(id);
                        return true;
                    }
                    return false;
                });
            #endregion
            #region Block Panel Info property
            DrawMixedProperty(
                mask: (Vector2Int coord) =>
                {
                    return slots.ContainsKey(coord) && slots[coord].visible
                        && (slots[coord].block_type == LevelConfig.GetBlockTypeFrom(ETile.BookShelf)
                            || slots[coord].block_type == LevelConfig.GetBlockTypeFrom(ETile.MagicTap)
                            || slots[coord].block_type == LevelConfig.GetBlockTypeFrom(ETile.CandyTree))
                        ;
                },
                getValue: (Vector2Int coord) => { return slots[coord].bookShelfInfo == null ? 0 : slots[coord].bookShelfInfo.Index; },
                setValue: (Vector2Int coord, int value) =>
                {
                    if (slots[coord].bookShelfInfo == null)
                    {
                        slots[coord].bookShelfInfo = new BookShelfInfo();
                    }
                    slots[coord].bookShelfInfo.Index = value;
                },
                drawSingle: (int value) =>
                {
                    return EditorGUILayout.IntSlider("Panel index:", value, 0, GameConst.k_blocker_group_slot_count - 1, GUILayout.Width(300));
                },
                drawMixed: (Action<int> setDefault) =>
                {
                    int id = EditorGUILayout.IntSlider("Panel index:", -1, 0, GameConst.k_blocker_group_slot_count - 1);
                    if (id != -1)
                    {
                        setDefault(id);
                        return true;
                    }
                    return false;
                });
            #endregion

            #region Scroll block dir
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            DrawMixedProperty<EScrollType>(
                mask: (Vector2Int coord) =>
                {
                    return slots.ContainsKey(coord) && slots[coord].visible && slots[coord].block_type == "scroll";
                },
                getValue: (Vector2Int coord) =>
                {
                    return (EScrollType)(Enum.Parse(typeof(EScrollType), slots[coord].scrollInfo.Type));
                },
                setValue: (Vector2Int coord, EScrollType value) =>
                {
                    if (slots[coord].scrollInfo.Type != value.ToString())
                    {
                        SlotUndo.Record(slots[coord]);
                    }
                    slots[coord].scrollInfo.Type = value.ToString();
                },
                drawSingle: (EScrollType value) =>
                {
                    return (EScrollType)EditorGUILayout.EnumPopup("Scroll Type", value);

                },
                drawMixed: (Action<EScrollType> setDefault) =>
                {
                    EScrollType id = (EScrollType)EditorGUILayout.EnumPopup("Scroll Type", (EScrollType)1);
                    // if (id != -1)
                    {
                        setDefault((EScrollType)id);
                        return true;
                    }
                    // return false;
                });
            EditorGUILayout.Space();
            DrawMixedProperty<EScrollDir>(
                mask: (Vector2Int coord) =>
                {
                    return slots.ContainsKey(coord) && slots[coord].visible && slots[coord].block_type == "scroll";
                },
                getValue: (Vector2Int coord) =>
                {
                    return (EScrollDir)(Enum.Parse(typeof(EScrollDir), slots[coord].scrollInfo.Dir));
                },
                setValue: (Vector2Int coord, EScrollDir value) =>
                {
                    if (slots[coord].scrollInfo.Dir != value.ToString())
                    {
                        SlotUndo.Record(slots[coord]);
                    }
                    slots[coord].scrollInfo.Dir = value.ToString();
                },
                drawSingle: (EScrollDir value) =>
                {
                    return (EScrollDir)EditorGUILayout.EnumPopup("Scroll dir", value);

                },
                drawMixed: (Action<EScrollDir> setDefault) =>
                {
                    EScrollDir id = (EScrollDir)EditorGUILayout.EnumPopup("Scroll dir", (EScrollDir)1);
                    // if (id != -1)
                    {
                        setDefault((EScrollDir)id);
                        return true;
                    }
                    // return false;
                });
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            #endregion

            #region Switcher panel info
            if (selected.Count == 1)
            {
                DrawMixedProperty(
                   mask: (Vector2Int coord) =>
                   {
                       return slots.ContainsKey(coord) && slots[coord].visible
                           && (slots[coord].block_type == LevelConfig.GetBlockTypeFrom(ETile.Switcher));
                   },
                   getValue: (Vector2Int coord) => { return slots[coord].switcherInfo; },
                   setValue: (Vector2Int coord, SwitcherInfo value) =>
                   {
                       slots[coord].switcherInfo = value;
                   },
                   drawSingle: (SwitcherInfo value) =>
                   {
                       SwitcherInfo info = value;
                       if (info == null)
                       {
                           info = new SwitcherInfo();
                       }
                       info.GroupIndex = EditorGUILayout.IntSlider("Group index:", info.GroupIndex, 1, Chip.colors.Length, GUILayout.Width(300));
                       info.Index = EditorGUILayout.IntSlider("Index:", info.Index, 1, 99, GUILayout.Width(300));
                       return info;
                   },
                   drawMixed: (Action<SwitcherInfo> setDefault) =>
                   {
                       EditorGUILayout.HelpBox("Switcher doesn't suppor multi select", MessageType.Info);
                       return false;
                   });
            }
            #endregion

        }
        #endregion

        #region Teleport property
        DrawMixedProperty(
            mask: (Vector2Int coord) =>
            {
                return slots.ContainsKey(coord) && slots[coord].visible;
            },
            getValue: (Vector2Int coord) =>
            {
                return slots[coord].teleport;
            },
            setValue: (Vector2Int coord, Vector2Int value) =>
            {
                if (slots[coord].teleport != value)
                {
                    SlotUndo.Record(slots[coord]);
                }
                if (coord == value || value == null)
                    slots[coord].teleport = Utils.Vector2IntNull;
                else
                    slots[coord].teleport = value;
            },
            drawSingle: (Vector2Int value) =>
            {
                EditorGUILayout.BeginHorizontal();
                defaultColor = GUI.color;
                if (wait_target)
                    GUI.color = Color.cyan;
                GUILayout.Label("Teleport", GUILayout.ExpandWidth(true));
                Vector2Int result = value;
                if (GUILayout.Button(value == Utils.Vector2IntNull ? "None" : value.ToString(), EditorStyles.miniButton, GUILayout.Width(60)))
                {
                    wait_target = true;
                    target_selection = null;
                }
                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    wait_target = false;
                    target_selection = null;
                    result = Utils.Vector2IntNull;
                }
                EditorGUILayout.EndHorizontal();
                if (wait_target && target_selection != null)
                {
                    wait_target = false;
                    result = target_selection.position;
                }
                GUI.color = defaultColor;
                return result;
            },
            drawMixed: (Action<Vector2Int> setDefault) =>
            {
                EditorGUILayout.BeginHorizontal();
                defaultColor = GUI.color;
                if (wait_target)
                    GUI.color = Color.cyan;
                GUILayout.Label("Teleport", GUILayout.ExpandWidth(true));
                bool result = false;
                if (GUILayout.Button("-", GUILayout.Width(60)))
                {
                    wait_target = true;
                    target_selection = null;
                }
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    wait_target = false;
                    target_selection = null;
                    result = true;
                    setDefault(Utils.Vector2IntNull);
                }
                EditorGUILayout.EndHorizontal();
                if (wait_target && target_selection != null)
                {
                    wait_target = false;
                    result = true;
                    setDefault(target_selection.position);
                }
                GUI.color = defaultColor;
                return result;
            });
        #endregion

        #region Butterfly generate property (in levelJson: it's ClimberSpawn)
        DrawMixedProperty(
            mask: (Vector2Int coord) =>
            {
                return slots.ContainsKey(coord) && slots[coord].visible;
            },
            getValue: (Vector2Int coord) =>
            {
                return slots[coord].tags.Contains(GameConst.k_climberGenerator_tag);
            },
            setValue: (Vector2Int coord, bool value) =>
            {
                if (!value && slots[coord].tags.Contains(GameConst.k_climberGenerator_tag))
                {
                    SlotUndo.Record(slots[coord]);
                    slots[coord].tags.Remove(GameConst.k_climberGenerator_tag);
                }
                else if (value && !slots[coord].tags.Contains(GameConst.k_climberGenerator_tag))
                {
                    SlotUndo.Record(slots[coord]);
                    slots[coord].tags.Add(GameConst.k_climberGenerator_tag);
                }
            },
            drawSingle: (bool value) =>
            {
                return EditorGUILayout.Toggle("Climber Generator (BF)", value);
            },
            drawMixed: (Action<bool> setDefault) =>
            {
                if (EditorGUILayout.Toggle("Climber Generator (BF)", false))
                {
                    setDefault(true);
                    return true;
                }
                return false;
            });
        #endregion

        #region Conveyor
        if (selected.Count == 1)
        {
            DrawMixedProperty(
               mask: (Vector2Int coord) =>
               {
                   return slots.ContainsKey(coord) && slots[coord].visible;
               },
               getValue: (Vector2Int coord) =>
               {
                   return slots[coord].conveyorInfo;
               },
               setValue: (Vector2Int coord, ConveyorInfo value) =>
               {
                   if (slots[coord].conveyorInfo != value)
                   {
                       SlotUndo.Record(slots[coord]);
                   }
                   slots[coord].conveyorInfo = value;
               },
               drawSingle: (ConveyorInfo value) =>
               {
                   ConveyorInfo info = value;
                   bool enableConveyor = info != null;
                   enableConveyor = EditorGUILayout.Toggle("Enable Conveyor:", enableConveyor, EditorStyles.toggleGroup);
                   if (enableConveyor)
                   {
                       if (info == null)
                       {
                           info = new ConveyorInfo();
                       }
                       EditorGUILayout.BeginHorizontal();
                       GUILayout.Space(20);
                       EditorGUILayout.BeginVertical();
                       {
                           //conveyor Type
                           EditorGUILayout.BeginHorizontal();
                           EditorGUILayout.LabelField("Type", GUILayout.Width(60));
                           EConveyorType type = info.GetConveyorType();
                           type = (EConveyorType)EditorGUILayout.EnumPopup(type, GUILayout.Width(150));
                           info.SetConveyorType(type);

                           EditorGUILayout.Separator();
                           EditorGUILayout.LabelField("Image", GUILayout.Width(60));
                           EConveyorImg imgType = info.GetConveyorImg();
                           imgType = (EConveyorImg)EditorGUILayout.EnumPopup(imgType, GUILayout.Width(150));
                           info.SetConveyorImg(imgType);
                           EditorGUILayout.EndHorizontal();
                           EditorGUILayout.Separator();
                           //Conveyor DIR in, out
                           EditorGUILayout.BeginHorizontal();
                           EditorGUILayout.LabelField("IN", GUILayout.Width(60));
                           EScrollDir inDir = info.GetDirIn();
                           inDir = (EScrollDir)EditorGUILayout.EnumPopup(inDir, GUILayout.Width(150));
                           info.SetDirIn(inDir);
                           EditorGUILayout.Separator();

                           EditorGUILayout.LabelField("OUT", GUILayout.Width(60));
                           EScrollDir outDir = info.GetDirOut();
                           outDir = (EScrollDir)EditorGUILayout.EnumPopup(outDir, GUILayout.Width(150));
                           EditorGUILayout.EndHorizontal();
                           if (info.CheckOutDirValid(outDir))
                           {
                               info.SetDirOut(outDir);
                           }
                           else
                           {
                               EditorGUILayout.HelpBox("IN and OUT is invalid", MessageType.Error);
                           }
                       }
                       EditorGUILayout.EndVertical();
                       EditorGUILayout.EndHorizontal();
                   }
                   else
                   {
                       info = null;
                   }
                   return info;

               },
               drawMixed: (Action<ConveyorInfo> setDefault) =>
               {
                   EditorGUILayout.HelpBox("Conveyor doesn't suppor multi select", MessageType.Info);
                   return false;
               });
        }
        else
        {
            EditorGUILayout.HelpBox("Conveyor doesn't suppor multi select", MessageType.Info);
        }
        #endregion

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    void DrawMixedProperty<T>(Func<Vector2Int, bool> mask, Func<Vector2Int, T> getValue, Action<Vector2Int, T> setValue,
        Func<T, T> drawSingle, Func<Action<T>, bool> drawMixed, Action drawEmpty = null)
    {
        bool multiple = false; // is selected list have multiple different values
        bool assigned = false;
        T value = default(T);
        T temp;
        foreach (Vector2Int coord in selected)
        {
            if (!mask.Invoke(coord))
                continue;
            if (!assigned)
            {
                value = getValue.Invoke(coord);
                assigned = true;
                continue;
            }
            temp = getValue.Invoke(coord);
            if (!value.Equals(temp))
            {
                multiple = true;
                break;
            }
        }

        if (!assigned)
        {
            if (drawEmpty != null)
            {
                drawEmpty.Invoke();
            }
            return;
        }

        if (multiple)
        {
            EditorGUI.showMixedValue = true;
            Action<T> setDefault = (T t) =>
            {
                value = t;
            };
            if (drawMixed.Invoke(setDefault))
            {
                multiple = false;
            }
            EditorGUI.showMixedValue = false;
        }
        else
        {
            value = drawSingle(value);
        }

        if (!multiple)
        {
            foreach (Vector2Int coord in selected)
            {
                if (mask.Invoke(coord))
                {
                    setValue(coord, value);
                }
            }
        }
    }

    void DrawLayersSettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.Width(100), GUILayout.Height(170));

        GUILayout.Label("Layers", EditorStyles.centeredGreyMiniLabel);
        foreach (string layer in layers.Keys.ToArray())
        {
            layers[layer] = GUILayout.Toggle(layers[layer], layer);
        }
        EditorGUILayout.EndVertical();
    }

    SlotSettings NewSlotSettings(Vector2Int coord, bool force = false)
    {
        bool exist = slots.ContainsKey(coord);
        if (!exist || force)
        {
            if (exist)
                slots[coord] = new SlotSettings(coord);
            else
                slots.Add(coord, new SlotSettings(coord));
            if (coord.y == profile.height - 1)
            {
                slots[coord].generator = true;
            }
            if (coord.y == 0)
            {
                slots[coord].tags.Add(GameConst.k_sinkerGoal_tag);
            }
            return slots[coord];
        }
        return null;
    }

    void DrawFieldView()
    {
        // draw row, col legend
        defaultColor = GUI.color;
        GUI.color = Color.gray;
        for (int x = 0; x < LevelProfile.maxSize; x++)
        {
            GUI.Box(new Rect(rect.xMin + x * (cellSize + slotOffset) + legendSize,
                           rect.yMin + (0) * (cellSize + slotOffset), cellSize, legendSize), x.ToString(), EditorStyles.centeredGreyMiniLabel);
        }
        rect.y += legendSize;
        for (int y = 0; y < LevelProfile.maxSize; y++)
        {
            GUI.Box(new Rect(rect.xMin, rect.yMin + (LevelProfile.maxSize - y - 1) * (cellSize + slotOffset) + slotOffset,
               legendSize, cellSize), (y).ToString(), EditorStyles.centeredGreyMiniLabel);
        }

        GUI.color = defaultColor;

        Vector2Int key;
        for (int x = 0; x < LevelProfile.maxSize; x++)
        {
            for (int y = 0; y < LevelProfile.maxSize; y++)
            {
                key = new Vector2Int(x, y);
                if (DrawSlotButton(key, rect))
                {
                    if (wait_target)
                    {
                        if (slots.ContainsKey(key))
                        {
                            target_selection = slots[key];
                            continue;
                        }
                        else
                        {
                            wait_target = false;
                        }
                    }

                    if (Event.current.shift && selected.Count > 0)
                    {
                        Vector2Int start = new Vector2Int(selected.Last().x, selected.Last().y);
                        Vector2Int delta = new Vector2Int();
                        delta.x = start.x < x ? 1 : -1;
                        delta.y = start.y < y ? 1 : -1;
                        Vector2Int cursor = new Vector2Int();
                        for (cursor.x = start.x; cursor.x != x + delta.x; cursor.x += delta.x)
                        {
                            for (cursor.y = start.y; cursor.y != y + delta.y; cursor.y += delta.y)
                            {
                                if (!selected.Contains(cursor))
                                {
                                    Vector2Int coord = new Vector2Int(cursor.x, cursor.y);
                                    selected.Add(coord);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!Event.current.control)
                            selected.Clear();
                        if (selected.Contains(key))
                            selected.Remove(key);
                        else
                            selected.Add(key);
                    }
                }
            }
        }

        GUI.color = defaultColor;

    }

    void DrawActionBar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

        if (GUILayout.Button("Select all visible", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            selected.Clear();
            for (int x = 0; x < LevelProfile.maxSize; x++)
            {
                for (int y = 0; y < LevelProfile.maxSize; y++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    if (slots.ContainsKey(coord) && slots[coord].visible)
                    {
                        selected.Add(coord);
                    }
                }
            }
        }

        if (GUILayout.Button("Select all", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            selected = new List<Vector2Int>(slots.Keys);

        }

        if (GUILayout.Button("Unselect all", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            selected.Clear();
        }

        if (selected.Count > 0)
        {
            if (GUILayout.Button("Delete Cell(s)", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                DeleteCells();
            }
        }

        GUILayout.FlexibleSpace();

        defaultColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.Lerp(defaultColor, Color.blue, 0.4f);
        if (!EditorApplication.isPlayingOrWillChangePlaymode && GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            SaveFile();
        }
        GUI.backgroundColor = Color.Lerp(defaultColor, Color.green, 0.6f);
        if (!EditorApplication.isPlayingOrWillChangePlaymode && GUILayout.Button("Test", EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            RunLevel();
        }

        GUI.backgroundColor = Color.Lerp(defaultColor, Color.red, 0.6f);
        if (GUILayout.Button("Reset", EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            ResetField();
        }
        GUI.backgroundColor = defaultColor;

        EditorGUILayout.EndHorizontal();
    }

    void DeleteCells()
    {
        foreach (var coord in selected)
        {
            SlotUndo.Record(slots[coord]);
            var slSetting = NewSlotSettings(coord, true);
            if (slSetting != null)
            {
                slSetting.visible = false;
            }
        }
    }

    void RunLevel()
    {
        SaveFile();

        EditorApplication.isPlaying = true;
        PlayerPrefs.SetInt("TestLevel", m_level);
    }

    void ResetField()
    {
        slots.Clear();
        for (int x = 0; x < LevelProfile.maxSize; x++)
        {
            for (int y = 0; y < LevelProfile.maxSize; y++)
            {
                NewSlotSettings(new Vector2Int(x, y));
            }
        }
    }
    bool DrawSlotButton(Vector2Int coord, Rect r)
    {
        bool btn = false;
        defaultColor = GUI.color;

        Rect rect = new Rect(r.xMin + coord.x * (cellSize + slotOffset) + legendSize,
            r.yMin + (/* profile.height */LevelProfile.maxSize - coord.y - 1) * (cellSize + slotOffset) + slotOffset,
            cellSize, cellSize);

        if (slots.ContainsKey(coord) && slots[coord].visible)
        {
            bool showHighlight = wait_target;
            if (!showHighlight)
            {
                foreach (var slCoord in selected)
                {
                    if (slots[slCoord].teleport == coord)
                    {
                        showHighlight = true;
                        break;
                    }
                }
            }
            GUI.color = (wait_target || showHighlight) ? Color.cyan : Color.gray;
        }
        else
            GUI.color *= new Color(0, 0, 0, 0.1f);

        GUI.DrawTexture(rect, slotIcon);
        btn = GUI.Button(rect, "", labelStyle);
        GUI.color = defaultColor;

        // Draw backlayer
        if (layers["BackLayer"] && slots.ContainsKey(coord) && slots[coord].stone_level >= 0)
        {
            defaultColor = GUI.color;
            float a = (1f * slots[coord].stone_level + 1) / 3;
            Color cl = Color.Lerp(Color.white, Color.grey, a);
            GUI.color = cl;

            GUI.DrawTexture(rect, stoneIcon);
            GUI.color = defaultColor;
        }

        if (layers["Chips"] && slots.ContainsKey(coord) && slots[coord].visible && slots[coord].chip != "")
        {
            defaultColor = GUI.color;
            if (slots[coord].color_id > profile.colorCount)
            {
                slots[coord].color_id = 0;
            }
            int color_id = slots[coord].color_id;
            GUI.color = color_id > 0 && color_id <= profile.colorCount ? chipColor[color_id - 1] : Color.white;
            String shortName = chipInfos[slots[coord].chip].shirtName;
            Texture icon = piecesIcons.ContainsKey(shortName) ? piecesIcons[shortName] : chipIcon;
            GUI.DrawTexture(rect, icon);
            GUI.Box(rect, shortName, labelStyle);
            GUI.color = defaultColor;
        }

        // Draw block
        if (layers["Blocks"] && slots.ContainsKey(coord) && slots[coord].visible && slots[coord].block_type != "")
        {

            Session.BlockInfo info = blockInfos[slots[coord].block_type];
            if (info.color)
            {
                defaultColor = GUI.color;
                int color_id = slots[coord].color_id;
                GUI.color = color_id > 0 && color_id <= profile.colorCount ? chipColor[color_id - 1] : Color.white;
                Rect iconRect = new Rect(rect.x + rect.width / 4, rect.y, rect.width / 2, rect.height / 2);
                if (blockIcons.ContainsKey(slots[coord].block_type))
                {
                    GUI.DrawTexture(iconRect, blockIcons[slots[coord].block_type]);
                }
                else
                {
                    GUI.DrawTexture(iconRect, chipIcon);
                }
                GUI.color = defaultColor;
            }
            GUI.DrawTexture(rect, blockIcon);
            string name = info.shirtName + (info.levelCount > 1 ? (":" + (slots[coord].block_level).ToString()) : "");
            if (slots[coord].block_type == "scroll")
            {
                switch (slots[coord].scrollInfo.Dir)
                {
                    case "UP": name += " ^"; break;
                    case "DOWN": name += " v"; break;
                    case "LEFT": name += " <"; break;
                    case "RIGHT": name += " >"; break;
                }
            }
            else if (slots[coord].block_type == LevelConfig.GetBlockTypeFrom(ETile.BookShelf)
              || slots[coord].block_type == LevelConfig.GetBlockTypeFrom(ETile.MagicTap)
              || slots[coord].block_type == LevelConfig.GetBlockTypeFrom(ETile.CandyTree)
              )
            {
                name += "_" + slots[coord].bookShelfInfo.Index;
            }
            else if (slots[coord].block_type == LevelConfig.GetBlockTypeFrom(ETile.Switcher))
            {
                if (slots[coord].switcherInfo != null)
                    name += "_" + slots[coord].switcherInfo.GroupIndex + "_" + slots[coord].switcherInfo.Index;
            }
            GUI.Box(new Rect(rect.x, rect.y + rect.height / 2, rect.width, rect.height / 2),
                name, labelStyle);
        }

        if (layers["Conveyors"] && slots.ContainsKey(coord) && slots[coord].visible)
        {
            ConveyorInfo info = slots[coord].conveyorInfo;
            if (info != null)
            {
                EConveyorImg imgType = info.GetConveyorImg();
                EScrollDir inDir = info.GetDirIn();
                EScrollDir outDir = info.GetDirOut();
                string imgName = string.Empty;
                if (imgType == EConveyorImg.PLATE_CLOCKWISE)
                {
                    if (inDir == EScrollDir.DOWN)
                    {
                        imgName = "spinner_cw_1";
                    }
                    else if (inDir == EScrollDir.LEFT)
                    {
                        imgName = "spinner_cw_0";
                    }
                    else if (inDir == EScrollDir.UP)
                    {
                        imgName = "spinner_cw_3";
                    }
                    else if (inDir == EScrollDir.RIGHT)
                    {
                        imgName = "spinner_cw_2";
                    }
                }
                else if (imgType == EConveyorImg.PLATE_COUNTERCLOCKWISE)
                {
                    if (inDir == EScrollDir.DOWN)
                    {
                        imgName = "spinner_ccw_00";
                    }
                    else if (inDir == EScrollDir.RIGHT)
                    {
                        imgName = "spinner_ccw_01";
                    }
                    else if (inDir == EScrollDir.UP)
                    {
                        imgName = "spinner_ccw_02";
                    }
                    else if (inDir == EScrollDir.LEFT)
                    {
                        imgName = "spinner_ccw_03";
                    }
                }
                else if (imgType == EConveyorImg.DEFAULT)
                {
                    imgName = ("conveyer_" + inDir.ToString() + "_" + outDir.ToString()).ToLower();
                }
                if (!string.IsNullOrEmpty(imgName) && conveyorIcons.ContainsKey(imgName))
                {
                    defaultColor = GUI.color;
                    GUI.color = new Color(1, 1, 1, 0.5f);
                    GUI.DrawTexture(rect, conveyorIcons[imgName]);
                    GUI.color = defaultColor;
                }
            }
        }

        if (slots.ContainsKey(coord) && slots[coord].visible)
        {
            Rect label_rect = new Rect(rect);
            label_rect.width = 10;
            label_rect.height = 10;
            label_rect.x += rect.width - label_rect.width;
            if (k_use_gravity)
            {
                if (layers["Gravity"])
                {
                    defaultColor = GUI.color;
                    GUI.color = Color.yellow;
                    GUI.DrawTexture(label_rect, gravityIcon[slots[coord].gravity]);
                    label_rect.y += label_rect.height;
                    GUI.color = defaultColor;
                }
            }

            if (layers["Generators"])
            {
                if (slots[coord].generator && !teleportTargets.Contains(coord))
                {
                    GUI.DrawTexture(label_rect, generatorIcon);
                    label_rect.y += label_rect.height;
                }

                if (slots[coord].tags.Contains(GameConst.k_sinkerGoal_tag))
                {
                    GUI.DrawTexture(label_rect, keyOutIcon);
                    label_rect.y += label_rect.height;
                }

                if (slots[coord].tags.Contains(GameConst.k_climberGenerator_tag))
                {
                    defaultColor = GUI.color;
                    GUI.color = Color.Lerp(Color.white, Color.magenta, 0.6f);
                    GUI.DrawTexture(label_rect, generatorIcon);
                    label_rect.y += label_rect.height;
                    GUI.color = defaultColor;
                }
            }

            if (layers["Teleports"])
            {
                if (slots[coord].teleport != Utils.Vector2IntNull)
                {
                    if (slots.ContainsKey(slots[coord].teleport))
                    {
                        defaultColor = GUI.color;
                        GUI.color = Color.cyan;
                        GUI.DrawTexture(label_rect, teleportIcon);
                        label_rect.y += label_rect.height;
                        GUI.color = defaultColor;
                    }
                    else
                    {
                        slots[coord].teleport = Utils.Vector2IntNull;
                    }
                }
                if (teleportTargets.Contains(coord))
                {
                    GUI.DrawTexture(label_rect, teleOutIcon);
                }
            }
        }

        // toggle when slot is selected
        if (selected.Contains(coord))
        {
            GUI.Toggle(new Rect(rect.xMin, rect.yMin, 10, 10), true, "");
        }

        GUI.backgroundColor = defaultColor;

        return btn;
    }

    ////////////////////////////////////////////////////////////////////////////////
    void SaveFile()
    {
        if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
        {
            profile.slots = slots.Values.ToList();
            m_lvConfig = LevelConfig.ParseFromProfile(profile);

            string saveString = JsonUtility.ToJson(m_lvConfig, prettyPrint: false);

            //Write to file
            string activeDir = Application.dataPath + @"/Resources/Levels/";
            if (!Directory.Exists(activeDir))
            {
                Directory.CreateDirectory(activeDir);
            }
            string newPath = System.IO.Path.Combine(activeDir, m_level + ".txt");
            StreamWriter sw = new StreamWriter(newPath);
            sw.Write(saveString);
            sw.Close();

            if (profile.allTargets.Count == 0)
            {
                EditorUtility.DisplayDialog("Warning", "This level has no target.", "OK");
            }
        }
        AssetDatabase.Refresh();
    }

    int GetLastFileIndex()
    {
        string dirPath = "Assets/Resources/Levels";

        DirectoryInfo d = new DirectoryInfo(dirPath);
        FileInfo[] files = d.GetFiles("*.txt", SearchOption.AllDirectories); //Getting files

        int max = 1;
        foreach (var file in files)
        {
            string fileName = file.Name.Substring(0, file.Name.IndexOf(file.Extension));
            int id = 0;
            if (int.TryParse(fileName, out id))
            {
                if (id > max)
                {
                    max = id;
                }
            }
        }
        return max;
    }
}