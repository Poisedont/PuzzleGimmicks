using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Session : Singleton<Session>
{
    #region Variables
    [HideInInspector] public bool squareCombination = true;

    [SerializeField] ChipInfosDefine chipInfosDefine;
    [SerializeField] BlockInfosDefine blockInfosDefine;
    [SerializeField] CombinationsDefine combinationsDefine;
    [SerializeField] MixDefine mixDefine;
    [HideInInspector] public List<ChipInfo> chipInfos = new List<ChipInfo>();
    [HideInInspector] public List<BlockInfo> blockInfos = new List<BlockInfo>();
    List<Combinations> combinations = new List<Combinations>();
    List<Mix> mixes = new List<Mix>();
    List<Solution> solutions = new List<Solution>();

    [Header("Internal variables")]
    public bool iteraction = false;
    public int lastMovementId;
    public int movesCount; // Number of remaining moves
    public int swapEvent; // After each successed swap this parameter grows by 1 
    public float timeLeft; // Number of remaining time
    public int eventCount; // Event counter
    public int score = 0; // Current score
    public bool isPlaying = false;
    public bool outOfLimit = false;
    public bool reachedTheTarget = false;

    public bool gameForceOver = false;
    public bool firstChipGeneration = false;
    public int matchCount = 0;
    public int stars;
    bool swaping = false; // TRUE when the animation plays swapping 2 chips

    bool targetRoutineIsOver = false;
    bool limitationRoutineIsOver = false;

    bool wait = false;

    public int[] colorMask = new int[6]; // Mask of random colors: color number - colorID
    public int creatingDropsCount;
    public UnityAction PostSwapAction;
    #endregion
    ////////////////////////////////////////////////////////////////////////////////
    private void Start()
    {
        // load info
        if (chipInfosDefine)
        {
            chipInfos = chipInfosDefine.chipInfos;
        }

        if (combinationsDefine)
        {
            combinations = combinationsDefine.combinations;
        }

        if (mixDefine)
        {
            mixes = mixDefine.mixes;
        }
        // sort combine define by priority
        combinations.Sort((Combinations a, Combinations b) =>
        {
            if (a.priority < b.priority)
                return -1;
            if (a.priority > b.priority)
                return 1;
            return 0;
        });

        if (blockInfosDefine)
        {
            blockInfos = blockInfosDefine.blockInfos;
        }
        QualitySettings.vSyncCount = 1;
    }

    private void Update()
    {
        // Debug.Log("Busy " + Chip.busyList.Count);
        // Debug.Log("Can I Wait " + CanIWait());
    }
    #region Inner classes
    [System.Serializable]
    public class ChipInfo
    {
        public string name = "";
        public string contentName = "";
        public bool color = true;
        public string shirtName = "";
    }

    [System.Serializable]
    public class BlockInfo
    {
        public string name = "";
        public string contentName = ""; // name of prefab content
        public string shirtName = ""; //short name
        public int levelCount = 0; // max level of block
        public bool chip = false; //contain chip or not
        public bool color = false; //block has color require
    }

    [System.Serializable]
    public class Mix
    {
        public Pair pair = new Pair("", "");

        public string function;

        public bool Compare(string _a, string _b)
        {
            return pair == new Pair(_a, _b);
        }

        public static bool ContainsThisMix(string _a, string _b)
        {
            return Instance.mixes.Find(x => x.Compare(_a, _b)) != null;
        }

        public static Mix FindMix(string _a, string _b)
        {
            return Instance.mixes.Find(x => x.Compare(_a, _b));
        }
    }

    // Class with information of move
    public class Move
    {
        //
        // A -> B
        //

        // position of start chip (A)
        public Vector2Int from;
        // position of target chip (B)
        public Vector2Int to;

        public Solution solution; // solution of this move
        public int potencial; // potential of this move
    }

    [System.Serializable]
    public class Combinations
    {
        public int priority = 0;
        public string chip;
        public bool horizontal = true;
        public bool vertical = true;
        public bool isL = false;
        public bool square = false;
        public int minCount = 4;

    }

    // Class with information of solution
    public class Solution
    {
        //   T
        //   T
        // LLXRR  X - center of solution
        //   B
        //   B

        public int count; // count of chip combination (count = T + L + R + B + X)
        public int potential; // potential of solution
        public int id; // ID of chip color
        public List<Chip> chips = new List<Chip>();

        // center of solution
        public int x;
        public int y;

        public bool v; // is this solution is vertical?  (v = L + R + X >= 3)
        public bool h; // is this solution is horizontal? (h = T + B + X >= 3)
        public bool q;
        public bool L; // is this solution is L shape
    }
    #endregion
    public void StartSession(FieldTarget sessionType, Limitation limitationType)
    {
        StopAllCoroutines(); // Ending of all current coroutines

        isPlaying = true;

        // Start corresponding coroutine depending on the limiation mode
        switch (limitationType)
        {
            case Limitation.Moves: StartCoroutine(MovesLimitation()); break;
            case Limitation.Time: StartCoroutine(TimeLimitation()); break;
        }

        if (LevelProfile.main.HasTarget(FieldTarget.KeyDrop))
        {
            creatingDropsCount = LevelProfile.main.GetTargetCount(0, FieldTarget.KeyDrop);
            creatingDropsCount -= KeyDrop.s_alive_key_count;
        }

        StartCoroutine(TargetSession());

        StartCoroutine(BaseSession()); // Base routine of game session
        StartCoroutine(ShowingHintRoutine()); // Coroutine display hints
        StartCoroutine(ShuffleRoutine()); // Coroutine of mixing chips at the lack moves
        StartCoroutine(FindingSolutionsRoutine()); // Coroutine of finding a solution and destruction of existing combinations of chips
        StartCoroutine(SpreadRoutine());
    }

    IEnumerator BaseSession()
    {
        Debug.Log("Status (Base)" + "Began.");
        while (!limitationRoutineIsOver && !targetRoutineIsOver && !gameForceOver)
        {
            yield return 0;
        }

        Debug.Log("Status (Base)" + "Waiting is over.");

        // Checking the condition of losing
        if (!reachedTheTarget)
        {
            Debug.Log("Status (Base)" + "Session failed. Clearing.");
            yield return StartCoroutine(CameraControl.Instance.HideFieldRoutine());
            FieldManager.Instance.RemoveField();

            ShowLosePopup();
            Debug.Log("Status (Base)" + "Session failed. End.");
            yield break;
        }

        iteraction = false;

        // Debug.Log("Status (Base)" + "Session completed. Waiting the cutscene.");

        Debug.Log("Status (Base)" + "Session completed. Target is reached.");

        // show GUI targetReach popup
        // AUdio play("TargetIsReached");

        Debug.Log("Status (Base)" + "Session completed. Bonus matching.");

        // Conversion of the remaining moves into bombs and activating them
        yield return StartCoroutine(BurnLastMovesToPowerups());

        if (PlayerManager.Instance && !PlayerManager.Instance.m_skipCollapseAllPowerups)
            yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));

        Debug.Log("Status (Base)" + "Session completed. Clearing.");

        // Ending the session, showing win popup
        yield return StartCoroutine(CameraControl.Instance.HideFieldRoutine());
        FieldManager.Instance.RemoveField();
        StartCoroutine(YouWin());

        Debug.Log("Status (Base)" + "Session completed. End.");
    }

    void ShowLosePopup()
    {
        // Audio play ("YouLose");
        isPlaying = false;
        SoundManager.PlaySFX(SoundDefine.k_OutOfMoveSFX);
        CameraControl.Instance.HideField();
        // show GUI GameOver
        if (gameForceOver)
        {
            PopupControl.Instance.OpenPopup("GameOver");
        }
    }

    // Showing win popup
    IEnumerator YouWin()
    {
        // Audio play ("YouWin");
        SoundManager.PlaySFX(SoundDefine.k_GoalGet);
        PlayerPrefs.SetInt("FirstPass", 1);
        isPlaying = false;

        // "life" ++;
        if (PlayerManager.Instance)
        {
            PlayerManager.Instance.m_PlayerLive++;
            //  SetScore (LevelProfile.main.level, score);
            PlayerManager.Instance.m_levelResultScoreArray[PlayerManager.Instance.m_levelSelected] = score;


            if (score > LevelProfile.main.firstStarScore && PlayerManager.Instance.m_levelResultStarsArray[PlayerManager.Instance.m_levelSelected] < 1)
            {
                PlayerManager.Instance.m_levelResultStarsArray[PlayerManager.Instance.m_levelSelected] = 1;
            }
            if (score > LevelProfile.main.secondStarScore && PlayerManager.Instance.m_levelResultStarsArray[PlayerManager.Instance.m_levelSelected] < 2)
            {
                PlayerManager.Instance.m_levelResultStarsArray[PlayerManager.Instance.m_levelSelected] = 2;
            }
            if (score > LevelProfile.main.thirdStarScore && PlayerManager.Instance.m_levelResultStarsArray[PlayerManager.Instance.m_levelSelected] < 3)
            {
                PlayerManager.Instance.m_levelResultStarsArray[PlayerManager.Instance.m_levelSelected] = 3;
            }

            if (PlayerManager.Instance.m_levelSelected < PlayerManager.Instance.m_levelResultScoreArray.Length)
            {
                if (PlayerManager.Instance.m_levelSelected == PlayerManager.Instance.m_currentLevel)
                {
                    PlayerManager.Instance.m_currentLevel++;
                }
            }

            PlayerManager.Instance.SaveGame();
        }

        CameraControl.Instance.HideField();

        yield return 0;

        // Show GUI YouWin
        PopupControl.Instance.OpenPopup("FinishLevel");
        StartCoroutine(CallEventUpdateUIFinishPopup());
        // save profile
        if (PlayerManager.Instance)
        {
            PlayerManager.Instance.SaveGame();
        }
    }

    IEnumerator CallEventUpdateUIFinishPopup()
    {
        yield return new WaitForEndOfFrame();
        FinishLevel.m_updateUIEvent.Invoke();
    }

    // Coroutine of searching solutions and the destruction of existing combinations
    IEnumerator FindingSolutionsRoutine()
    {
        List<Solution> solutions;
        int id = 0;

        while (true)
        {
            if (isPlaying)
            {
                yield return StartCoroutine(Utils.WaitFor(() => lastMovementId > id, 0.2f));

                id = lastMovementId;
                solutions = FindSolutions();
                if (solutions.Count > 0)
                {
                    matchCount++;
                    MatchSolutions(solutions);
                }
                else
                {
                    ///Quyen rem to fix bug "Events resolution during a "cascade""
                    //yield return StartCoroutine(Utils.WaitFor(() =>
                    //{
                    //    return Chip.busyList.Count == 0;
                    //}, 0.1f));
                }
            }
            else
            {
                yield return 0;
            }
        }
    }

    public static void Reset()
    {
        Instance.stars = 0;

        Instance.eventCount = 0;
        Instance.matchCount = 0;
        Instance.lastMovementId = 0;
        Instance.swapEvent = 0;
        Instance.score = 0;
        Instance.firstChipGeneration = true;

        Instance.isPlaying = false;
        Instance.movesCount = LevelProfile.main.limit;
        Instance.timeLeft = LevelProfile.main.limit;

        Instance.reachedTheTarget = false;
        Instance.outOfLimit = false;

        Instance.targetRoutineIsOver = false;
        Instance.limitationRoutineIsOver = false;

        Instance.iteraction = true;
        Instance.gameForceOver = false;

        Butterfly.Cleanup();
        Smoke.Cleanup();
        BookShelfGroup.Cleanup();
        Lamp.Cleanup();
        MagicTapGroup.Cleanup();
        ConveyorTile.Cleanup();
        CandyTreeGroup.Cleanup();
    }
    public void EventCounter()
    {
        eventCount++;
    }

    public bool CanIWait()
    {
        if (PlayerManager.Instance && PlayerManager.Instance.m_skipCollapseAllPowerups)
            return true;
        else
            return isPlaying && Chip.busyList.Count == 0;
    }

    public float GetResource()
    {
        switch (LevelProfile.main.limitation)
        {
            case Limitation.Moves:
                return 1f * movesCount / LevelProfile.main.limit;
            case Limitation.Time:
                return 1f * timeLeft / LevelProfile.main.limit;
        }
        return 1f;
    }

    #region Swap
    public void Swap(Chip a, Chip b)
    {
        if (!a || !b)
            return;
        if (a == b)
            return;
        if (a.slot.block || b.slot.block)
            return;

        a.movementID = GetMovementID();
        b.movementID = GetMovementID();

        Slot slotA = a.slot;
        Slot slotB = b.slot;

        slotB.chip = a;
        slotA.chip = b;
    }

    public void SwapChipToSlot(Chip c, Slot slot)
    {
        if (!c || !slot) return;
        if (slot.chip == c) return;
        if (slot.block) return;

        c.movementID = GetMovementID();
        slot.chip = c;
    }
    public void SwapByPlayer(Chip a, Chip b, bool onlyForMatching, bool byAI = false)
    {
        // Starting corresponding coroutine
        StartCoroutine(SwapByPlayerRoutine(a, b, onlyForMatching, byAI));
    }

    void MixChips(Chip a, Chip b)
    {
        Mix mix = Mix.FindMix(a.chipType, b.chipType);
        if (mix == null)
            return;
        Chip target = null;
        Chip secondary = null;
        if (a.chipType == mix.pair.a)
        {
            target = a;
            secondary = b;
        }
        if (b.chipType == mix.pair.a)
        {
            target = b;
            secondary = a;
        }

        if (target == null)
        {
            Debug.LogError("It can't be mixed, because there is no target chip");
            return;
        }
        b.slot.chip = target;
        secondary.HideChip(false);

        Debug.Log("Has mix::: " + mix.function);
        target.SendMessage(mix.function, secondary);
    }
    IEnumerator SwapByPlayerRoutine(Chip a, Chip b, bool onlyForMatching, bool byAI = false)
    {
        if (!isPlaying)
            yield break;
        if (!iteraction && !byAI)
            yield break;
        // cancellation terms
        if (swaping)
            yield break; // If the process is already running
        if (!a || !b)
            yield break; // If one of the chips is missing
        if (a.destroying || b.destroying)
            yield break;
        if (a.IsBusy || b.IsBusy)
            yield break; // If one of the chips is busy
        if (a.slot.block || b.slot.block)
            yield break; // If one of the chips is blocked
        if (a.logic.GetChipType() == "IceChip" || b.logic.GetChipType() == "IceChip")
            yield break;

        switch (LevelProfile.main.limitation)
        {
            case Limitation.Moves:
                if (movesCount <= 0)
                    yield break;
                break; // If not enough moves
            case Limitation.Time:
                if (Instance.timeLeft <= 0)
                    yield break;
                break; // If not enough time
        }

        Mix mix = mixes.Find(x => x.Compare(a.chipType, b.chipType));

        int move = 0; // Number of points movement which will be expend

        swaping = true;

        Vector3 posA = a.slot.transform.position;
        Vector3 posB = b.slot.transform.position;

        Vector3 posEffect = (posA + posB) / 2;
        StartCoroutine(PlayEffectMoveNoMatch(posEffect));

        float progress = 0;

        Vector3 normal = a.slot.x == b.slot.x ? Vector3.right : Vector3.up;

        float time = 0;

        a.IsBusy = true;
        b.IsBusy = true;

        // Animation swapping 2 chips
        while (progress < CommonConfig.main.swap_duration)
        {
            time = EasingFunctions.easeInOutQuad(progress / CommonConfig.main.swap_duration);
            a.transform.position = Vector3.Lerp(posA, posB, time);// + normal * Mathf.Sin(Mathf.PI * time) * 0.2f;
            if (mix == null)
            {
                b.transform.position = Vector3.Lerp(posB, posA, time);// - normal * Mathf.Sin(Mathf.PI * time) * 0.2f;
            }
            progress += Time.deltaTime;

            yield return 0;
        }

        a.transform.position = posB;
        if (mix == null)
            b.transform.position = posA;

        a.movementID = Instance.GetMovementID();
        b.movementID = Instance.GetMovementID();



        if (mix != null)
        { // Scenario mix effect
            swaping = false;
            a.IsBusy = false;
            b.IsBusy = false;
            MixChips(a, b);
            yield return new WaitForSeconds(0.3f);
            movesCount--;
            swapEvent++;
            if (PostSwapAction != null)
            {
                PostSwapAction();
            }
            yield break;
        }

        // Scenario the effect of swapping two chips
        Slot slotA = a.slot;
        Slot slotB = b.slot;

        slotB.chip = a;
        slotA.chip = b;


        move++;

        // searching for solutions of matching
        int count = 0;
        Solution solution;

        solution = MatchAnaliz(slotA);
        if (solution != null)
            count += solution.count;

        // solution = MatchSquareAnaliz(slotA);
        // if (solution != null)
        //     count += solution.count;

        solution = MatchAnaliz(slotB);
        if (solution != null)
            count += solution.count;

        // solution = MatchSquareAnaliz(slotB);
        // if (solution != null)
        //     count += solution.count;

        // canceling of changing places of chips if no match solution
        if (count == 0 && !onlyForMatching)
        {
            //TODO: Audio play("SwapFailed");
            SoundManager.PlaySFX(SoundDefine.k_blockchange);
            while (progress > 0)
            {
                time = EasingFunctions.easeInOutQuad(progress / CommonConfig.main.swap_duration);
                a.transform.position = Vector3.Lerp(posA, posB, time);// - normal * Mathf.Sin(3.14f * time) * 0.2f;
                b.transform.position = Vector3.Lerp(posB, posA, time);// + normal * Mathf.Sin(3.14f * time) * 0.2f;

                progress -= Time.deltaTime;

                yield return 0;
            }

            a.transform.position = posA;
            b.transform.position = posB;



            a.movementID = GetMovementID();
            b.movementID = GetMovementID();

            slotB.chip = b;
            slotA.chip = a;

            move--;
        }
        else
        {

            //TODO: Play audio("SwapSuccess");
            //SoundManager.PlaySFX(SoundDefine.k_cage);
            swapEvent++;
            if (PostSwapAction != null)
            {
                PostSwapAction();
            }
        }

        firstChipGeneration = false;

        if (!byAI)
            movesCount -= move;
        EventCounter();

        a.IsBusy = false;
        b.IsBusy = false;
        swaping = false;
    }

    IEnumerator PlayEffectMoveNoMatch(Vector3 pos)
    {

        GameObject o_effect = ContentManager.Instance.GetItem("MoveNotMatch");
        ParticleSystem effect = o_effect.GetComponent<ParticleSystem>();

        o_effect.transform.position = pos;
        effect.Play();
        yield return new WaitForSeconds(effect.duration);

        effect.Stop();
        Destroy(o_effect);
    }

    IEnumerator SwapByHintRoutine(Chip a, Chip b, Solution solution, bool byAI = false)
    {
        if (!isPlaying)
            yield break;
        if (!iteraction && !byAI)
            yield break;
        // cancellation terms
        if (swaping)
            yield break; // If the process is already running
        if (!a || !b)
            yield break; // If one of the chips is missing
        if (a.destroying || b.destroying)
            yield break;
        if (a.IsBusy || b.IsBusy)
            yield break; // If one of the chips is busy
        if (a.slot.block || b.slot.block)
            yield break; // If one of the chips is blocked


        switch (LevelProfile.main.limitation)
        {
            case Limitation.Moves:
                if (movesCount <= 0)
                    yield break;
                break; // If not enough moves
            case Limitation.Time:
                if (Instance.timeLeft <= 0)
                    yield break;
                break; // If not enough time
        }

        //Mix mix = mixes.Find(x => x.Compare(a.chipType, b.chipType));

        Vector3 posA = a.slot.transform.position;
        Vector3 posB = b.slot.transform.position;

        float progress = 0;

        Vector3 normal = a.slot.x == b.slot.x ? Vector3.right : Vector3.up;

        float time = 0;

        Chip chipMove = a;
        Chip chipTO = b;

        if (solution != null && solution.chips.Contains(b))
        {
            chipMove = b;
            chipTO = a;
        }

        swaping = true;
        Color originColor = chipTO.slot.transform.Find("bg").GetComponent<SpriteRenderer>().color;

        Vector3 posM = chipMove.slot.transform.position;

        Vector3 newPos = new Vector3();
        newPos.x = (posA.x + posB.x) / 2;
        newPos.y = (posA.y + posB.y) / 2;

        newPos.x = (newPos.x + posM.x) / 2;
        newPos.y = (newPos.y + posM.y) / 2;
        // Animation swapping 2 chips
        while (progress < CommonConfig.main.swap_duration)
        {

            if (!chipMove)
                break;
            time = EasingFunctions.easeInOutQuad(progress / CommonConfig.main.swap_duration);
            chipMove.transform.Find("icon").transform.position = Vector3.Lerp(posM, newPos, time);// + normal * Mathf.Sin(Mathf.PI * time) * 0.2f;
            chipTO.slot.transform.Find("bg").GetComponent<SpriteRenderer>().color = Color.Lerp(originColor, Color.white, time);
            if (solution != null)
            {
                for (int i = 0; i < solution.chips.Count; i++)
                {
                    if (!solution.chips[i].Equals(chipMove))
                    {
                        solution.chips[i].slot.transform.Find("bg").GetComponent<SpriteRenderer>().color = Color.Lerp(originColor, Color.white, time);
                    }
                }
            }
            progress += Time.deltaTime;

            yield return 0;
        }

        while (progress > 0)
        {
            if (!chipMove)
                break;
            time = EasingFunctions.easeInOutQuad(progress / CommonConfig.main.swap_duration);
            chipMove.transform.Find("icon").transform.position = Vector3.Lerp(posM, newPos, time);// - normal * Mathf.Sin(3.14f * time) * 0.2f;
            chipTO.slot.transform.Find("bg").GetComponent<SpriteRenderer>().color = Color.Lerp(originColor, Color.white, time);
            if (solution != null)
            {
                for (int i = 0; i < solution.chips.Count; i++)
                {
                    if (!solution.chips[i].Equals(chipMove))
                    {
                        solution.chips[i].slot.transform.Find("bg").GetComponent<SpriteRenderer>().color = Color.Lerp(originColor, Color.white, time);
                    }
                }
            }
            progress -= Time.deltaTime;

            yield return 0;
        }
        while (progress < CommonConfig.main.swap_duration)
        {
            if (!chipMove)
                break;
            time = EasingFunctions.easeInOutQuad(progress / CommonConfig.main.swap_duration);
            chipMove.transform.Find("icon").transform.position = Vector3.Lerp(posM, newPos, time);
            chipTO.slot.transform.Find("bg").GetComponent<SpriteRenderer>().color = Color.Lerp(originColor, Color.white, time);
            if (solution != null)
            {
                for (int i = 0; i < solution.chips.Count; i++)
                {
                    if (!solution.chips[i].Equals(chipMove))
                    {
                        solution.chips[i].slot.transform.Find("bg").GetComponent<SpriteRenderer>().color = Color.Lerp(originColor, Color.white, time);
                    }
                }
            }
            progress += Time.deltaTime;

            yield return 0;
        }

        while (progress > 0)
        {
            if (!chipMove)
                break;
            time = EasingFunctions.easeInOutQuad(progress / CommonConfig.main.swap_duration);
            chipMove.transform.Find("icon").transform.position = Vector3.Lerp(posM, newPos, time);// - normal * Mathf.Sin(3.14f * time) * 0.2f;
            chipTO.slot.transform.Find("bg").GetComponent<SpriteRenderer>().color = Color.Lerp(originColor, Color.white, time);
            if (solution != null)
            {
                for (int i = 0; i < solution.chips.Count; i++)
                {
                    if (!solution.chips[i].Equals(chipMove))
                    {
                        solution.chips[i].slot.transform.Find("bg").GetComponent<SpriteRenderer>().color = Color.Lerp(originColor, Color.white, time);
                    }
                }
            }
            progress -= Time.deltaTime;

            yield return 0;
        }

        List<Slot> slots = new List<Slot>(Slot.all.Values);
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].transform.Find("bg").GetComponent<SpriteRenderer>().color = originColor;
        }

        swaping = false;
        //EventCounter();
    }

    public int GetMovementID()
    {
        lastMovementId++;
        return lastMovementId;
    }
    #endregion

    #region Analysis
    // Search function possible moves
    public List<Move> FindMoves()
    {
        List<Move> moves = new List<Move>();
        if (!FieldManager.Instance.gameObject.activeSelf)
            return moves;
        if (LevelProfile.main == null)
        {
            return moves;
        }

        Solution solution;
        int potential;

        Side[] asixes = new Side[2] { Side.Right, Side.Top };

        foreach (Side asix in asixes)
        {
            foreach (Slot slot in Slot.all.Values)
            {
                if (slot[asix] == null)
                    continue;
                if (slot.block != null || slot[asix].block != null)
                    continue;
                if (slot.chip == null || slot[asix].chip == null)
                    continue;
                if (slot.chip.id == slot[asix].chip.id)
                    continue;

                Move move = new Move();
                move.from = slot.coord;
                move.to = slot[asix].coord;
                AnalizSwap(move);

                Dictionary<Slot, Solution> solutions = new Dictionary<Slot, Solution>();

                Slot[] cslots = new Slot[2] { slot, slot[asix] };
                foreach (Slot cslot in cslots)
                {
                    solutions.Add(cslot, null);

                    potential = 0;
                    solution = MatchAnaliz(cslot);
                    if (solution != null)
                    {
                        solutions[cslot] = solution;
                        potential = solution.potential;
                    }

                    /* solution = MatchSquareAnaliz(cslot);
                    if (solution != null && potential < solution.potential)
                    {
                        solutions[cslot] = solution;
                        potential = solution.potential;
                    } */

                    move.potencial += potential;
                }

                if (solutions[cslots[0]] != null && solutions[cslots[1]] != null)
                {
                    move.solution = solutions[cslots[0]].potential > solutions[cslots[1]].potential ? solutions[cslots[0]] : solutions[cslots[1]];
                }
                else
                {
                    move.solution = solutions[cslots[0]] != null ? solutions[cslots[0]] : solutions[cslots[1]];
                }

                AnalizSwap(move);

                if (Mix.ContainsThisMix(slot.chip.chipType, slot[asix].chip.chipType))
                {
                    move.potencial += 100;
                }
                if (move.potencial > 0)
                {
                    moves.Add(move);
                }
            }
        }

        return moves;
    }
    void AnalizSwap(Move move)
    {
        Slot slot;
        Chip fChip = Slot.GetSlot(move.from).chip;
        Chip tChip = Slot.GetSlot(move.to).chip;
        if (!fChip || !tChip) return;
        slot = tChip.slot;
        fChip.slot.chip = tChip;
        slot.chip = fChip;
    }
    ////////////////////////////////////////////////////////////////////////////////
    // Analysis of chip for combination
    public Solution MatchAnaliz(Slot slot)
    {

        if (!slot.chip)
            return null;
        if (!slot.chip.IsMatchable())
            return null;


        //if (slot.chip.IsUniversalColor())
        //{ // multicolor
        //    List<Solution> solutions = new List<Solution>();
        //    Solution z;
        //    Chip multicolorChip = slot.chip;
        //    for (int i = 0; i < 6; i++)
        //    {
        //        multicolorChip.id = i;
        //        z = MatchAnaliz(slot);
        //        if (z != null)
        //        {
        //            solutions.Add(z);
        //        }
        //        // z = MatchSquareAnaliz(slot);
        //        // if (z != null)
        //        //     solutions.Add(z);
        //    }
        //    multicolorChip.id = Chip.universalColorId;
        //    z = null;
        //    foreach (Solution sol in solutions)
        //        if (z == null || z.potential < sol.potential)
        //            z = sol;
        //    return z;
        //}

        Slot s;
        Dictionary<Side, List<Chip>> sides = new Dictionary<Side, List<Chip>>();
        int count;
        Vector2Int key;
        foreach (Side side in Utils.straightSides)
        {
            count = 1;
            sides.Add(side, new List<Chip>());
            while (true)
            {
                key = slot.coord + Utils.GetSideOffset(side) * count;
                if (!Slot.all.ContainsKey(key))
                    break;
                s = Slot.all[key];
                if (!s.chip)
                    break;
                if (s.chip.id != slot.chip.id && !s.chip.IsUniversalColor())
                    break;
                if (!s.chip.IsMatchable())
                    break;
                sides[side].Add(s.chip);
                count++;
            }
        }

        bool h = sides[Side.Right].Count + sides[Side.Left].Count >= 2;
        bool v = sides[Side.Top].Count + sides[Side.Bottom].Count >= 2;

        if (h || v)
        {
            Solution solution = new Solution();

            solution.h = h;
            solution.v = v;

            solution.chips = new List<Chip>();
            solution.chips.Add(slot.chip);

            if (h)
            {
                solution.chips.AddRange(sides[Side.Right]);
                solution.chips.AddRange(sides[Side.Left]);
            }
            if (v)
            {
                solution.chips.AddRange(sides[Side.Top]);
                solution.chips.AddRange(sides[Side.Bottom]);
            }

            solution.count = solution.chips.Count;

            solution.x = slot.x;
            solution.y = slot.y;
            solution.id = slot.chip.id;

            foreach (Chip c in solution.chips)
            {
                solution.potential += c.GetPotencial(); //update potential
            }

            //check if solution is L
            bool hasLeft = sides[Side.Left].Count > 1;
            bool hasRight = sides[Side.Right].Count > 1;
            bool hasTop = sides[Side.Top].Count > 1;
            bool hasBottom = sides[Side.Bottom].Count > 1;
            if ((hasLeft && !hasRight || hasRight && !hasLeft)
                && (hasTop && !hasBottom || hasBottom && !hasTop))
            {
                solution.L = true;
            }

            return solution;
        }
        return null;
    }

    public Solution MatchSquareAnaliz(Slot slot)
    {

        if (!Instance.squareCombination)
            return null;
        if (!slot.chip)
            return null;
        if (!slot.chip.IsMatchable())
            return null;


        if (slot.chip.IsUniversalColor())
        { // multicolor
            List<Solution> solutions = new List<Solution>();
            Solution z;
            Chip multicolorChip = slot.chip;
            for (int i = 0; i < 6; i++)
            {
                multicolorChip.id = i;
                z = MatchSquareAnaliz(slot);
                if (z != null)
                    solutions.Add(z);
            }
            multicolorChip.id = Chip.universalColorId;
            z = null;
            foreach (Solution sol in solutions)
                if (z == null || z.potential < sol.potential)
                    z = sol;
            return z;
        }

        List<Chip> square = new List<Chip>();
        List<Chip> buffer = new List<Chip>();
        Side sideR;
        Vector2Int key;
        Slot s;

        buffer.Clear();
        foreach (Side side in Utils.straightSides)
        {
            for (int r = 0; r <= 2; r++)
            {
                sideR = Utils.RotateSide(side, r);
                key = slot.coord + Utils.GetSideOffset(sideR);
                if (Slot.all.ContainsKey(key))
                {
                    s = Slot.all[key];
                    if (s.chip && (s.chip.id == slot.chip.id || s.chip.IsUniversalColor()) && s.chip.IsMatchable())
                        buffer.Add(s.chip);
                    else
                        break;
                }
                else
                    break;
            }
            if (buffer.Count == 3)
            {
                foreach (Chip chip_b in buffer)
                    if (!square.Contains(chip_b))
                        square.Add(chip_b);
            }
            buffer.Clear();
        }


        bool q = square.Count >= 3;

        if (q)
        {
            Solution solution = new Solution();

            solution.q = q;

            solution.chips = new List<Chip>();
            solution.chips.Add(slot.chip);

            solution.chips.AddRange(square);

            solution.count = solution.chips.Count;

            solution.x = slot.x;
            solution.y = slot.y;
            solution.id = slot.chip.id;

            foreach (Chip c in solution.chips)
            {
                // update potential
                solution.potential += c.GetPotencial();
            }

            return solution;
        }
        return null;
    }
    #endregion

    #region Limit mode
    IEnumerator TimeLimitation()
    {

        outOfLimit = false;

        // Waiting until the rules of the game are carried out
        while (timeLeft > 0 && !targetRoutineIsOver)
        {
            if (Time.timeScale == 1)
            {
                timeLeft -= 1f;
            }
            timeLeft = Mathf.Max(timeLeft, 0);
            if (timeLeft <= 5)
            {
                // TODO: Audio play("TimeWarrning");
            }
            yield return new WaitForSeconds(1f);

            if (timeLeft <= 0)
            {
                Debug.Log("Status (Limitation)" + "Out of limit. Waiting for destroying.");
                do
                {
                    yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));
                }
                while (Slot.HasDestroyingChip());

                if (!reachedTheTarget)
                {
                    // TODO: show Menu no more time
                    // Audio play("NoMoreMoves");
                    wait = true;
                    // Pending the decision of the player - lose or purchase additional time
                    Debug.Log("Status (Limitation)" + "Out of limit. Extra resources offer.");
                    while (wait)
                    {
                        yield return new WaitForSeconds(0.5f);
                    }

                }
            }
        }

        Debug.Log("Status (Limitation)" + "Waiting is over.");

        yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));

        if (timeLeft <= 0) outOfLimit = true;

        limitationRoutineIsOver = true;

        Debug.Log("Status (Limitation)" + "End. Out of limit: " + outOfLimit);
    }

    IEnumerator MovesLimitation()
    {
        outOfLimit = false;

        // Waiting until the rules of the game are carried out
        while (movesCount > 0)
        {
            yield return new WaitForSeconds(1f);
            if (movesCount <= 0)
            {
                Debug.Log("Status (Limitation)" + "Out of limit. Waiting for destoying.");
                do
                {
                    yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));
                }
                while (Slot.HasDestroyingChip());

                if (!reachedTheTarget)
                {
                    // show Menu no more Move
                    PopupControl.Instance.OpenPopup("Buy5Move");
                    // Audio play("NoMoreMoves");
                    SoundManager.PlaySFX(SoundDefine.k_OutOfMoveSFX);
                    wait = true;
                    // Pending the decision of the player - lose or purchase additional time
                    Debug.Log("Status (Limitation)" + "Out of limit. Extra resources offer.");
                    while (wait)
                    {
                        yield return new WaitForSeconds(0.5f);
                    }

                }
            }
        }

        Debug.Log("Status (Limitation)" + "Waiting is over.");

        yield return StartCoroutine(Utils.WaitFor(CanIWait, 1f));

        outOfLimit = true;
        limitationRoutineIsOver = true;

        Debug.Log("Status (Limitation)" + "End. Out of limit: " + outOfLimit);
    }

    WaitForSeconds m_checkTargetWait = new WaitForSeconds(0.33f);
    IEnumerator TargetSession()
    {
        reachedTheTarget = false;
        foreach (var target in LevelProfile.main.allTargets)
        {
            Debug.Log("Level target: " + target.ToString());
        }
        yield return 0;

        while (!outOfLimit && !reachedTheTarget)
        {
            //update need targets
            foreach (var target in LevelProfile.main.allTargets)
            {
                switch (target.Type)
                {
                    case FieldTarget.Stone:
                        {
                            int targetCount = target.GetTargetCount(0);
                            int currentStone = BackLayer.s_allStones.Count;
                            target.SetCurrentCount(0, targetCount - currentStone);
                        }
                        break;
                }
            }

            bool isReach = true;
            foreach (var target in LevelProfile.main.allTargets)
            {
                if (!target.IsReachTarget())
                {
                    isReach = false;
                    break;
                }
            }
            reachedTheTarget = isReach;
            yield return m_checkTargetWait;
        }

        //in case of out of limit
        bool isReachAll = true;
        foreach (var target in LevelProfile.main.allTargets)
        {
            if (!target.IsReachTarget())
            {
                isReachAll = false;
                break;
            }
        }

        reachedTheTarget = isReachAll;

        targetRoutineIsOver = true;

    }
    #endregion

    #region Game flow coroutines
    IEnumerator BurnLastMovesToPowerups()
    {
        //yield return StartCoroutine(CollapseAllPowerups());
        if (PlayerManager.Instance && !PlayerManager.Instance.m_skipCollapseAllPowerups)
        {
            GameplayPanel.m_eventPlayAnim.Invoke();
            yield return new WaitForSeconds(1f);

            int newBombs = 0;
            switch (LevelProfile.main.limitation)
            {
                case Limitation.Moves: newBombs = movesCount; break;
                case Limitation.Time: newBombs = Mathf.CeilToInt(timeLeft / 3); break;
            }

            int count;

            PlayerManager.Instance.m_canSkipCollapseAllPowerups = true;

            while (newBombs > 0)
            {
                count = Mathf.Min(newBombs, 100);
                while (count > 0)
                {
                    count--;
                    newBombs--;
                    movesCount--;
                    timeLeft -= 3;
                    timeLeft = Mathf.Max(timeLeft, 0);
                    switch (Random.Range(0, 2))
                    {
                        case 0: FieldManager.Instance.AddPowerup("SimpleBomb"); break;
                        case 1: FieldManager.Instance.AddPowerup("CrossBomb"); break;
                    }
                    yield return new WaitForSeconds(0.1f);
                }
                yield return StartCoroutine(Utils.WaitFor(CanIWait, 0.5f));
                yield return StartCoroutine(CollapseAllPowerups());
            }
        }
    }

    // Coroutine of activation all bombs in playing field
    IEnumerator CollapseAllPowerups()
    {
        yield return StartCoroutine(Utils.WaitFor(CanIWait, 0.5f));
        List<Chip> powerUp = FindPowerups();
        while (powerUp.Count > 0)
        {
            if (PlayerManager.Instance.m_skipCollapseAllPowerups)
                break;
            powerUp = powerUp.FindAll(x => !x.destroying);
            if (powerUp.Count > 0)
            {
                EventCounter();
                Chip pu = powerUp[Random.Range(0, powerUp.Count)];
                pu.DestroyChip();
            }
            yield return StartCoroutine(Utils.WaitFor(CanIWait, 0.5f));
            powerUp = FindPowerups();
        }
        //yield return StartCoroutine(Utils.WaitFor(CanIWait, 0.5f));
    }

    // Finding bomb function
    List<Chip> FindPowerups()
    {
        var bombs = FindObjectsOfType<IBomb>();
        List<Chip> chips = new List<Chip>();
        for (int i = 0; i < bombs.Length; i++)
        {
            Chip chip = bombs[i].GetComponent<Chip>();
            if (chip)
            {
                chips.Add(chip);
            }
        }

        if (PlayerManager.Instance.m_skipCollapseAllPowerups)
        {
            chips.Clear();
        }

        return chips;
    }

    // Coroutine of showing hints
    IEnumerator ShowingHintRoutine()
    {
        int hintOrder = 0;
        float delay = 5;

        yield return new WaitForSeconds(1f);

        while (!reachedTheTarget)
        {
            while (!isPlaying)
                yield return 0;
            yield return StartCoroutine(Utils.WaitFor(CanIWait, delay));
            //if (eventCount > hintOrder)
            {
                //hintOrder = eventCount;
                ShowHint();
            }
        }
    }

    // Showing random hint
    void ShowHint()
    {
        if (!isPlaying) return;
        List<Move> moves = FindMoves();

        foreach (Move move in moves)
        {
            Debug.DrawLine(Slot.GetSlot(move.from).transform.position, Slot.GetSlot(move.to).transform.position, Color.red, 10);
        }

        if (moves.Count == 0) return;

        Move bestMove = moves[Random.Range(0, moves.Count)];

        StartCoroutine(SwapByHintRoutine(Slot.GetSlot(bestMove.from).chip, Slot.GetSlot(bestMove.to).chip, bestMove.solution, true));
        //if (bestMove.solution == null) return;

        //foreach (Chip chip in bestMove.solution.chips)
        //{
        //    chip.Flashing(eventCount);
        //}
    }

    IEnumerator ShuffleRoutine()
    {
        int shuffleOrder = 0;
        float delay = 1f;
        while (true)
        {
            yield return StartCoroutine(Utils.WaitFor(CanIWait, delay));
            if ( !targetRoutineIsOver && Chip.busyList.Count == 0)
            {
                //shuffleOrder = eventCount;
                yield return StartCoroutine(Shuffle(false));
            }
        }
    }

    public void ForceSuffle()
    {
        StartCoroutine(Shuffle(true));
    }

    void RawShuffle(List<Slot> slots)
    {
        EventCounter();
        int targetID;
        for (int j = 0; j < slots.Count; j++)
        {
            targetID = Random.Range(0, j - 1);
            if (!slots[j].chip || !slots[targetID].chip)
                continue;
            if (!slots[j].chip.CanSuffle() || !slots[targetID].chip.CanSuffle())
                continue;
            Swap(slots[j].chip, slots[targetID].chip);
        }
    }
    public IEnumerator Shuffle(bool f)
    {
        bool force = f;


        List<Move> moves = FindMoves();
        if (moves.Count > 0 && !force)
        {
            yield break;
        }
        if (!isPlaying)
        {
            yield break;
        }

        isPlaying = false;

        List<Slot> slots = new List<Slot>(Slot.all.Values);

        //Dictionary<Slot, Vector3> positions = new Dictionary<Slot, Vector3>();
        //foreach (Slot slot in slots)
        //    positions.Add(slot, slot.transform.position);

        GameplayPanel.m_eventShuffAnim.Invoke();

        float t = 0;
        while (t < 1)
        {
            t += Time.unscaledDeltaTime * 3;
            Slot.folder.transform.localScale = Vector3.one * Mathf.Lerp(1, 0.6f, EasingFunctions.easeInOutQuad(t));
            Slot.folder.transform.eulerAngles = Vector3.forward * Mathf.Lerp(0, Mathf.Sin(Time.unscaledTime * 40) * 3, EasingFunctions.easeInOutQuad(t));

            yield return 0;
        }


        if (f || moves.Count == 0)
        {
            f = false;
            RawShuffle(slots);
        }

        moves = FindMoves();
        List<Solution> solutions = FindSolutions();

        int itrn = 0;
        int targetID;
        while (solutions.Count > 0 || moves.Count == 0)
        {
            if (itrn > 100) // try 100 times
            {
                ShowLosePopup();
                yield break;
            }
            if (solutions.Count > 0)
            {
                for (int s = 0; s < solutions.Count; s++)
                {
                    targetID = Random.Range(0, slots.Count - 1);
                    if (slots[targetID].chip && slots[targetID].chip.chipType != "Key" && slots[targetID].chip.id != solutions[s].id)
                    {
                        Swap(solutions[s].chips[Random.Range(0, solutions[s].chips.Count - 1)], slots[targetID].chip);
                    }
                }
            }
            else
            {
                RawShuffle(slots);
            }

            moves = FindMoves();
            solutions = FindSolutions();
            itrn++;
            Slot.folder.transform.eulerAngles = Vector3.forward * Mathf.Sin(Time.unscaledTime * 40) * 3;

            yield return 0;
        }

        t = 0;
        // Audio play("Shuffle");
        SoundManager.PlaySFX(SoundDefine.k_shuffle);
        while (t < 1)
        {
            t += Time.unscaledDeltaTime * 3;
            Slot.folder.transform.localScale = Vector3.one * Mathf.Lerp(0.6f, 1, EasingFunctions.easeInOutQuad(t));
            Slot.folder.transform.eulerAngles = Vector3.forward * Mathf.Lerp(Mathf.Sin(Time.unscaledTime * 40) * 3, 0, EasingFunctions.easeInOutQuad(t));
            yield return 0;
        }

        Slot.folder.transform.localScale = Vector3.one;
        Slot.folder.transform.eulerAngles = Vector3.zero;

        GameplayPanel.m_eventShuffAnim.Invoke();

        isPlaying = true;
    }

    List<Solution> FindSolutions()
    {
        List<Solution> solutions = new List<Solution>();
        Solution zsolution;
        foreach (Slot slot in Slot.all.Values)
        {
            zsolution = MatchAnaliz(slot);
            if (zsolution != null) solutions.Add(zsolution);
            // zsolution = MatchSquareAnaliz(slot);
            // if (zsolution != null) solutions.Add(zsolution);
        }
        return solutions;
    }

    void MatchSolutions(List<Solution> solutions)
    {
        if (!isPlaying) return;
        solutions.Sort(delegate (Solution x, Solution y)
        {
            if (x.potential == y.potential)
                return 0;
            else if (x.potential > y.potential)
                return -1;
            else
                return 1;
        });

        int width = LevelProfile.main.width;
        int height = LevelProfile.main.height;

        bool[,] mask = new bool[width, height];
        Vector2Int key = new Vector2Int();
        Slot slot;

        for (key.x = 0; key.x < width; key.x++)
            for (key.y = 0; key.y < height; key.y++)
            {
                mask[key.x, key.y] = false;
                if (Slot.all.ContainsKey(key))
                {
                    slot = Slot.all[key];
                    if (slot.chip)
                    {
                        mask[key.x, key.y] = true;
                    }
                }
            }

        List<Solution> final_solutions = new List<Solution>();

        bool breaker;
        foreach (Solution s in solutions)
        {
            breaker = false;
            foreach (Chip c in s.chips)
            {
                if (!mask[c.slot.x, c.slot.y])
                {
                    breaker = true;
                    break;
                }
            }
            if (breaker)
                continue;

            final_solutions.Add(s);

            foreach (Chip c in s.chips)
            {
                mask[c.slot.x, c.slot.y] = false;
            }
        }

        foreach (Solution solution in final_solutions)
        {
            EventCounter();

            int puID = -1;

            if (solution.chips.FindAll(x => !x.IsMatchable()).Count == 0)
            {
                foreach (Chip chip in solution.chips)
                {
                    if (chip.id == solution.id || chip.IsUniversalColor())
                    {
                        if (!chip.slot)
                            continue;

                        slot = chip.slot;

                        if (chip.movementID > puID)
                        {
                            puID = chip.movementID;
                        }

                        chip.SetScore(Mathf.Pow(2, solution.count - 3) / solution.count);
                        if (!slot.block)
                        {
                            FieldManager.Instance.BlockCrush(slot.coord, true);
                        }
                        chip.DestroyChip();

                        if (slot.stone)
                        {
                            slot.stone.StoneCrush();
                        }
                    }
                }
            }
            else
                return;

            solution.chips.Sort(delegate (Chip a, Chip b)
            {
                return a.movementID > b.movementID ? -1 : a.movementID == b.movementID ? 0 : 1;
            });

            breaker = false;
            foreach (Combinations combination in combinations)
            {
                if (combination.square && !solution.q)
                    continue;
                if (!combination.square)
                {
                    if (combination.horizontal && !solution.h)
                        continue;
                    if (combination.vertical && !solution.v)
                        continue;
                    if (combination.minCount > solution.count)
                        continue;
                    if (combination.isL && !solution.L || !combination.isL && solution.L)
                        continue;
                }

                foreach (Chip ch in solution.chips)
                {
                    if (ch.slot != null && ch.chipType != "Portion")
                    {
                        Debug.Log(ch.chipType);
                        FieldManager.Instance.GetNewBomb(ch.slot.coord, combination.chip, ch.slot.transform.position, solution.id);
                        breaker = true;
                        break;
                    }
                }
                if (breaker)
                {
                    break;
                }
            }
        }
    }

    IEnumerator SpreadRoutine()
    {
        Smoke.seed = 0;

        int last_swapEvent = swapEvent;

        yield return new WaitForSeconds(1f);

        while (isPlaying)
        {
            yield return new WaitUntil(() => Smoke.all.Count > 0);
            yield return StartCoroutine(Utils.WaitFor(() => swapEvent > last_swapEvent, 0.1f));
            last_swapEvent = swapEvent;
            yield return StartCoroutine(Utils.WaitFor(CanIWait, 0.15f));
            if (Smoke.lastSmokeCrush < swapEvent)
            {
                // how many swap, how many smoke spread
                Smoke.seed += swapEvent - Smoke.lastSmokeCrush;
                Smoke.lastSmokeCrush = swapEvent;
            }
            Smoke.Spread();
        }
    }
    #endregion

    #region Targets
    // index > 0 use for color Target, blocker
    public int GetCurrentCountOfTarget(FieldTarget targetType, int index = 0)
    {
        if (LevelProfile.main.HasTarget(targetType))
        {
            LevelTarget target = LevelProfile.main.allTargets.Find(a => a.Type == targetType);

            return target.GetCurrentCount(index);
        }
        return -1;
    }

    public void IncreaseCountColor(int colorId)
    {
        if (LevelProfile.main.HasTarget(FieldTarget.Color))
        {
            LevelProfile.main.IncreaseTargetProgress(FieldTarget.Color, colorId, 1);
        }
    }

    public void CollectKey()
    {
        if (LevelProfile.main.HasTarget(FieldTarget.KeyDrop))
        {
            LevelProfile.main.IncreaseTargetProgress(FieldTarget.KeyDrop, 0, 1);
        }
    }

    public void CollectCompass()
    {
        if (LevelProfile.main.HasTarget(FieldTarget.Blocker))
        {
            LevelProfile.main.IncreaseTargetProgress(FieldTarget.Blocker, (int)BlockerTargetType.Compass, 1);
        }
    }

    public void CollectBook()
    {
        if (LevelProfile.main.HasTarget(FieldTarget.FixBlock))
        {
            LevelProfile.main.IncreaseTargetProgress(FieldTarget.FixBlock, 0, 1);
        }
    }

    public void CollectCaged()
    {
        if (LevelProfile.main.HasTarget(FieldTarget.Cage))
        {
            LevelProfile.main.IncreaseTargetProgress(FieldTarget.Cage, 0, 1);
        }
    }

    public void CollectColorBook()
    {
        if (LevelProfile.main.HasTarget(FieldTarget.ColorBlocker))
        {
            LevelProfile.main.IncreaseTargetProgress(FieldTarget.ColorBlocker, 0, 1);
        }
    }

    public void CollectScroll()
    {
        if (LevelProfile.main.HasTarget(FieldTarget.MagicScroll))
        {
            LevelProfile.main.IncreaseTargetProgress(FieldTarget.MagicScroll, 0, 1);
        }
    }

    public void CollectButterfly()
    {
        if (LevelProfile.main.HasTarget(FieldTarget.Butterfly))
        {
            LevelProfile.main.IncreaseTargetProgress(FieldTarget.Butterfly, 0, 1);
        }
    }
    public void CollectCurtain()
    {
        if (LevelProfile.main.HasTarget(FieldTarget.Curtain))
        {
            LevelProfile.main.IncreaseTargetProgress(FieldTarget.Curtain, 0, 1);
        }
    }

    public void CollectCrystal()
    {
        if (LevelProfile.main.HasTarget(FieldTarget.Blocker))
        {
            LevelProfile.main.IncreaseTargetProgress(FieldTarget.Blocker, (int)BlockerTargetType.Crystal, 1);
        }
    }
    public void CollectSmokePot()
    {
        if (LevelProfile.main.HasTarget(FieldTarget.Blocker))
        {
            LevelProfile.main.IncreaseTargetProgress(FieldTarget.Blocker, (int)BlockerTargetType.SmokePot, 1);
        }
    }
    public void CollectRandomChanger()
    {
        if (LevelProfile.main.HasTarget(FieldTarget.RandomChanger))
        {
            LevelProfile.main.IncreaseTargetProgress(FieldTarget.RandomChanger, 0, 1);
        }
    }

    public void CollectMetal()
    {
        if (LevelProfile.main.HasTarget(FieldTarget.MetalBrick))
        {
            LevelProfile.main.IncreaseTargetProgress(FieldTarget.MetalBrick, 0, 1);
        }
    }

    public void CollectIce()
    {
        if (LevelProfile.main.HasTarget(FieldTarget.IceBrick))
        {
            LevelProfile.main.IncreaseTargetProgress(FieldTarget.IceBrick, 0, 1);
        }
    }
    #endregion
}