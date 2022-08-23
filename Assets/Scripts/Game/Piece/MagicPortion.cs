using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicPortion : MonoBehaviour, IChipLogic
{
    protected Chip m_chip;
    public Chip chip
    {
        get { return m_chip; }
    }

    int m_eventBirth;
    public int MagicRule { get; set; }

    public static int s_portionCount = 0;
    public static int s_generatorEvent = 0;
    ////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        m_chip = GetComponent<Chip>();
        m_eventBirth = Session.Instance.eventCount;
        s_portionCount++;
    }
    public IEnumerator Destroying()
    {
        if (m_eventBirth == Session.Instance.eventCount)
        {
            chip.destroying = false;
            yield break;
        }

        chip.IsBusy = true;
        --s_portionCount;

        transform.FindChild("icon").gameObject.SetActive(false);
        GameObject o = ContentManager.Instance.GetItem("SmokeInSlot");
        o.transform.position = transform.position;
        ParticleSystem effect = o.GetComponent<ParticleSystem>();
        effect.Play();
        yield return new WaitForSeconds(effect.duration);
        effect.Stop();
        Destroy(o.gameObject);

        SpawnRandomObject();

        int ptarget = LevelProfile.main.GetTargetCount(0, FieldTarget.RandomChanger);

        if (ptarget > 0)
        {
            TargetUI[] listO = GameObject.FindObjectsOfType<TargetUI>();

            TargetUI go = null;

            for (int i = 0; i < listO.Length; i++)
            {
                if (listO[i].GetLvlTarget().GetTarget().Equals(FieldTarget.RandomChanger))
                {
                    if (listO[i].GetLvlTarget().GetTargetCount(0) > 0
                    && listO[i].GetIndexTaret() == 0
                    && listO[i].GetLvlTarget().GetTargetCount(0) > listO[i].GetLvlTarget().GetCurrentCount(0)
                    )
                    {
                        go = listO[i];
                    }
                }
            }

            if (go)
            {
                Transform target = go.transform;

                //sprite.sortingLayerName = "UI";
                //sprite.sortingOrder = 10;

                float time = 0;
                float speed = Random.Range(1f, 1.8f);
                Vector3 startPosition = transform.position;
                Vector3 targetPosition = target.position;

                while (time < 1)
                {
                    transform.position = Vector3.Lerp(startPosition, targetPosition, EasingFunctions.easeInOutQuad(time));
                    time += Time.unscaledDeltaTime * speed;
                    yield return 0;
                }

                transform.position = target.position;
            }
            Session.Instance.CollectRandomChanger();
        }

        
    }

    public string GetChipType()
    {
        return "Portion";
    }

    public List<Chip> GetDangeredChips(List<Chip> stack)
    {
        stack.Add(chip);
        return stack;
    }

    public int GetPotencial()
    {
        return 1;
    }

    public bool IsMatchable()
    {
        return true;
    }
    ////////////////////////////////////////////////////////////////////////////////
    void SpawnRandomObject()
    {
        RandomChangerConfig.ChangerInfo info = LevelProfile.main.randomChangerConfig.GetRandomObject();
        string item = info.Item;
        // item = "Compass";
        Debug.Log("Generate item : " + item);

        switch (item)
        {
            case "Compass":
                {
                    FieldManager.Instance.GetNewBlockerChip(this.chip.slot.coord, this.chip.slot.transform.position, info.Level);
                }
                break;
            case "Butterfly":
                {
                    FieldManager.Instance.GetNewButterfly(this.chip.slot.coord, this.chip.slot.transform.position);
                }
                break;
            case "magicBook":
                {
                    int level = info.Level;
                    FieldManager.Instance.GetNewFixBlockerChip(this.chip.slot.coord, this.chip.slot.transform.position, level);
                    SlotGravity.Reshading();
                }
                break;
            case "cage":
                {
                    Vector2Int coord = this.chip.slot.coord;
                    FieldManager.Instance.GetNewSimpleChip(coord, this.chip.slot.transform.position);
                    FieldManager.Instance.GetNewCageBlock(coord);
                    SlotGravity.Reshading();
                }
                break;
            case "Curtain":
                {
                    FieldManager.Instance.GetNewCurtain(this.chip.slot.coord, info.Level);
                    SlotGravity.Reshading();
                }
                break;
        }
    }
}