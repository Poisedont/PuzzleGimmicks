//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokePot : MonoBehaviour, IChipLogic, IChipAffectByNeighBor
{
    Chip m_chip;
    public Chip chip { get { return m_chip; } }

    public Chip NeighborChip { get; set; }

    int m_eventBirth;
    ////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        m_chip = GetComponent<Chip>();
    }
    private void Start()
    {
        m_eventBirth = Session.Instance.eventCount;
    }
    public IEnumerator Destroying()
    {
        if (m_eventBirth == Session.Instance.eventCount)
        {
            chip.destroying = false;
            yield break;
        }

        GameObject Obj = ContentManager.Instance.GetItem("SmokeBlue");
        ParticleSystem blueEffects = Obj.GetComponent<ParticleSystem>();
        Obj.transform.position = transform.position;
        blueEffects.Play();

        chip.IsBusy = false;
        SpawnSmoke();
        chip.ParentRemove();

        //yield return new WaitForSeconds(blueEffects.duration);
        

        int ptarget = LevelProfile.main.GetTargetCount((int)BlockerTargetType.SmokePot, FieldTarget.Blocker);

        if (ptarget > 0)
        {
            TargetUI[] listO = GameObject.FindObjectsOfType<TargetUI>();

            TargetUI go = null;

            for (int i = 0; i < listO.Length; i++)
            {
                if (listO[i].GetLvlTarget().GetTarget().Equals(FieldTarget.Blocker))
                {
                    if (listO[i].GetLvlTarget().GetTargetCount((int)BlockerTargetType.SmokePot) > 0
                    && listO[i].GetIndexTaret() == (int)BlockerTargetType.SmokePot
                    && listO[i].GetLvlTarget().GetTargetCount((int)BlockerTargetType.SmokePot) > listO[i].GetLvlTarget().GetCurrentCount((int)BlockerTargetType.SmokePot)
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

        }
        else
        {
            yield return new WaitForSeconds(blueEffects.duration);
        }

        blueEffects.Stop();
        Destroy(blueEffects.gameObject);
        Destroy(Obj);

        Session.Instance.CollectSmokePot();
    }

    private void SpawnSmoke()
    {
        FieldManager.Instance.GetNewSmoke(this.chip.slot.coord);
        Smoke.lastSmokeCrush = Session.Instance.swapEvent; //prevent smoke spread
        SlotGravity.Reshading();
    }

    public string GetChipType()
    {
        return EPieces.SmokePot.ToString();
    }

    public List<Chip> GetDangeredChips(List<Chip> stack)
    {
        return stack;
    }

    public int GetPotencial()
    {
        return 0;
    }

    public bool IsMatchable()
    {
        return false;
    }

    public bool IsCanEffectedByNeighbor()
    {
        return true;
    }
}